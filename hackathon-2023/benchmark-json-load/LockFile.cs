using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime.Utilities;

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
            var result = new List<ProjectFileDependencyGroup>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string framework = (string)reader.Value;

                    reader.Read(); // Move to the start of the array
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        List<string> dependencies = new();

                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.EndArray)
                            {
                                result.Add(new ProjectFileDependencyGroup(framework, dependencies));
                                break;
                            }

                            string dependency = (string)reader.Value;
                            dependencies.Add(dependency);
                        }
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return result;
        }
    }

}
