using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Neo4jClient;
using NeoApi.Extensions;


namespace NeoApi.Logic.Graph
{
    public class Identity
    {
        public string Id { get; set; }
    }

    public class TypedEdge<TFrom,TTo>
    {
        public TFrom From { get; set; }
        public TTo To { get; set; }
        public string Label { get; set; }
    }

    public class GraphLogic
    {
        private readonly IGraphClient _client;

        public GraphLogic(IGraphClient client)
        {
            _client = client;
        }

        public void Merge<T>(T node) where T : Identity
        {
            var nodeType = GetNodeType(node);
//            _client.Cypher.Merge($"(n:{nodeType}){{params}}")
//                .WithParam("params", node).ExecuteWithoutResults();

            _client.Cypher
            .Merge($"(n:{nodeType} {{ Id: '{{id}}' }})")
            .OnCreate()
            .Set("n = {node}")
            .WithParams(new
            {
                id = node.Id,
                node
            })
            .ExecuteWithoutResults();

        }

        public void Relate<T1, T2>(TypedEdge<T1, T2> r) where T1 : Identity where T2 : Identity
        {
            var nodeType1 = GetNodeType(r.From);
            var nodeType2 = GetNodeType(r.To);

            _client.Cypher
                .Match($"(n1:{nodeType1})")
                .Where((T1 n1) => n1.Id == r.From.Id)
                .Match($"(n2:{nodeType2})")
                .Where((T2 n2) => n2.Id == r.To.Id)
                .Create($"n1-[:{r.Label}]->n2")
                .ExecuteWithoutResults();
        }

        public string GetNodeType<T>(T node) where T : Identity
        {
            var attribute = node.GetType().GetTypeInfo().GetCustomAttribute<NodeTypeAttribute>();
            if (attribute != null)
            {
                return attribute.Name;
            }
            return typeof(T).Name;
        }
    }

    public class NodeTypeAttribute : Attribute
    {
        public NodeTypeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
