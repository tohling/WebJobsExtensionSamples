using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using SampleExtension;

namespace FunctionApp
{
    public static class WriterFunction
    {
        [FunctionName("WriterFunction")]
        public static HttpResponseMessage Run(
            HttpRequestMessage req,
            [HttpTrigger] SampleItem item,
            [Sample] ICollector<string> sampleOutput, TraceWriter log)
        {
            sampleOutput.Add($"{item.Name}:{item.Contents}");

            return req.CreateResponse(HttpStatusCode.OK, "Wrote new item:" + item.Name);
        }
    }
}