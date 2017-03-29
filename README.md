# microsoft.docs

This repro has samples for writing WebJobs SDK extensions. 
See https://github.com/Azure/azure-webjobs-sdk/wiki/Extensibilityfor more details. 

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
1. SampleExtension - the actual new binding.  
2. SampleFunctions - example of user code consuming the binding. 
3. Host - a small executable used to run the sample locally.  
