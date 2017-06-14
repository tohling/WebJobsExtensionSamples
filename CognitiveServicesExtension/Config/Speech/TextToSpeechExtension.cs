// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CognitiveServicesExtension.Config.Speech;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveServicesExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="TextToSpeechAttribute"/>.
    /// </summary>
    public class TextToSpeechExtension : IExtensionConfigProvider
    {
        static readonly TimeSpan WaitTimeInSecond = TimeSpan.FromSeconds(3);

        const string BearerLabel = "Bearer ";

        const string TempDirEnvKey = "TEMP";

        const string DefaultLocale = "en-US";

        const string RequestUri = "https://speech.platform.bing.com/synthesize";

        const string DefaultVoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)";

        const AudioOutputFormat DefaultOutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm;

        private bool audioReady = false;

        private TextToSpeechAttribute textToSpeechAttribute;

        private string tempFilePath;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="TextToSpeechAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<TextToSpeechAttribute>();

            rule.BindToInput<string>(BuildItemFromAttr);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private string BuildItemFromAttr(TextToSpeechAttribute attribute)
        {
            this.textToSpeechAttribute = attribute;
            string audioUri = null;
            if (!string.IsNullOrEmpty(this.textToSpeechAttribute.Text))
            {
                this.GetAudioHelper();
            }
            else
            {
                throw new InvalidOperationException("Missing audio url.");
            }

            while(!audioReady)
            {
                Task.Delay(WaitTimeInSecond).Wait();
            }

            if (!string.IsNullOrEmpty(this.textToSpeechAttribute.Connection)
                && !string.IsNullOrEmpty(this.textToSpeechAttribute.BlobContainerName)
                && !string.IsNullOrEmpty(this.textToSpeechAttribute.BlobName))
            {
                audioUri = this.UploadAudioAsync().Result;
            }
            else
            {
                throw new InvalidOperationException("Missing storage connection string.");
            }

            return audioUri;
        }

        /// <summary>
        /// Gets the audio helper.
        /// </summary>
        /// <param name="attribute">The TextToSpeechAttribute.</param>
        private void GetAudioHelper()
        {
            this.audioReady = false;
            string accessToken = null;
            Authentication auth = new Authentication(textToSpeechAttribute.SubscriptionKey);

            try
            {
                accessToken = auth.GetAccessToken();
            }
            catch (Exception ex)
            {
                return;
            }


            var sythesizer = new Synthesize();

            sythesizer.OnAudioAvailable += SaveAudio;
            sythesizer.OnError += ErrorHandler;

            var gender = Gender.Female;
            if (textToSpeechAttribute.VoiceType.Equals("male", StringComparison.InvariantCultureIgnoreCase))
            {
                gender = Gender.Male;
            }

            var locale = textToSpeechAttribute.Locale ?? DefaultLocale;

            // Reuse Synthesize object to minimize latency
            sythesizer.Sythesize(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(RequestUri),
                Text = textToSpeechAttribute.Text,
                VoiceType = gender,
                Locale = locale,
                VoiceName = DefaultVoiceName,
                OutputFormat = DefaultOutputFormat,
                AuthorizationToken = BearerLabel + accessToken,
            }).Wait();
        }

        /// <summary>
        /// This method is called once the audio returned from the service.
        /// It will then attempt to save that audio file to blob storage.
        /// Note that the save will fail if the output audio format is not pcm encoded.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="GenericEventArgs{Stream}"/> instance containing the event data.</param>
        private void SaveAudio(object sender, GenericEventArgs<Stream> args)
        {
            tempFilePath = Path.Combine(Environment.GetEnvironmentVariable(TempDirEnvKey), this.textToSpeechAttribute.BlobName);
            using (WaveStream waveStream = new WaveFileReader(args.EventData))
            {
                WaveFileWriter.CreateWaveFile(tempFilePath, waveStream);
            }

            args.EventData.Dispose();
            this.audioReady = true;
        }

        /// <summary>
        /// Handler an error when a TTS request failed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GenericEventArgs{Exception}"/> instance containing the event data.</param>
        private static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
            throw new InvalidOperationException($"Unable to complete the TTS request: {e.ToString()}");
        }

        public async Task<string> UploadAudioAsync()
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(this.textToSpeechAttribute.Connection);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(this.textToSpeechAttribute.BlobContainerName);

            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }
                    );
            }

            if (!this.textToSpeechAttribute.BlobName.EndsWith("wav", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidDataException("Invalid audio file format.  Only .wav is supported at this time.");
            }

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(this.textToSpeechAttribute.BlobName);
            cloudBlockBlob.Properties.ContentType = "application/octet-stream";
            await cloudBlockBlob.UploadFromFileAsync(tempFilePath);

            return cloudBlockBlob.Uri.ToString();
        }
    }
}
