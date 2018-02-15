using System.Diagnostics;
using System.Text;
using System.IO;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;


namespace FunctionsCosmosdbInputBinding
{
    public class SampleObj
    {
        public string value1 { get; set; }
    }

    public static class FunctionsCosmosdbInputBinding
    {
        static string endpoint = ConfigurationManager.AppSettings["Endpoint"];
        static string authKey = ConfigurationManager.AppSettings["AuthKey"];

        [FunctionName("FunctionsCosmosdbInputBinding")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req,
            //[DocumentDB("sample-book", "sample-col",
            //ConnectionStringSetting = "scraping-pool-documentdb_DOCUMENTDB",
            //SqlQuery = "SELECT c.columns FROM c WHERE c.filter = '{param1}'")]IEnumerable<JObject> documents,
            //SqlQuery = "SELECT c.columns FROM c WHERE c.filter = '{param1}'")]IEnumerable<JObject> documents,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            /* bodyÇÊÇËÉpÉâÉÅÅ[É^éÊìæ */
            dynamic jsonContent = await req.Content.ReadAsAsync<object>();
            JToken activityLog = JObject.Parse(jsonContent.ToString());
            string param1 = (string)activityLog["param1"] ?? null;

            // open the client's connection
            List<dynamic> dbGetList = new List<dynamic>();
            using (DocumentClient client = new DocumentClient(
                new Uri(endpoint),
                authKey,
                new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp
                }))
            {
                Uri collectionUrl = UriFactory.CreateDocumentCollectionUri("sample-db", "sample-col");
                SampleObj document = client.CreateDocumentQuery<dynamic>(
                    collectionUrl,
                    $"SELECT c.columns FROM c WHERE c.filter = '{param1}'").AsEnumerable().FirstOrDefault();

                dbGetList.Add(document.value1);
            }
            
            return param1 == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello " + param1);
        }
    }
}
