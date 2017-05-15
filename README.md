# microsoft.docs

This repro has samples for writing WebJobs SDK extensions. 
See https://github.com/Azure/azure-webjobs-sdk/wiki/Extensibility for more details. 

This creates a new SampleAttribute for reading and writing files  

Sample writer:
```
[Sample] ICollector<string> output
```

Sample reader:
```
 [Sample(Name = "{name}")] string contents, 
```

There are several projects:
1. SampleExtension - the actual new binding. The extension dll can also be published to Functions and be consumed by code writtin in the Functions portal. 
2. SampleFunctions - example of user code consuming the binding. 
3. Host - a small executable used to invoke the bindings and run the sample locally.  In the portal, the Functions runtime will handle this for you. 
4. Sample2Extension - demonstrates extending an extension. 



# For use in Azure Functions:

The https://github.com/Azure/WebJobsExtensionSamples/tree/master/ScriptRuntimeSample contains sample functions for consuming this extension in Azure Functions. 

To load a custom extension in Azure Functions:
1. Stop the site 
2.	Set appsetting `AzureWebJobs_ExtensionsPath=d:\home\customExt`.  Functions will scan this directory and assume each subdirectory is a custom extension and attempt to load it. 
3.	Copy the sample extension binaries (ie, contents of $\WebJobsExtensionSamples\SampleExtension\bin\Debug)  to `d:\home\customExt\sample` 
4.	Set appsetting `SamplePath=d:\home\sample`. This is config for the sample extension. See https://github.com/Azure/WebJobsExtensionSamples/blob/master/SampleExtension/SampleAttribute.cs#L21 
5.	Ensure directory d:\home\sample is created 
6.	Restart site 

At this point, you can copy the functiosn from the sample directory. It has a Writer which writes a file to d:\home\sample, and a reader which reads from it. 

