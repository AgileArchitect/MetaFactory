using System.Collections.Generic;
using System.Linq;
using Neo4jClient;

namespace NeoApi.Extensions
{
    public static class ExtensionsToGraphClient
    {
        public static T Store<T>(this IGraphClient client, T theT)
        {
            var type = typeof(T).Name;
            var cypher = client.Cypher.Create($"(o:{type} {{ object }})").WithParam("object", theT).Return<T>("o");
            return cypher.Results.FirstOrDefault();
        }

        public static IEnumerable<T> Query<T>(this IGraphClient client)
        {
            var type = typeof(T).Name;
            return client.Cypher.Match($"(p:{type})").Return<T>("p").Results;
        }
    }
}