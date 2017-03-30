using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SampleExtension;
using System.IO;

namespace SampleFunctions
{
    // USe the Sample Extension 
    public class Functions
    {
        // Write some messages
        [NoAutomaticTrigger]
        public void Writer([Sample] ICollector<string> output)
        {
            // Each string gets converted to a SampleItem and then emited. 
            output.Add("bob:10");
            output.Add("joe:11");
            output.Add("tom:12");
        }

        // Bind to input as string
        // BindToInput<SampleItem> --> Converter --> string
        [NoAutomaticTrigger]
        public void Reader(
            string name,  // from trigger
            [Sample(Name = "{name}")] string contents, 
            TraceWriter log)
        {
            log.Info(contents);
        }

        // Bind to input as rich type:
        // BindToInput<SampleItem> --> item
        [NoAutomaticTrigger]
        public void Reader2(
            string name,  // from trigger
            [Sample(Name = "{name}")] SampleItem item,
            TextWriter log)
        {
            log.WriteLine($"{item.Name}:{item.Contents}");
        }

        #region Using 2nd extensions

        // Bind to input as rich type:
        // BindToInput<SampleItem> --> item
        [NoAutomaticTrigger]
        public void Reader3(
            string name,  // from trigger
            [Sample(Name = "{name}")] CustomType<int> item,
            TextWriter log)
        {
            log.WriteLine($"Via custom type {item.Name}:{item.Value}");
        }
        #endregion 
    }
}
