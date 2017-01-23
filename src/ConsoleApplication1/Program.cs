using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Neo4jClient;

namespace ConsoleApplication1
{
    public static class ExtensionsPackageVersion
    {
        public static string Selector(this PackageVersion pv, string label)
        {
            return $"({label}:Package {{ Name: '{pv.Package}', Version: '{pv.Version}'}})";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var packages = new Program().GetPackageInfos("http://packages.ncontrol.local/nuget/Packages");

            packages.AsParallel().ForAll(p =>
            {
                Update(p);
            });
        }

        public static void Update(PackageInfo p)
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "trustno1");
            client.Connect();
//            foreach (var p in packages)
//            {
                Console.WriteLine($"Updating package {p.Package} {p.Version}");
                client.Cypher.Merge(p.Selector("p")).ExecuteWithoutResults();
                foreach (var d in p.Dependencies)
                {
                    client.Cypher.Merge(d.Selector("d")).ExecuteWithoutResults();
                    client.Cypher.Match(p.Selector("p"))
                            .Match(d.Selector("d"))
                            .Merge("(d)-[:DependsOn]-(p)")
                            .ExecuteWithoutResults();
                }
//            }

        }

        public IEnumerable<PackageInfo> GetPackageInfos(string url)
        {
            var next = url;
            var result = new List<PackageInfo>();

            while (next != null)
            {
                next = GetNextAndLoadPackagesInfoes(result, next);
            }
            return result;
        }

        public string GetNextAndLoadPackagesInfoes(IList<PackageInfo> result, string url)
        {
            HttpClient client = new HttpClient();

            var content =
                client.GetAsync(url)
                    .Result.Content.ReadAsStringAsync().Result;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            var root = doc.DocumentElement;


            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("f", "http://www.w3.org/2005/Atom");
//            nsmgr.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
//            nsmgr.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");

            var entries = root.SelectNodes("/f:feed/f:entry", nsmgr);
            var next = root.SelectSingleNode("/f:feed/f:link[@rel='next']", nsmgr);

            for( int i = 0; i < entries.Count; i++ )
            {
                var entry = entries[i];
                var name = entry["title"].InnerText;
                var version = entry["m:properties"]["d:Version"].InnerText;
                var dependencies = entry["m:properties"]["d:Dependencies"].InnerText;

                var deps = new PackageVersion[] {};
                if (!string.IsNullOrEmpty(dependencies))
                {
                    deps = dependencies.Split('|').Select(dep =>
                    {
                        var tuple = dep.Split(':');
                        return new PackageVersion
                        {
                            Package = tuple[0],
                            Version = tuple[1]
                        };
                    }).ToArray();
                }

                result.Add(new PackageInfo
                {
                    Package = name,
                    Version = version,
                    Dependencies = deps
                });
            }

            return next?.Attributes["href"].Value;
        }

    }

    public class PackageInfo : PackageVersion
    {
        public ICollection<PackageVersion> Dependencies { get; set; }
    }

    public class PackageVersion
    {
        public string Package { get; set; }
        public string Version { get; set; }
    }
}
