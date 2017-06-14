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
    public class ImageInDomainAnalyzerAttribute : Attribute
    {
        // Name of file to read. 
        [AutoResolve]
        public string ImageUrl { get; set; }

        [AutoResolve]
        public string ModelName { get; set; }

        // path where 
        [AppSetting(Default = "VisionSubscriptionKey")]
        public string SubscriptionKey { get; set; }
    }
}
