// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.CognitiveServices.SpeechRecognition;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveServicesExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="SpeechToTextAttribute"/>.
    /// </summary>
    public class SpeechToTextExtension : IExtensionConfigProvider
    {
        static readonly TimeSpan WaitTimeInSecond = TimeSpan.FromSeconds(3);

        const string DefaultLocale = "en-US";

        private static DataRecognitionClient dataClient;

        private string audioUrl;

        private string audioText;

        private bool textReady = false;

        private string storageConnectionString;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="SpeechToTextAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<SpeechToTextAttribute>();

            rule.BindToInput<string>(BuildItemFromAttr);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private string BuildItemFromAttr(SpeechToTextAttribute attribute)
        {
            string locale = attribute.Locale ?? DefaultLocale;
            this.CreateDataRecClient(locale, attribute.SubscriptionKey);

            if (!string.IsNullOrEmpty(attribute.AudioUrl))
            {
                this.audioUrl = attribute.AudioUrl;
                this.SendAudioHelper(this.audioUrl);
            }
            else
            {
                throw new InvalidOperationException("Missing audio url");
            }

            if (!string.IsNullOrEmpty(attribute.Connection))
            {
                this.storageConnectionString = attribute.Connection;
            }
            else
            {
                throw new InvalidOperationException("Missing storage connection string.");
            }

            while (!textReady)
            {
                Task.Delay(WaitTimeInSecond).Wait();
            }

            return this.audioText;
        }

        private void CreateDataRecClient(string locale, string subscriptionKey)
        {
            dataClient = SpeechRecognitionServiceFactory.CreateDataClient(
                SpeechRecognitionMode.LongDictation,
                locale,
                subscriptionKey);

            // Event handlers for speech recognition results
            dataClient.OnResponseReceived += this.OnDataDictationResponseReceivedHandler;
            //dataClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            dataClient.OnConversationError += this.OnConversationErrorHandler;
        }

        /// <summary>
        /// Writes the response result.
        /// </summary>
        /// <param name="e">The <see cref="SpeechResponseEventArgs"/> instance containing the event data.</param>
        private void WriteResponseResult(SpeechResponseEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string msg = null;
            if (e.PhraseResponse.Results.Length == 0)
            {
                msg = "No phrase response is available.";
                stringBuilder.AppendLine(msg);
            }
            else
            {
                msg = "********* Final n-BEST Results *********";
                stringBuilder.AppendLine(msg);
                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    msg = string.Format(
                        "[{0}] Text=\"{1}\"",
                        i,
                        e.PhraseResponse.Results[i].DisplayText);
                    stringBuilder.AppendLine(msg);
                }

                stringBuilder.AppendLine();
            }

            this.audioText = stringBuilder.ToString(); 
            textReady = true;
        }

        /// <summary>
        /// Disabled: Console.WriteLine does not work in Functions framework
        /// Called when a partial response is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PartialSpeechResponseEventArgs"/> instance containing the event data.</param>
        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Console.WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            Console.WriteLine("{0}", e.PartialResult);
            Console.WriteLine();
        }

        /// <summary>
        /// Called when an error is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechErrorEventArgs"/> instance containing the event data.</param>
        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            string errorMessage = $"ErrorCode: {e.SpeechErrorCode.ToString()}, ErrorMessage: {e.SpeechErrorText}";
            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        /// Called when a final response is received;
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechResponseEventArgs"/> instance containing the event data.</param>
        private void OnDataDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
                e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout)
            {
                dataClient.Dispose();
            }

            this.WriteResponseResult(e);
        }

        /// <summary>
        /// Sends the audio helper.
        /// </summary>
        /// <param name="audioUrl">The url of the wav file.</param>
        private void SendAudioHelper(string audioUrl)
        {
            textReady = false;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Note for wave files, we can just send data from the file right to the server.
                // In the case you are not an audio file in wave format, and instead you have just
                // raw data (for example audio coming over bluetooth), then before sending up any 
                // audio data, you must first send up an SpeechAudioFormat descriptor to describe 
                // the layout and format of your raw audio data via DataRecognitionClient's sendAudioFormat() method.
                int bytesRead = 0;
                byte[] buffer = new byte[1024];

                try
                {
                    Uri blobUri = new Uri(audioUrl);
                    CloudBlockBlob blob = new CloudBlockBlob(blobUri);
                    blob.DownloadToStream(memoryStream);
                    memoryStream.Position = 0;

                    do
                    {
                        // Get more Audio data to send into byte buffer.
                        bytesRead = memoryStream.Read(buffer, 0, buffer.Length);

                        // Send of audio data to service. 
                        dataClient.SendAudio(buffer, bytesRead);
                    } while (bytesRead > 0);
                }
                finally
                {
                    // We are done sending audio.  Final recognition results will arrive in OnResponseReceived event call.
                    dataClient.EndAudio();
                }
            }
        }
    }
}
