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
    public class ImageThumbnailAttribute : Attribute
    {
        // Name of file to read. 
        [AutoResolve]
        public string ImageUrl { get; set; }

        [AutoResolve]
        public string Width { get; set; }

        [AutoResolve]
        public string Height { get; set; }

        [AutoResolve]
        public string SmartCropping { get; set; }

        [AppSetting(Default = "AzureWebJobsStorage")]
        public string Connection { get; set; }

        [AutoResolve]
        public string BlobContainerName { get; set; }

        [AutoResolve]
        public string BlobName { get; set; }

        // path where 
        [AppSetting(Default = "VisionSubscriptionKey")]
        public string SubscriptionKey { get; set; }
    }
}