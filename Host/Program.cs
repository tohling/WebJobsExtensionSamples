// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using SampleFunctions;

namespace Host
{
    // WebJobs is .NET 4.6 
    class Program
    {
        static void Main(string[] args)
        {
            var config = new JobHostConfiguration();
            config.DashboardConnectionString = null;

            // apply config before creating the host. 
            var sampleExtension = new SampleExtension.Config.SampleExtensions();
            config.AddExtension(sampleExtension);

            // A 2nd extension that adds a custom rule on top of the first extension. 
            //var sample2Extension = new SampleExtension.Config.Sample2Extensions();
            //config.AddExtension(sample2Extension);

            // Debug diagnostics!
            // config.CreateMetadataProvider().DebugDumpGraph(Console.Out);

            var host = new JobHost(config);

            // Test some invocations. 
            // We're not using listeners here, so we can invoke directly. 
            // var method = typeof(Functions).GetMethod("Writer");
            // host.Call(method);

            /*
            var method = typeof(Functions).GetMethod("Reader");
            host.Call(method, new { name = "tom" });
            */

            var imageToTextExtension = new CognitiveServicesExtension.Config.ImageToTextExtension();
            var analyzeImageExtension = new CognitiveServicesExtension.Config.AnalyzeImageExtension();
            var analyzeImageInDomainExtension = new CognitiveServicesExtension.Config.AnalyzeImageInDomainExtension();
            var thumbnailExtension = new CognitiveServicesExtension.Config.ThumbnailExtension();
            var descriptionExtension = new CognitiveServicesExtension.Config.DescribeImageExtension();
            var tagExtension = new CognitiveServicesExtension.Config.TagImageExtension();
            var recognizeHandwritingExtension = new CognitiveServicesExtension.Config.RecognizeHandwritingImageExtension();
            var detectEmotionExtension = new CognitiveServicesExtension.Config.DetecEmotionExtension();
            config.AddExtension(imageToTextExtension);
            config.AddExtension(analyzeImageExtension);
            config.AddExtension(analyzeImageInDomainExtension);
            config.AddExtension(thumbnailExtension);
            config.AddExtension(descriptionExtension);
            config.AddExtension(tagExtension);
            config.AddExtension(recognizeHandwritingExtension);
            config.AddExtension(detectEmotionExtension);

            /*
            var method = typeof(Functions).GetMethod("ImageToTextInterpreter");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/photo.png", detectOrientation = "true" });

            method = typeof(Functions).GetMethod("ImageToOcrResultsInterpreter");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/photo.png", detectOrientation = "true" });
            
            var method = typeof(Functions).GetMethod("ImageAnalyzerGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/happy.jpg", visualFeatures = "Adult,Categories,Color,Description,Faces,ImageType,Tags" });

            method = typeof(Functions).GetMethod("ImageAnalyzerGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/happy.jpg", visualFeatures = "Adult,Categories,Color,Description,Faces,ImageType,Tags" });
            
            var method = typeof(Functions).GetMethod("ImageAnalyzerGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg", visualFeatures = "Adult,Categories,Color,Description,Faces,ImageType,Tags" });
            

            var method = typeof(Functions).GetMethod("ImageAnalyzerInDomainGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", modelName = "celebrities"});
            
            var method = typeof(Functions).GetMethod("ThumbnailGeneratorGetBytes");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", width = "300", height = "500" , path = @"C:\temp\cogtest\satya_3by5.jpg"});

            */

            /*
            var method = typeof(Functions).GetMethod("ImageDescriberGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", maxCandidates = "3" });

            method = typeof(Functions).GetMethod("ImageDescriberGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", maxCandidates = "3" });

            method = typeof(Functions).GetMethod("ImageDescriberGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", maxCandidates = "3" });
            */
            /*
            var method = typeof(Functions).GetMethod("ImageTagGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg" });

            method = typeof(Functions).GetMethod("ImageTagGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg" });

            method = typeof(Functions).GetMethod("ImageTagGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg" });
            */
            /*
            var method = typeof(Functions).GetMethod("HandwritingGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/letter3.jpg" });

            method = typeof(Functions).GetMethod("HandwritingGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/letter3.jpg" });

            method = typeof(Functions).GetMethod("HandwritingGetHandwritingRecognitionOperationResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/letter3.jpg" });
            */

            var method = typeof(Functions).GetMethod("EmotionGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/angry.jpg", subscriptionKey = "17288ecac33a48948018ece331a9b1d9" });

            method = typeof(Functions).GetMethod("EmotionGetHandwritingEmotionResults");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/angry.jpg", subscriptionKey = "17288ecac33a48948018ece331a9b1d9" });

            method = typeof(Functions).GetMethod("EmotionGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/surprised.jpg", subscriptionKey = "17288ecac33a48948018ece331a9b1d9" });

            method = typeof(Functions).GetMethod("EmotionGetHandwritingEmotionResults");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/surprised.jpg", subscriptionKey = "17288ecac33a48948018ece331a9b1d9" });

            // host.RunAndBlock();
        }
    }
}
