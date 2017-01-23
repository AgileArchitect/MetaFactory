using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using NeoApi.Model;
using NeoApi.Extensions;


namespace NeoApi.Controllers
{
    public class DependencyController : ControllerBase
    {
        private readonly IGraphClient _client;


        public DependencyController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        [Route("/api/hello")]
        [Produces(typeof(string))]
        public string Hello()
        {
            return "Hello";
        }

        [HttpGet]
        [Route("/api/packages")]
        public IEnumerable<Package> Packages()
        {
            return _client.Query<Package>();
        }

        [HttpPost]
        [Route("/api/package/{name}")]
        public HttpStatusCode SetPackage([FromRoute] string name, [FromBody] Package[] dependencies)
        {
            return HttpStatusCode.OK;
        }
    }
}
