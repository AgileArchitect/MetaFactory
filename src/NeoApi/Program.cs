using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using NeoApi.Extensions;
using NeoApi.Model;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace NeoApi
{
    public class Startup
    {
        public IContainer ApplicationContainer { get; private set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });


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
        public void Configure(IApplicationBuilder app,ILoggerFactory loggerFactory, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole();

            app.UseSwagger();
            app.UseSwaggerUi(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Neo API v1"));
            app.UseMvc();
            appLifetime.ApplicationStopped.Register(() => ApplicationContainer.Dispose());
        }
    }

    public class Service
    {
        private IWebHost _host;

        public void Start()
        {
            _host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                //                .UseUrls("http://+:5000")
                .UseStartup<Startup>()
                .Build();


            _host.Start();
        }

        public void Stop()
        {
            _host.Dispose();
        }
    }

    public class Program
    {


        public static void Main(string[] args)
        {
            var service = new Service();
            service.Start();

            if (args.Length > 2 && args[0] == "generate")
            {
                var url = args[1];
                var destination = args[2];
                var result = new HttpClient().GetAsync(url).Result;
                var body = result.Content.ReadAsStringAsync().Result;
                File.WriteAllText("swagger.json", body);
                service.Stop();
            }
            else
            {

                Console.WriteLine("Press a key to end..");
                Console.ReadKey();
                service.Stop();
            }
        }
    }

    public class MyLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger();
        }
    }

    public class MyLogger : ILogger, IDisposable
    {

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return BeginScope(state);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState,Exception,string> formatter)
        {
            Console.WriteLine(formatter(state,exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }
    }

}
