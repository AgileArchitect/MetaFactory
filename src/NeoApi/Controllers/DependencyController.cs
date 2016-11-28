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

        [Route("/api/hello")]
        public string Hello()
        {
            return "Hello";
        }

        [Route("/api/packages")]
        public IEnumerable<Package> Packages()
        {
            return _client.Query<Package>();
        }

        [HttpPost]
        [Route("/api/package/{name}")]
        public HttpStatusCode SetPackage(string name, Package[] dependencies)
        {
            return HttpStatusCode.OK;
        }
    }
}
