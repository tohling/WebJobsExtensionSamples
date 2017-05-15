using System.Net;

public static async Task<HttpResponseMessage> Run(
    HttpRequestMessage req,
    string contents, // Bind to SampleExtension  
    TraceWriter log
 )
{
    log.Info("C# HTTP trigger function processed a request.");
    log.Info("Contents:" + contents);
  
    return req.CreateResponse(HttpStatusCode.OK, "Contents: " + contents);
}