// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using SampleFunctions;

namespace Host
{
    // WebJobs is .NET 4.6 
    class Program
    {
        static void Main(string[] args)
        {
            var config = new JobHostConfiguration();
            config.DashboardConnectionString = null;

            // apply config before creating the host. 
            var sampleExtension = new SampleExtension.SampleExtensions();
            config.AddExtension(sampleExtension);

            var host = new JobHost(config);

            // Test some invocations. 
            // We're not using listeners here, so we can invoke directly. 
            var method = typeof(Functions).GetMethod("Writer");
            host.Call(method);

            method = typeof(Functions).GetMethod("Reader2");
            host.Call(method, new { name = "tom" });

            // host.RunAndBlock();
        }
    }
}
