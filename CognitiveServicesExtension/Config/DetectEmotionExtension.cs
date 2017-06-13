// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CognitiveServicesExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="ImageEmotionAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="AnalysisResult"/> 
    /// </summary>
    public class DetecEmotionExtension : IExtensionConfigProvider
    {
        private static EmotionServiceClient emotionServiceClient;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="ImageEmotionAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 
            context.AddConverter<Emotion[], string>(ConvertToString);

            var rule = context.AddBindingRule<ImageEmotionAttribute>();

            rule.BindToInput<Emotion[]>(BuildItemFromAttr);
        }

        private string ConvertToString(Emotion[] result)
        {
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private Emotion[] BuildItemFromAttr(ImageEmotionAttribute attribute)
        {
            emotionServiceClient = new EmotionServiceClient(attribute.SubscriptionKey);
            Emotion[] result = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                result = emotionServiceClient.RecognizeAsync(attribute.ImageUrl).Result;
            }
            else if (attribute.ImageStream != null)
            {
                result = emotionServiceClient.RecognizeAsync(attribute.ImageStream).Result;
            }
            else
            {
                throw new InvalidOperationException("Missing image url or stream.");
            }

            return result;
        }
    }
}
