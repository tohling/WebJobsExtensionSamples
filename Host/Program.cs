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
            var speechToTextExtension = new CognitiveServicesExtension.Config.SpeechToTextExtension();
            var textToSpeechExtension = new CognitiveServicesExtension.Config.TextToSpeechExtension();
            var textToCallExtension = new CognitiveServicesExtension.Config.TextToCallExtension();
            config.AddExtension(imageToTextExtension);
            config.AddExtension(analyzeImageExtension);
            config.AddExtension(analyzeImageInDomainExtension);
            config.AddExtension(thumbnailExtension);
            config.AddExtension(descriptionExtension);
            config.AddExtension(tagExtension);
            config.AddExtension(recognizeHandwritingExtension);
            config.AddExtension(detectEmotionExtension);
            config.AddExtension(speechToTextExtension);
            config.AddExtension(textToSpeechExtension);
            config.AddExtension(textToCallExtension);

            var method = typeof(Functions).GetMethod("ImageToTextInterpreter");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/photo.png", detectOrientation = "true" });

            /*
            method = typeof(Functions).GetMethod("ImageToOcrResultsInterpreter");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/photo.png", detectOrientation = "true" });
            
            method = typeof(Functions).GetMethod("ImageAnalyzerGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/happy.jpg", visualFeatures = "Adult,Categories,Color,Description,Faces,ImageType,Tags" });
            
            method = typeof(Functions).GetMethod("ImageAnalyzerGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/happy.jpg", visualFeatures = "Adult,Categories,Color,Description,Faces,ImageType,Tags" });
            /*
            method = typeof(Functions).GetMethod("ImageAnalyzerGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg", visualFeatures = "Adult,Categories,Color,Description,Faces,ImageType,Tags" });
            
            method = typeof(Functions).GetMethod("ImageAnalyzerInDomainGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", modelName = "celebrities"});
            
            method = typeof(Functions).GetMethod("ThumbnailGeneratorGetBytes");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", width = "300", height = "500" , path = @"C:\temp\cogtest\satya_3by5.jpg"});

            method = typeof(Functions).GetMethod("ImageDescriberGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", maxCandidates = "3" });

            method = typeof(Functions).GetMethod("ImageDescriberGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", maxCandidates = "3" });

            method = typeof(Functions).GetMethod("ImageDescriberGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/satya.jpg", maxCandidates = "3" });

            method = typeof(Functions).GetMethod("ImageTagGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg" });

            method = typeof(Functions).GetMethod("ImageTagGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg" });

            method = typeof(Functions).GetMethod("ImageTagGetAnalysisResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/meeting.jpg" });

            method = typeof(Functions).GetMethod("HandwritingGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/letter3.jpg" });

            method = typeof(Functions).GetMethod("HandwritingGetHandwritingRecognitionOperationResult");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/letter3.jpg" });

            method = typeof(Functions).GetMethod("EmotionGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/angry.jpg" });

            method = typeof(Functions).GetMethod("EmotionGetEmotionResults");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/angry.jpg" });

            method = typeof(Functions).GetMethod("EmotionGetString");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/surprised.jpg" });
            
            method = typeof(Functions).GetMethod("EmotionGetJObject");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/surprised.jpg" });
            */
            /*
            method = typeof(Functions).GetMethod("EmotionGetEmotionResults");
            host.Call(method, new { imageUrl = "http://hutohant10store.blob.core.windows.net/test/surprised.jpg" });
            

            method = typeof(Functions).GetMethod("SpeechToTextGetResult");
            host.Call(method, new { audioUrl = "http://hutohant10store.blob.core.windows.net/test/amy.wav"});

            method = typeof(Functions).GetMethod("SpeechToTextGetResult");
            host.Call(method, new { audioUrl = "http://hutohant10store.blob.core.windows.net/test/whatstheweatherlike.wav" });
            

            method = typeof(Functions).GetMethod("TextToSpeechGetResult");
            host.Call(method, 
                new
                {
                    text = "It is very late right now.  You should go to bed.",
                    voiceType = "male",
                    locale = "en-US",
                    blobContainerName = "speech",
                    blobName = "nightadvice.wav"
            });
            */

            method = typeof(Functions).GetMethod("TextToCallGetResult");
            host.Call(method,
                new
                {
                    text = "Greeting2",
                    voiceType = "male",
                    locale = "en-US",
                    blobContainerName = "speech",
                    blobName = "greeting1.wav",
                    callerNumber = "+12067353578",
                    calleeNumber = "+14257868063",
                    useTemplate = "true"
                });

            // host.RunAndBlock();
        }
    }
}
