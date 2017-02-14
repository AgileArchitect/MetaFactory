using System;
using Microsoft.AspNetCore.Mvc;
using NeoApi.Logic.Graph;

namespace NeoApi.Controllers
{
    [Route("/api/package")]
    public class PackageController : ControllerBase
    {
        private readonly GraphLogic _logic;

        public PackageController(GraphLogic logic)
        {
            _logic = logic;
        }

        [Route("dependency")]
        [HttpPost]
        public void Dependency([FromBody] PackageDependency dependency)
        {
            var packages = new Package[] {dependency.From, dependency.To};
            foreach (var p in packages)
            {
                var version = new Version(p.Version).ToString();
                p.Version = version;
                p.Id = $"{p.Name}.{p.Version}";
                _logic.Merge(p);
            }

            _logic.Relate(new TypedEdge<Package,Package>
            {
                From = dependency.From,
                To = dependency.To,
                Label = "depends_on"
            });
        }
    }

    public class PackageDependency : Identity
    {
        public Package From { get; set; }
        public Package To { get; set; }
    }

    public class Package : Identity
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}