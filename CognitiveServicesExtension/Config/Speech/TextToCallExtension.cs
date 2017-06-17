// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CognitiveServicesExtension.Config.Speech;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Clients;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace CognitiveServicesExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="TextToCallAttribute"/>.
    /// </summary>
    public class TextToCallExtension : IExtensionConfigProvider
    {
        const string BearerLabel = "Bearer ";

        const string TempDirEnvKey = "TEMP";

        const string DefaultLocale = "en-US";

        const string RequestUri = "https://speech.platform.bing.com/synthesize";

        const string DefaultVoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)";

        const string DefaultTwilioPhrase = "Hello Azure Functions user!";

        const AudioOutputFormat DefaultOutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm;

        private bool audioReady = false;

        private bool useTemplate = false;

        private bool isFemale = true;

        private TextToCallAttribute textToCallAttribute;

        private string tempFilePath;

        private Dictionary<string, string> templateGreeting = new Dictionary<string, string>();

        public object TwilioClient { get; private set; }

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="TextToCallAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            AddTemplateGreetings();
            context.AddConverter<string, JObject>(ConvertToJObject);

            var rule = context.AddBindingRule<TextToCallAttribute>();

            rule.BindToInput<string>(BuildItemFromAttr);
        }

        private JObject ConvertToJObject(string result)
        {
            return JObject.FromObject(result);
        }

        // All {} and %% in the Attribute have been resolved by now. 
        private string BuildItemFromAttr(TextToCallAttribute attribute)
        {
            this.textToCallAttribute = attribute;
            string audioUri = null;
            string twilioXmlUri = null;
            if (!string.IsNullOrEmpty(this.textToCallAttribute.Text))
            {
                this.GetAudioHelper();
            }
            else
            {
                throw new InvalidOperationException("Missing audio url.");
            }

            while (!audioReady)
            {
                Thread.Sleep(5000);
            }

            if (!string.IsNullOrEmpty(this.textToCallAttribute.Connection)
                && !string.IsNullOrEmpty(this.textToCallAttribute.BlobContainerName)
                && !string.IsNullOrEmpty(this.textToCallAttribute.BlobName))
            {
                audioUri = this.UploadAudioAsync().Result;
                Thread.Sleep(5000);
                twilioXmlUri = this.UploadTwilioXmlAsync(audioUri).Result;
                Thread.Sleep(3000);
                this.CallNumber(twilioXmlUri);
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
            Authentication auth = new Authentication(textToCallAttribute.SubscriptionKey);

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

            if (textToCallAttribute.VoiceType.Equals("male", StringComparison.InvariantCultureIgnoreCase))
            {
                isFemale = false;
            }

            var locale = textToCallAttribute.Locale ?? DefaultLocale;
            if(!string.IsNullOrEmpty(textToCallAttribute.UseTemplate) && textToCallAttribute.UseTemplate.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                useTemplate = true;
            }

            var text = useTemplate ? GetTextFromTemplate(textToCallAttribute.Text) : textToCallAttribute.Text;

            // Reuse Synthesize object to minimize latency
            sythesizer.Sythesize(CancellationToken.None, new Synthesize.InputOptions()
            {
                RequestUri = new Uri(RequestUri),
                Text = text,
                VoiceType = isFemale ? Gender.Female : Gender.Male,
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
            tempFilePath = Path.Combine(Environment.GetEnvironmentVariable(TempDirEnvKey), this.textToCallAttribute.BlobName);
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
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(this.textToCallAttribute.Connection);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(this.textToCallAttribute.BlobContainerName);

            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }
                    );
            }

            if (!this.textToCallAttribute.BlobName.EndsWith("wav", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidDataException("Invalid audio file format.  Only .wav is supported at this time.");
            }

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(this.textToCallAttribute.BlobName);
            cloudBlockBlob.Properties.ContentType = "audio/wav";
            await cloudBlockBlob.UploadFromFileAsync(tempFilePath);

            return cloudBlockBlob.Uri.ToString();
        }

        public async Task<string> UploadTwilioXmlAsync(string audioUrl)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(this.textToCallAttribute.Connection);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(this.textToCallAttribute.BlobContainerName);

            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }
                    );
            }

            if (!this.textToCallAttribute.BlobName.EndsWith("wav", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidDataException("Invalid audio file format.  Only .wav is supported at this time.");
            }

            var xmlBlobName = this.textToCallAttribute.BlobName.Replace(".wav", ".xml");

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(xmlBlobName);
            cloudBlockBlob.Properties.ContentType = "application/xml";
            string xmlContent = GetXmlContent(audioUrl);
            await cloudBlockBlob.UploadTextAsync(xmlContent);

            return cloudBlockBlob.Uri.ToString();
        }

        private void CallNumber(string twilioXmlUri)
        {
            VerifyTwilioConfig();
            var twilioNumber = this.textToCallAttribute.CallerNumber;
            Twilio.TwilioClient.Init(this.textToCallAttribute.TwilioAccountSid, this.textToCallAttribute.TwilioAuthToken);

            var to = new PhoneNumber(this.textToCallAttribute.CalleeNumber);
            var from = new PhoneNumber(this.textToCallAttribute.CallerNumber);
            var call = CallResource.Create(to,
                                           from,
                                           method: Twilio.Http.HttpMethod.Get,
                                           url: new Uri(twilioXmlUri));
        }

        private void AddTemplateGreetings()
        {
            this.templateGreeting.Add("Greeting1", "Azure Functions BYOB framework is awesome.");
            this.templateGreeting.Add("Greeting2", "Thank you for trying out Azure Functions.");
        }

        private string GetTextFromTemplate(string key)
        {
            string text = null;
            if(this.templateGreeting.ContainsKey(key))
            {
                text = this.templateGreeting[key];
            }

            return text;
        }

        private string GetXmlContent(string audioUrl)
        {
            string voice = this.isFemale ? "alice" : "man";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<Response>");
            sb.AppendLine($"<Say voice=\"{voice}\">{DefaultTwilioPhrase}</Say>");
            sb.AppendLine($"<Play>{audioUrl}</Play>");
            sb.AppendLine("</Response>");
            return sb.ToString();
        }

        public void VerifyTwilioConfig()
        {
            if(string.IsNullOrEmpty(this.textToCallAttribute.TwilioAccountSid)
                || string.IsNullOrEmpty(this.textToCallAttribute.TwilioAuthToken)
                || string.IsNullOrEmpty(this.textToCallAttribute.CallerNumber)
                || string.IsNullOrEmpty(this.textToCallAttribute.CalleeNumber))
            {
                throw new InvalidDataException("Missing or invalid Twilio configuration. Kindly provide TwilioAccountSid, TwilioAuthToken, CallerNumber, CalleeNumber");
            }
        }
    }
}
