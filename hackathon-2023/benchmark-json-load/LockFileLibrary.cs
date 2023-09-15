using Microsoft.DotNet.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

# nullable enable

namespace benchmark_json_load
{
    public class NuGetVersion
    {
        public Version Version { get; }
        public int Revision { get; }
        public NuGetVersion(Version version, IEnumerable<string>? releaseLabels, string? metadata, string? originalVersion)
        {
            Version = version;
            Revision = Version.Revision;
        }
    }

    public class LockFileLibrary
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public NuGetVersion Version { get; set; }

        public bool IsServiceable { get; set; }

        public string Sha512 { get; set; }

        public IList<string> Files { get; set; } = new List<string>();

        /// <summary>
        /// Relative path to the project.json file for projects
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Relative path to the msbuild project file. Ex: xproj, csproj
        /// </summary>
        public string MSBuildProject { get; set; }

        public bool? HasTools { get; set; }
    }
}
