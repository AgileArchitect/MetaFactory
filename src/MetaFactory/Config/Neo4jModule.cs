using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Neo4jClient;
using NeoApi.Logic.Graph;

namespace NeoApi.Config
{
    public class Neo4jModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var neo4j = Environment.GetEnvironmentVariable("NEO4J_HOST") ?? "localhost";

            Console.WriteLine($"Using neo4j at http://{neo4j}:7474");

            builder.RegisterType<GraphLogic>();

            builder.Register(
                    (c,m) =>
                        NeoServerConfiguration.GetConfiguration(
                            new Uri($"http://{neo4j}:7474/db/data"), "neo4j", "trustno1"))
                .As<NeoServerConfiguration>()
                .SingleInstance();

            builder.Register((c, m) => new GraphClientFactory(c.Resolve<NeoServerConfiguration>()))
                .As<GraphClientFactory>()
                .SingleInstance();

            builder.Register((c, m) => c.Resolve<GraphClientFactory>().Create()).As<IGraphClient>();

        }
    }
}
