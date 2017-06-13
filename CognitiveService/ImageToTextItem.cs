// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using Microsoft.ProjectOxford.Vision.Contract;

namespace CognitiveService
{
    /// <summary>
    /// A "native type" for the extension. This gives full access to the extension's object model. 
    /// The extension may then register converters to convert between this and "bcl" types 
    /// like string, JObject, etc. 
    /// </summary>
    public class ImageToTextItem
    {
        public OcrResults Results { get; set; }
    }
}