// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.Text;

namespace CognitiveService.Config
{
    /// <summary>
    /// Extension for binding <see cref="ImageToTextAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="OcrResults"/> 
    /// </summary>
    public class ImageToTextExtension : IExtensionConfigProvider
    {
        const string DefaultLanguage = "unk";

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="ImageToTextAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 

            // This is useful on input. 
            context.AddConverter<OcrResults, string>(ConvertToString);

            // Create 2 binding rules for the Sample attribute.
            var rule = context.AddBindingRule<ImageToTextAttribute>();

            rule.BindToInput<OcrResults>(BuildItemFromAttr);
        }

        private string ConvertToString(OcrResults results)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (results != null && results.Regions != null)
            {
                stringBuilder.Append(" ");
                stringBuilder.AppendLine();
                foreach (var item in results.Regions)
                {
                    foreach (var line in item.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            stringBuilder.Append(word.Text);
                            stringBuilder.Append(" ");
                        }
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private OcrResults BuildItemFromAttr(ImageToTextAttribute attribute)
        {
            string language = attribute.Language ?? DefaultLanguage;
            VisionServiceClient visionServiceClient = new VisionServiceClient(attribute.SubscriptionKey);
            OcrResults results = visionServiceClient.RecognizeTextAsync(attribute.ImageUrl, language).Result;
            return results;
        }
    }
}
