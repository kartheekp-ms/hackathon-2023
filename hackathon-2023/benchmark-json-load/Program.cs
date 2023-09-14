using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace benchmark_json_load
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
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

            var jObject = JObject.Load(jsonReader, DefaultLoadSettings);
            var versionToken = jObject["version"];
            int version = versionToken.Value<int>();

            return new LockFile() { Version = version };
        }

        [Benchmark]
        public LockFile Deserialize()
        {
            using var assetsFileStream = File.OpenRead(_filePath);
            using var textReader = new StreamReader(assetsFileStream);
            using var jsonReader = new JsonTextReader(textReader);

            JsonSerializer serializer = new();
            var lockFile = serializer.Deserialize<LockFile>(jsonReader);

            return lockFile;
        }
    }
}
