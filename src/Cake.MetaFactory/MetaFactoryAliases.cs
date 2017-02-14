using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using MetaFactory.Client;
using MetaFactory.Client.Models;

namespace Cake.MetaFactory
{
    public class MetaFactoryRunner
    {
        private readonly ICakeContext _context;
        private readonly IMetaFactoryClient _client;        

        public MetaFactoryRunner(ICakeContext context, string url)
        {
            _context = context;
            _client = new MetaFactoryClient { BaseUri = new Uri(url)};
        }

        public void Dependency(PackageVersion from, PackageVersion to)
        {
            try
            {
                _client.ApiPackageDependencyPost(new PackageDependency
                {
                    FromProperty = from.ToPackage(),
                    To = to.ToPackage()
                });
                _context.Log.Information($"Reported dependency {from.Name}.{from.Version}->{to.Name}.{to.Version}");
            }
            catch (Exception e)
            {
                _context.Log.Warning($"Problem: {e.Message}");
                _context.Log.Warning($"StackTrace: {e.StackTrace}");
            }
        }
    }

    public class PackageVersion
    {
        public string Name { get; set; }
        public Version Version { get; set; }

        internal Package ToPackage()
        {
            return new Package
            {
                Name = Name,
                Version = Version.ToString(3),
                Id = $"{Name}.{Version.ToString(3)}"
            };
        }
    }

    public static class MetaFactoryAliases
    {
        [CakeMethodAlias]
        public static MetaFactoryRunner MetaFactory(this ICakeContext context, string url)
        {
            return new MetaFactoryRunner(context, url);
        }
    }
}
