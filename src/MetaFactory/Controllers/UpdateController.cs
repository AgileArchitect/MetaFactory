using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using NeoApi.Model;
using NeoApi.Extensions;
using NeoApi.Logic.Graph;


namespace NeoApi.Controllers
{


    [Route("/api/node")]
    public class NodeController : ControllerBase
    {
        private readonly GraphLogic _logic;
        private readonly IGraphClient _graphClient;

        public NodeController(GraphLogic logic, IGraphClient graphClient)
        {
            _logic = logic;
            _graphClient = graphClient;
        }

        [HttpPost]
        [Route("update/node")]
        [Produces(typeof(string))]
        public void Update([FromBody] Node node)
        {
            _logic.Merge(node);
        }

        [HttpPost]
        [Route("update/subgraph")]
        [Produces(typeof(string))]
        public void Update([FromBody] Graph graph)
        {
            var nodes = new Dictionary<string, Node>();

            foreach (var node in graph.Nodes)
            {
                _logic.Merge(node);
                nodes.Add(node.Id, node);
            }

            foreach( var edge in graph.Edges)
            {
                _logic.Relate(new TypedEdge<Node,Node>
                {
                   From = nodes[edge.From],
                   To = nodes[edge.To],
                   Label = edge.Label
                });
            }
        }

        [HttpPost]
        [Route("update/{label}/{fromType}/{toType}")]
        [Produces(typeof(Graph))]
        public Graph QueryRelationship([FromRoute] string label, [FromRoute] string fromType, [FromRoute] string toType)
        {
            var data = _graphClient.Cypher.Match($"(n1:{fromType})-[:{label}]->(n2:{toType})")
                .Return((n1, n2) => new
                {
                    Node = n1.As<Node>(),
                    Relations = n2.CollectAs<Node>()
                }).Results;

            var nodes = new List<Node>();
            var edges = new List<Edge>();
            foreach (var n in data)
            {
                nodes.Add(n.Node);
                foreach (var other in n.Relations)
                {
                    nodes.Add(other);
                    edges.Add(new Edge
                    {
                        From = n.Node.Id,
                        To = other.Id,
                        Label = label
                    });
                }
            }

            var graph = new Graph()
            {
                Nodes = nodes,
                Edges = edges
            };

            return graph;
        }
    }

    public class Edge
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Label { get; set; }
    }

    public class Node : Identity
    {
        public string Label { get; set; }
    }

    public class Graph
    {
        public IEnumerable<Node> Nodes { get; set; }
        public IEnumerable<Edge> Edges { get; set; }
    }
}
