// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace CognitiveServicesExtension.Config
{
    public class ImageToTextExtension : IExtensionConfigProvider
    {
        const string DefaultLanguage = "unk";

        private static VisionServiceClient visionServiceClient;

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddConverter<OcrResults, string>(ConvertToString);
            context.AddConverter<OcrResults, JObject>(ConvertToJObject);

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

        private JObject ConvertToJObject(OcrResults results)
        {
            return JObject.FromObject(results);
        }

        private OcrResults BuildItemFromAttr(ImageToTextAttribute attribute)
        {
            string language = attribute.Language ?? DefaultLanguage;
            bool detectOrientation = true;
            if(!string.IsNullOrEmpty(attribute.DetectOrientation) 
                && attribute.DetectOrientation.Equals("false", System.StringComparison.InvariantCultureIgnoreCase))
            {
                detectOrientation = false;
            }
            visionServiceClient = new VisionServiceClient(attribute.SubscriptionKey);
            OcrResults results = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                results = visionServiceClient.RecognizeTextAsync(attribute.ImageUrl, language, detectOrientation).Result;
            }
            else
            {
                throw new InvalidOperationException("Missing image url.");
            }

            return results;
        }
    }
}
