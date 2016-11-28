using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using NeoApi.Extensions;
using NeoApi.Model;
using Newtonsoft.Json;

namespace NeoApi
{
    public class Startup
    {
        public IContainer ApplicationContainer { get; private set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Create the container builder.
            var builder = new ContainerBuilder();

            // Register dependencies, populate the services from
            // the collection, and build the container. If you want
            // to dispose of the container at the end of the app,
            // be sure to keep a reference to it as a property or field.
            //            builder.RegisterType<MyType>().As<IMyType>();

            builder.RegisterAssemblyModules(Assembly.GetEntryAssembly());

            builder.Populate(services);
            

            ApplicationContainer = builder.Build();

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app,ILoggerFactory loggerFactory,IApplicationLifetime appLifetime)
        {
            app.UseMvc();
            appLifetime.ApplicationStopped.Register(() => ApplicationContainer.Dispose());
        }
    }

    public class Program
    {


        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        public static void DoSomeStuff()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "trustno1");
            client.Connect();

            var package = new Package { Name = "MyPackage", Version = "1.0.2" };

            var p1 = client.Store(package);

            var results = client.Query<Package>();


            Console.WriteLine(results);
        }

        public class Person
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }


    }
}
