using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace benchmark_json_load
{
    public class LockFile
    {
        public int Version { get; set; }

        [JsonConverter(typeof(ProjectFileDependencyGroupConverter))]
        public IList<ProjectFileDependencyGroup> ProjectFileDependencyGroups { get; set; } = new List<ProjectFileDependencyGroup>();
    }

    public class ProjectFileDependencyGroup
    {
        public ProjectFileDependencyGroup(string frameworkName, IEnumerable<string> dependencies)
        {
            FrameworkName = frameworkName;
            Dependencies = dependencies;
        }

        public string FrameworkName { get; }

        public IEnumerable<string> Dependencies { get; }
    }


    public class ProjectFileDependencyGroupConverter : JsonConverter<IList<ProjectFileDependencyGroup>>
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, IList<ProjectFileDependencyGroup> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IList<ProjectFileDependencyGroup> ReadJson(JsonReader reader, Type objectType, IList<ProjectFileDependencyGroup> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var groups = new List<ProjectFileDependencyGroup>();

            foreach (var prop in jObject.Properties())
            {
                var group = new ProjectFileDependencyGroup(prop.Name, prop.Value.ToObject<IList<string>>());
                groups.Add(group);
            }

            return groups;
        }
    }

}
