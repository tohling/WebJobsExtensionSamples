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
    /// Extension for binding <see cref="ImageDescriptionAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="AnalysisResult"/> 
    /// </summary>
    public class DescribeImageExtension : IExtensionConfigProvider
    {
        private static VisionServiceClient visionServiceClient;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="ImageDescriptionAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 
            context.AddConverter<AnalysisResult, string>(ConvertToString);
            context.AddConverter<AnalysisResult, JObject>(ConvertToJObject);

            var rule = context.AddBindingRule<ImageDescriptionAttribute>();

            rule.BindToInput<AnalysisResult>(BuildItemFromAttr);
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
        private AnalysisResult BuildItemFromAttr(ImageDescriptionAttribute attribute)
        {
            int maxCandidatesidth = 1;
            if (int.TryParse(attribute.MaxCandidates, out maxCandidatesidth) == false)
            {
                throw new InvalidOperationException("Invalid width. Valid range >= 1");
            }
            visionServiceClient = new VisionServiceClient(attribute.SubscriptionKey);
            AnalysisResult result = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                result = visionServiceClient.DescribeAsync(attribute.ImageUrl, maxCandidatesidth).Result;
            }
            else
            {
                throw new InvalidOperationException("Missing image url.");
            }

            return result;
        }
    }
}
