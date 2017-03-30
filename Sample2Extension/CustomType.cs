// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace SampleExtension
{
    // My extensions arbitrary custom type. 
    // Make it generic just to be interesting.
    // This is similar to how CloudTable can bind to IQueryable<T>
    public class CustomType<T>
    {
        public string Name { get; set; }
        public T Value;
    }
}