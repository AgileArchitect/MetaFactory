using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaFactory.Client;

namespace MetaFactory.Collector.NugetDependencies
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var basehref = args[0];
            try
            {
                var uri = new Uri(basehref);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Usage: <cmd> <uri> <packages.json>");
            }

            var client = new MetaFactory.Client.MetaFactoryClient()
            {
                BaseUri = new Uri(basehref)
            };
        }
    }
}
