// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveServicesExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="HandwritingToTextAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="HandwritingRecognitionOperationResult"/> 
    /// </summary>
    public class RecognizeHandwritingImageExtension : IExtensionConfigProvider
    {
        static readonly int MaxRetryTimes = 3;
        static readonly TimeSpan QueryWaitTimeInSecond = TimeSpan.FromSeconds(3);
        private static VisionServiceClient visionServiceClient;
        
        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="HandwritingToTextAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 
            context.AddConverter<HandwritingRecognitionOperationResult, string>(ConvertToString);
            context.AddConverter<HandwritingRecognitionOperationResult, JObject>(ConvertToJObject);

            var rule = context.AddBindingRule<HandwritingToTextAttribute>();

            rule.BindToInput<HandwritingRecognitionOperationResult>(BuildItemFromAttr);
        }

        private JObject ConvertToJObject(HandwritingRecognitionOperationResult result)
        {
            return JObject.FromObject(result);
        }

        private string ConvertToString(HandwritingRecognitionOperationResult results)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (results != null && results.RecognitionResult != null && results.RecognitionResult.Lines != null && results.RecognitionResult.Lines.Length > 0)
            {
                stringBuilder.Append("Text: ");
                stringBuilder.AppendLine();
                foreach (var line in results.RecognitionResult.Lines)
                {
                    foreach (var word in line.Words)
                    {
                        stringBuilder.Append(word.Text);
                        stringBuilder.Append(" ");
                    }

                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private HandwritingRecognitionOperationResult BuildItemFromAttr(HandwritingToTextAttribute attribute)
        {
            visionServiceClient = new VisionServiceClient(attribute.SubscriptionKey);
            HandwritingRecognitionOperationResult result = null;
            HandwritingRecognitionOperation operation = null;

            if (!string.IsNullOrEmpty(attribute.ImageUrl))
            {
                operation = visionServiceClient.CreateHandwritingRecognitionOperationAsync(attribute.ImageUrl).Result;
            }
            else
            {
                throw new InvalidOperationException("Missing image url.");
            }

            int i = 0;
            result = visionServiceClient.GetHandwritingRecognitionOperationResultAsync(operation).Result;
            while ((result.Status == HandwritingRecognitionOperationStatus.Running || result.Status == HandwritingRecognitionOperationStatus.NotStarted) && i++ < MaxRetryTimes)
            {
                Task.Delay(QueryWaitTimeInSecond).Wait();

                result = visionServiceClient.GetHandwritingRecognitionOperationResultAsync(operation).Result;
            }

            return result;
        }
    }
}
