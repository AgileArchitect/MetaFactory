using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Execution;

namespace NeoApi.Controllers
{
    public class ProxyController : ControllerBase
    {
        private HttpClient _client = new HttpClient()
        {
//            BaseAddress = 
        };

        public ProxyController()
        {
            
        }

//        [Route("/db/data/transaction/commit")]
//        [HttpPost]
//        public dynamic Proxy()
//        {
//            Stream req = Request.Body;
//            //req.Seek(0, System.IO.SeekOrigin.Begin);
//            string json = new StreamReader(req).ReadToEnd();
//
//            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("neo4j", "trustno1");
//
//            var response = _client.PostAsync(uri, new StringContent(json)).Result;
//
//            return response;
//        }

        private static Uri uri = new Uri("http://localhost:7474/db/data/transaction/commit");
    }
}
