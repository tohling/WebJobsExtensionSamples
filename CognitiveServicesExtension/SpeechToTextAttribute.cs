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
    public class SpeechToTextAttribute : Attribute
    {
        // Name of file to read. 
        [AutoResolve]
        public string AudioUrl { get; set; }

        [AutoResolve]
        public string Locale { get; set; }

        // path where 
        [AppSetting(Default = "SpeechSubscriptionKey")]
        public string SubscriptionKey { get; set; }
    }
}