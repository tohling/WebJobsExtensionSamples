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
    public static class ReaderFunction
    {
        [FunctionName("ReaderFunction")]
        public static HttpResponseMessage Run(
            HttpRequestMessage req,
            [HttpTrigger] SampleItem item,
            [Sample(FileName = "{Name}")] string contents, // Bind to SampleExtension  
            TraceWriter log
         )
        {
            log.Info("C# HTTP trigger function processed a request.");
            log.Info("Contents:" + contents);

            return req.CreateResponse(HttpStatusCode.OK, "Contents: " + contents);
        }
    }
}