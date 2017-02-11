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

            builder.RegisterType<GraphLogic>();

            var config = NeoServerConfiguration.GetConfiguration(new Uri("http://NCPRDMON02.ncontrol.local:7474/db/data"), "neo4j", "trustno1");

            builder.RegisterInstance(config);

            
            var factory = new GraphClientFactory(config);

            builder.RegisterInstance(factory);
            builder.Register((c, m) => c.Resolve<GraphClientFactory>().Create()).As<IGraphClient>();

        }
    }
}
