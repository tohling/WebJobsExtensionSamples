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
