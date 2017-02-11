using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

namespace NeoApi.Controllers
{
    [Route("/api/query")]
    public class QueryController : ControllerBase
    {
        private readonly IGraphClient _client;

        public QueryController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        [Route("graph/{cypher}/{ret}")]
        [Produces(typeof(Graph))]
        public async Task<Graph> Query([FromRoute ]string cypher, string ret)
        {
            cypher = cypher.Replace(ret, "schwami");
            var nodes = await _client.Cypher.Match(cypher).Return(schwami => schwami.As<Node>()).ResultsAsync;

            return new Graph()
            {
                Nodes = nodes.ToArray(),
                Edges = new Edge[] {}
            };
        }
    }
}
