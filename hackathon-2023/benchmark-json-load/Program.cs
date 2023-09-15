using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace benchmark_json_load
{
    internal class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run(typeof(Program).Assembly);
            //new Benchmarks().JObjectLoad();
            //new Benchmarks().Deserialize();
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.Net80)]
    public class Benchmarks
    {        
        private const string _filePath = "D:\\repos\\hackathon-2023\\hackathon-2023\\benchmark-json-load\\project.assets.json";
        private static readonly JsonLoadSettings DefaultLoadSettings = new()
        {
            LineInfoHandling = LineInfoHandling.Ignore,
            CommentHandling = CommentHandling.Ignore
        };

        [Benchmark]
        public LockFile JObjectLoad()
        {
            using var assetsFileStream = File.OpenRead(_filePath);
            using var textReader = new StreamReader(assetsFileStream);
            using var jsonReader = new JsonTextReader(textReader);
            while (jsonReader.TokenType != JsonToken.StartObject)
            {
                if (!jsonReader.Read())
                {
                    throw new InvalidDataException();
                }
            }

            //version
            var jObject = JObject.Load(jsonReader, DefaultLoadSettings);
            var versionToken = jObject["version"];
            int version = versionToken.Value<int>();

            ////projectFileDependencyGroups
            JObject projectFileDependencyGroupsJson = jObject["projectFileDependencyGroups"] as JObject;
            var projectFileDependencyGroups = new List<ProjectFileDependencyGroup>(projectFileDependencyGroupsJson.Count);
            foreach (var child in projectFileDependencyGroupsJson)
            {
                var jDependenciesArray = child.Value as JArray;
                var dependencies = new List<string>(jDependenciesArray.Count);
                foreach (var dependency in jDependenciesArray)
                {
                    dependencies.Add(dependency.Value<string>());
                }
                projectFileDependencyGroups.Add(new ProjectFileDependencyGroup(child.Key, dependencies));
            }

            return new LockFile() { Version = version };
        }

        [Benchmark]
        public LockFile Deserialize()
        {
            using var assetsFileStream = File.OpenRead(_filePath);
            using var textReader = new StreamReader(assetsFileStream);
            using var jsonReader = new JsonTextReader(textReader);

            JsonSerializer serializer = JsonSerializer.Create();
            var lockFile = serializer.Deserialize<LockFile>(jsonReader);

            return lockFile;
        }
    }
}
