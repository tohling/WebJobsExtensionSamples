// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveServicesExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="ImageAnalyzerAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="AnalysisResult"/> 
    /// </summary>
    public class AnalyzeImageExtension : IExtensionConfigProvider
    {
        private static VisionServiceClient visionServiceClient;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="ImageAnalyzerAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 

            // This is useful on input. 
            context.AddConverter<AnalysisResult, string>(ConvertToString);
            context.AddConverter<AnalysisResult, JObject>(ConvertToJObject);

            var rule = context.AddBindingRule<ImageAnalyzerAttribute>();

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
        private AnalysisResult BuildItemFromAttr(ImageAnalyzerAttribute attribute)
        {
            var features = GetVisualFeatures(attribute.VisualFeatures);
            var details = GetDetails(attribute.Details);
            visionServiceClient = new VisionServiceClient(attribute.SubscriptionKey);
            AnalysisResult result = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                result = visionServiceClient.AnalyzeImageAsync(attribute.ImageUrl, features, details).Result;
            }
            else if (attribute.ImageStream != null)
            {
                result = visionServiceClient.AnalyzeImageAsync(attribute.ImageStream, features, details).Result;
            }
            else
            {
                throw new InvalidOperationException("Missing image url or stream.");
            }

            return result;
        }

        private List<string> GetDetails(string stringDetails)
        {
            List<string> details = null;

            if (!string.IsNullOrEmpty(stringDetails))
            {
                var strdetails = stringDetails.Split(',');

                if (strdetails.Any())
                {
                    details = new List<string>();
                    details.AddRange(strdetails);
                }
            }

            return details;
        }

        private List<VisualFeature> GetVisualFeatures(string stringFeatures)
        {
            List<VisualFeature> visualFeatures = null;

            if (!string.IsNullOrEmpty(stringFeatures))
            {
                var features = stringFeatures.Split(',');

                if (features.Any())
                {
                    visualFeatures = new List<VisualFeature>();
                    foreach (var feature in features)
                    {
                        if (string.Equals(feature, VisualFeature.ImageType.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            visualFeatures.Add(VisualFeature.ImageType);
                        }
                        else if (string.Equals(feature, VisualFeature.Color.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            visualFeatures.Add(VisualFeature.Color);
                        }
                        else if (string.Equals(feature, VisualFeature.Faces.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            visualFeatures.Add(VisualFeature.Faces);
                        }
                        else if (string.Equals(feature, VisualFeature.Adult.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            visualFeatures.Add(VisualFeature.Adult);
                        }
                        else if (string.Equals(feature, VisualFeature.Categories.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            visualFeatures.Add(VisualFeature.Categories);
                        }
                        else if (string.Equals(feature, VisualFeature.Tags.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            visualFeatures.Add(VisualFeature.Tags);
                        }
                        else if (string.Equals(feature, VisualFeature.Description.ToString(), System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            visualFeatures.Add(VisualFeature.Description);
                        }
                    }
                }
            }

            return visualFeatures;
        }
    }
}
