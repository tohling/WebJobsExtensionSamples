// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Newtonsoft.Json;
using System.IO;

namespace SampleExtension.Config
{
    /// <summary>
    /// Extension for binding <see cref="SampleAttribute"/>.
    /// This reads and writes files, wrapped as <see cref="SampleItem"/> 
    /// </summary>
    public class SampleExtensions : IExtensionConfigProvider
    {
        // Root path where files are written. 
        // Used when attribute.Root is blank 
        // This is an example of extension-global configuration. 
        // Generally, attributes should be able to override these settings. 
        // Make sure these settings are Json serialization friendly. 
        [JsonProperty("Root")]
        public string Root { get; set; }

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="SampleAttribute"/> 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Register converters. These help convert between the user's parameter type
            //  and the type specified by the binding rules. 

            // This allows a user to bind to IAsyncCollector<string>, and the sdk
            // will convert that to IAsyncCollector<SampleItem>
            context.AddConverter<string, SampleItem>(ConvertToItem);
            
            // This is useful on input. 
            context.AddConverter<SampleItem, string>(ConvertToString);

            // Create 2 binding rules for the Sample attribute.
            context.AddBindingRule<SampleAttribute>().
                BindToInput<SampleItem>(BuildItemFromAttr). 
                BindToCollector<SampleItem>(BuildCollector);
        }

        private string GetRoot(SampleAttribute attribute)
        {
            var root = attribute.Root ?? this.Root ?? Path.GetTempPath();
            return root;
        }

        private string ConvertToString(SampleItem item)
        {
            return item.Contents;
        }

        private SampleItem ConvertToItem(string arg)
        {
            var parts = arg.Split(':');
            return new SampleItem
            {
                 Name = parts[0],
                 Contents = parts[1]
            };
        }

        private IAsyncCollector<SampleItem> BuildCollector(SampleAttribute attribute)
        {
            var root = GetRoot(attribute);
            return new SampleAsyncCollector(root);
        }
        
          // All {} and %% in the Attribute have been resolved by now. 
        private SampleItem BuildItemFromAttr(SampleAttribute attribute)
        {
            var root = GetRoot(attribute);
            var path = Path.Combine(root, attribute.Name);
            if (!File.Exists(path))
            {
                return null;
            }
            var contents = File.ReadAllText(path);
            return new SampleItem
            {
                  Name = attribute.Name,
                  Contents = contents
            };
        }             
    }
}
