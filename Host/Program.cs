// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using SampleFunctions;
using System;

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
            var sampleExtension = new SampleExtension.Config.SampleExtensions();
            config.AddExtension(sampleExtension);

            // A 2nd extension that adds a custom rule on top of the first extension. 
            //var sample2Extension = new SampleExtension.Config.Sample2Extensions();
            //config.AddExtension(sample2Extension);

            // Debug diagnostics!
            config.CreateMetadataProvider().DebugDumpGraph(Console.Out);

            var host = new JobHost(config);

            // Test some invocations. 
            // We're not using listeners here, so we can invoke directly. 
            var method = typeof(Functions).GetMethod("Writer");
            host.Call(method);

            //method = typeof(Functions).GetMethod("Reader3");
            //host.Call(method, new { name = "tom" });

            // host.RunAndBlock();
        }
    }
}
