// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CognitiveServicesExtension;
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
    /// Extension for binding <see cref="ImageInDomainAnalyzerAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="AnalysisInDomainResult"/> 
    /// </summary>
    public class AnalyzeImageInDomainExtension : IExtensionConfigProvider
    {
        private static VisionServiceClient visionServiceClient;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="ImageInDomainAnalyzerAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 

            // This is useful on input. 
            context.AddConverter<AnalysisInDomainResult, string>(ConvertToString);
            context.AddConverter<AnalysisInDomainResult, JObject>(ConvertToJObject);

            var rule = context.AddBindingRule<ImageInDomainAnalyzerAttribute>();

            rule.BindToInput<AnalysisInDomainResult>(BuildItemFromAttr);
        }

        private JObject ConvertToJObject(AnalysisInDomainResult result)
        {
            return JObject.FromObject(result);
        }

        private string ConvertToString(AnalysisInDomainResult result)
        {
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private AnalysisInDomainResult BuildItemFromAttr(ImageInDomainAnalyzerAttribute attribute)
        {
            visionServiceClient = new VisionServiceClient(attribute.SubscriptionKey);
            AnalysisInDomainResult result = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                result = visionServiceClient.AnalyzeImageInDomainAsync(attribute.ImageUrl, GetDomainModel(attribute.ModelName)).Result;
            }
            else
            {
                throw new InvalidOperationException("Missing image url.");
            }

            return result;
        }

        private Model GetDomainModel(string modelName)
        {
            ModelResult modelResult = visionServiceClient.ListModelsAsync().Result;
            return modelResult.Models.First(m => m.Name.Equals(modelName, System.StringComparison.InvariantCultureIgnoreCase));
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
