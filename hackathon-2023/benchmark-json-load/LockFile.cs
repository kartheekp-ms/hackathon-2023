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

        [JsonProperty("libraries")]
        [JsonConverter(typeof(LockFileLibraryConverter))]
        public IList<LockFileLibrary> Libraries { get; set; } = new List<LockFileLibrary>();
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

    public class LockFileLibraryConverter : JsonConverter<IList<LockFileLibrary>>
    {
        public override IList<LockFileLibrary> ReadJson(JsonReader reader, Type objectType, IList<LockFileLibrary> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("Expected start of object.");

            var libraries = new List<LockFileLibrary>();

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName && reader.Value != null)
                {
                    // This will read the library name property (e.g., "Microsoft.Bcl.AsyncInterfaces/7.0.0")
                    string libraryName = reader.Value.ToString();

                    // Read again to move to the StartObject token of the library details
                    reader.Read();

                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        var library = new LockFileLibrary();
                        library.Name = libraryName;

                        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                        {
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                string propName = reader.Value.ToString();
                                reader.Read(); // Move to the property value.

                                switch (propName)
                                {
                                    case "sha512":
                                        library.Sha512 = reader.Value.ToString();
                                        break;
                                    case "type":
                                        library.Type = reader.Value.ToString();
                                        break;
                                    case "path":
                                        library.Path = reader.Value.ToString();
                                        break;
                                    case "hasTools":
                                        library.HasTools = (bool)reader.Value;
                                        break;
                                    case "files":
                                        library.Files = new List<string>();
                                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                                        {
                                            library.Files.Add(reader.Value.ToString());
                                        }
                                        break;
                                    default:
                                        // You can add more properties as needed, or skip any unexpected ones.
                                        break;
                                }
                            }
                        }

                        libraries.Add(library);
                    }
                }
            }

            return libraries;
        }


        public override void WriteJson(JsonWriter writer, IList<LockFileLibrary> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
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
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("Expected start of object.");

            var result = new List<ProjectFileDependencyGroup>();

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
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
            }

            return result;
        }
    }

}
