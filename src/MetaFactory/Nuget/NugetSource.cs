using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeoApi.Model;

namespace NeoApi.Nuget
{
    public class NugetSource
    {
        private readonly Uri _nugetV2Uri;

        public NugetSource(Uri nugetV2Uri)
        {
            _nugetV2Uri = nugetV2Uri;
        }
    }
}
