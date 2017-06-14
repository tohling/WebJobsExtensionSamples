using Microsoft.ProjectOxford.Vision.Contract;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SampleExtension;
using System.IO;
using System.Text;
using CognitiveServicesExtension;

namespace SampleFunctions
{
    // USe the Sample Extension 
    public class Functions
    {
        // Write some messages
        [NoAutomaticTrigger]
        public void Writer([Sample] ICollector<string> output)
        {
            // Each string gets converted to a SampleItem and then emited. 
            output.Add("bob:10");
            output.Add("joe:11");
            output.Add("tom:12");
        }

        // Bind to input as string
        // BindToInput<SampleItem> --> Converter --> string
        [NoAutomaticTrigger]
        public void Reader(
            string name,  // from trigger
            [Sample(FileName = "{name}")] string contents, 
            TraceWriter log)
        {
            log.Info(contents);
        }

        // Bind to input as rich type:
        // BindToInput<SampleItem> --> item
        [NoAutomaticTrigger]
        public void Reader2(
            string name,  // from trigger
            [Sample(FileName = "{name}")] SampleItem item,
            TextWriter log)
        {
            log.WriteLine($"{item.Name}:{item.Contents}");
        }

        // Bind to input as string
        // BindToInput<SampleItem> --> Converter --> string
        [NoAutomaticTrigger]
        public void ImageToTextInterpreter(
            string imageUrl,  // from trigger
            string detectOrientation,
            [ImageToText(ImageUrl = "{imageUrl}", DetectOrientation = "{detectOrientation}")] string results,
            TraceWriter log)
        {
            log.Info(results);
        }

        // Bind to input as string
        // BindToInput<SampleItem> --> Converter --> string
        [NoAutomaticTrigger]
        public void ImageToOcrResultsInterpreter(
            string imageUrl,  // from trigger
            [ImageToText(ImageUrl = "{imageUrl}")] OcrResults results,
            TraceWriter log)
        {
            string result = ConvertToString(results);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ImageAnalyzerGetString(
            string imageUrl,  // from trigger
            string visualFeatures,
            [ImageAnalyzer(ImageUrl = "{imageUrl}", VisualFeatures = "{visualFeatures}")] string analysisResult,
            TraceWriter log)
        {
            log.Info(analysisResult);
        }

        [NoAutomaticTrigger]
        public void ImageAnalyzerGetJObject(
            string imageUrl,  // from trigger
            string visualFeatures,
            [ImageAnalyzer(ImageUrl = "{imageUrl}", VisualFeatures = "{visualFeatures}")] JObject analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromJObject(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ImageAnalyzerGetAnalysisResult(
            string imageUrl,  // from trigger
            string visualFeatures,
            [ImageAnalyzer(ImageUrl = "{imageUrl}", VisualFeatures = "{visualFeatures}")] AnalysisResult analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromAnalysisResult(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ImageAnalyzerInDomainGetString(
            string imageUrl,  // from trigger
            string modelName,
            [ImageInDomainAnalyzer(ImageUrl = "{imageUrl}", ModelName = "{modelName}")] string analysisResult,
            TraceWriter log)
        {
            log.Info(analysisResult);
        }

        [NoAutomaticTrigger]
        public void ImageAnalyzerInDomainGetJObject(
            string imageUrl,  // from trigger
            string modelName,
            [ImageInDomainAnalyzer(ImageUrl = "{imageUrl}", ModelName = "{modelName}")] JObject analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromJObject(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ImageAnalyzerInDomainGetAnalysisResult(
            string imageUrl,  // from trigger
            string modelName,
            [ImageInDomainAnalyzer(ImageUrl = "{imageUrl}", ModelName = "{modelName}")] AnalysisInDomainResult analysisInDomainResult,
            TraceWriter log)
        {
            string result = ConvertFromAnalysisResult(analysisInDomainResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ThumbnailGeneratorGetBytes(
            string imageUrl,  // from trigger
            string width,
            string height,
            string path,
            [ImageThumbnail(ImageUrl = "{imageUrl}", Width = "{width}", Height = "{height}")] byte[] result,
            TraceWriter log)
        {
            File.WriteAllBytes(path, result);
            log.Info("Thumbnail generated");
        }

        [NoAutomaticTrigger]
        public void ImageDescriberGetString(
            string imageUrl,  // from trigger
            string maxCandidates,
            [ImageDescription(ImageUrl = "{imageUrl}", MaxCandidates = "{maxCandidates}")] string analysisResult,
            TraceWriter log)
        {
            log.Info(analysisResult);
        }

        [NoAutomaticTrigger]
        public void ImageDescriberGetJObject(
            string imageUrl,  // from trigger
            string maxCandidates,
            [ImageDescription(ImageUrl = "{imageUrl}", MaxCandidates = "{maxCandidates}")] JObject analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromJObject(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ImageDescriberGetAnalysisResult(
            string imageUrl,  // from trigger
            string maxCandidates,
            [ImageDescription(ImageUrl = "{imageUrl}", MaxCandidates = "{maxCandidates}")] AnalysisResult analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromAnalysisResult(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ImageTagGetString(
            string imageUrl,  // from trigger
            [ImageTag(ImageUrl = "{imageUrl}")] string analysisResult,
            TraceWriter log)
        {
            log.Info(analysisResult);
        }

        [NoAutomaticTrigger]
        public void ImageTagGetJObject(
            string imageUrl,  // from trigger
            [ImageTag(ImageUrl = "{imageUrl}")] JObject analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromJObject(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void ImageTagGetAnalysisResult(
            string imageUrl,  // from trigger
            [ImageTag(ImageUrl = "{imageUrl}")] AnalysisResult analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromAnalysisResult(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void HandwritingGetString(
            string imageUrl,  // from trigger
            [HandwritingToText(ImageUrl = "{imageUrl}")] string analysisResult,
            TraceWriter log)
        {
            log.Info(analysisResult);
        }

        [NoAutomaticTrigger]
        public void HandwritingGetJObject(
            string imageUrl,  // from trigger
            [HandwritingToText(ImageUrl = "{imageUrl}")] JObject analysisResult,
            TraceWriter log)
        {
            string result = ConvertFromJObject(analysisResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void HandwritingGetHandwritingRecognitionOperationResult(
            string imageUrl,  // from trigger
            [HandwritingToText(ImageUrl = "{imageUrl}")] HandwritingRecognitionOperationResult handwritingRecognitionOperationResult,
            TraceWriter log)
        {
            string result = ConvertFromHandwritingRecognitionOperationResult(handwritingRecognitionOperationResult);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void EmotionGetString(
            string imageUrl,  // from trigger
            [ImageEmotion(ImageUrl = "{imageUrl}")] string results,
            TraceWriter log)
        {
            log.Info(results);
        }

        [NoAutomaticTrigger]
        public void EmotionGetJObject(
            string imageUrl,  // from trigger
            [ImageEmotion(ImageUrl = "{imageUrl}")] JArray results,
            TraceWriter log)
        {
            string result = ConvertFromJObject(results);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void EmotionGetEmotionResults(
            string imageUrl,  // from trigger
            [ImageEmotion(ImageUrl = "{imageUrl}")]Emotion[] results,
            TraceWriter log)
        {
            string result = ConvertFromEmotion(results);
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void SpeechToTextGetResult(
            string audioUrl,  // from trigger
            [SpeechToText(AudioUrl = "{audioUrl}")]string result,
            TraceWriter log)
        {
            log.Info(result);
        }

        [NoAutomaticTrigger]
        public void TextToSpeechGetResult(
            string text,  // from trigger
            string voiceType,
            string locale,
            string blobContainerName,
            string blobName,
            [TextToSpeech(Text = "{text}", VoiceType = "{voiceType}", Locale = "{locale}", BlobContainerName = "{blobContainerName}", BlobName = "{blobName}")]string result,
            TraceWriter log)
        {
            log.Info(result);
        }

        private string ConvertFromJObject(JObject result)
        {
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private string ConvertFromJObject(JArray result)
        {
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private string ConvertFromAnalysisResult(AnalysisResult result)
        {
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private string ConvertFromAnalysisResult(AnalysisInDomainResult result)
        {
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private string ConvertFromEmotion(Emotion[] results)
        {
            return JsonConvert.SerializeObject(results, Formatting.Indented);
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

        private string ConvertFromHandwritingRecognitionOperationResult(HandwritingRecognitionOperationResult results)
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

#if false
        #region Using 2nd extensions

        // Bind to input as rich type:
        // BindToInput<SampleItem> --> item
        [NoAutomaticTrigger]
        public void Reader3(
            string name,  // from trigger
            [Sample(Name = "{name}")] CustomType<int> item,
            TextWriter log)
        {
            log.WriteLine($"Via custom type {item.Name}:{item.Value}");
        }
        #endregion
#endif
    }
}
