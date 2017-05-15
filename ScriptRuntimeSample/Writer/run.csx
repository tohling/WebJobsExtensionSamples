using System.Net;

public static async Task<HttpResponseMessage> Run(
    HttpRequestMessage req, // binds to HttpTrigger
    ICollector<string> myout, // Binds to SampleExtension
    TraceWriter log
    )
{
    log.Info("C# HTTP trigger function processed a request.");

    // Writes a file u
    string filename = "tom";
    string contents = "hello";
    myout.Add($"{filename}:{contents}");
    
    return req.CreateResponse(HttpStatusCode.OK, "Wrote new item:" + filename);
}