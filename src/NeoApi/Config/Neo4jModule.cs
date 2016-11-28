using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Neo4jClient;

namespace NeoApi.Config
{
    public class Neo4jModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var configuration = NeoServerConfiguration.GetConfiguration(new Uri("http://localhost:7474/db/data"),
                "neo4j", "trustno1");
            
            var factory = new GraphClientFactory(configuration);

            builder.RegisterInstance(factory);
            builder.Register((c, m) => c.Resolve<GraphClientFactory>().Create()).As<IGraphClient>();

        }
    }
}
