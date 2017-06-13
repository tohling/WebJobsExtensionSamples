// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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

            rule.BindToInput<byte[]>(BuildItemFromAttr);
        }

        private JObject ConvertToJObject(AnalysisResult result)
        {
            return JObject.FromObject(result);
        }

        private string ConvertToString(AnalysisResult result)
        {
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private byte[] BuildItemFromAttr(ImageThumbnailAttribute attribute)
        {
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
            byte[] result = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                result = visionServiceClient.GetThumbnailAsync(attribute.ImageUrl, width, height, smartCropping).Result;
            }
            else if (attribute.ImageStream != null)
            {
                result = visionServiceClient.GetThumbnailAsync(attribute.ImageStream, width, height, smartCropping).Result;
            }
            else
            {
                throw new InvalidOperationException("Missing image url or stream.");
            }
            return result;
        }
    }
}
