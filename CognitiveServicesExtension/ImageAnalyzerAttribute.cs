// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using System.IO;

namespace CognitiveServicesExtension
{
    /// <summary>
    /// Binding attribute to place on user code for WebJobs. 
    /// </summary>
    [Binding]
    public class ImageAnalyzerAttribute : Attribute
    {
        // Name of file to read. 
        [AutoResolve]
        public string ImageUrl { get; set; }

        [AutoResolve]
        public Stream ImageStream { get; set; }

        [AutoResolve]
        public string VisualFeatures { get; set; }

        [AutoResolve]
        public string Details { get; set; }

        // path where 
        [AppSetting(Default = "SubscriptionKey")]
        public string SubscriptionKey { get; set; }
    }
}