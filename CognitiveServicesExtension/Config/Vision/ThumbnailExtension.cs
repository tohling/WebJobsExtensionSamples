// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.ProjectOxford.Vision;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace CognitiveServicesExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="ImageAnalyzerAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="byte[]"/> 
    /// </summary>
    public class ThumbnailExtension : IExtensionConfigProvider
    {
        private static VisionServiceClient visionServiceClient;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="ThumbNailAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 

            var rule = context.AddBindingRule<ImageThumbnailAttribute>();

            rule.BindToInput<string>(BuildItemFromAttr);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private string BuildItemFromAttr(ImageThumbnailAttribute attribute)
        {
            string result = null;
            int width = 1;
            if(int.TryParse(attribute.Width, out width) == false)
            {
                throw new InvalidOperationException("Invalid width. Valid range >= 1");
            }

            int height = 1;
            if (int.TryParse(attribute.Height, out height) == false)
            {
                throw new InvalidOperationException("Invalid height. Valid range >= 1");
            }

            bool smartCropping = true;
            if (!string.IsNullOrEmpty(attribute.SmartCropping) && attribute.SmartCropping.Equals("false", System.StringComparison.InvariantCultureIgnoreCase))
            {
                smartCropping = false;
            }

            visionServiceClient = new VisionServiceClient(attribute.SubscriptionKey);

            byte[] thumbnail = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                thumbnail = visionServiceClient.GetThumbnailAsync(attribute.ImageUrl, width, height, smartCropping).Result;

                result = this.UploadThumbnailAsync(attribute, thumbnail).Result; 
            }
            else
            {
                throw new InvalidOperationException("Missing image url.");
            }
            return result;
        }

        public async Task<string> UploadThumbnailAsync(ImageThumbnailAttribute attribute, byte[] thumbnail)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(attribute.Connection);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(attribute.BlobContainerName);

            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }
                    );
            }

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(attribute.BlobName);
            await cloudBlockBlob.UploadFromByteArrayAsync(thumbnail, 0, thumbnail.Length);

            return cloudBlockBlob.Uri.ToString();
        }
    }
}
