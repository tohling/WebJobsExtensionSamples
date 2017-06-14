// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json;

namespace SampleExtension.Config
{
    /// <summary>
    /// An extension that extends another extension
    /// We just add new converter rules to existing extensions. 
    /// </summary>
    public class Sample2Extensions : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            // Add a SampleItem-->CustomType<T>
            // 'OpenType' is just a substitute for a generic T. 
            /*
            context.AddBindingRule<SampleAttribute>().
                AddConverter<SampleItem, CustomType<OpenType>>(typeof(CustomConverter<>));
                */
        }

        // A converter.
        // The sdk will pattern match the T against the user's signature. 
        private class CustomConverter<T>
            : IConverter<SampleItem, CustomType<T>>
        {
            public CustomType<T> Convert(SampleItem input)
            {
                // Do some custom logic to create a CustomType<>
                var contents = input.Contents;
                T obj = JsonConvert.DeserializeObject<T>(contents);
                return new CustomType<T>
                {
                     Name = input.Name,
                     Value = obj
                };
            }
        }
    }
}
