﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;

namespace CognitiveServicesExtension
{
    /// <summary>
    /// Binding attribute to place on user code for WebJobs. 
    /// </summary>
    [Binding]
    public class TextToSpeechAttribute : Attribute
    {
        // Name of file to read. 
        [AutoResolve]
        public string Text { get; set; }

        [AutoResolve]
        public string VoiceType { get; set; }

        [AutoResolve]
        public string Locale { get; set; }

        // path where 
        [AppSetting(Default = "SpeechSubscriptionKey")]
        public string SubscriptionKey { get; set; }

        [AppSetting(Default = "SpeechConnectionString")]
        public string Connection { get; set; }

        [AutoResolve]
        public string BlobContainerName { get; set; }

        [AutoResolve]
        public string BlobName { get; set; }
    }
}