using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMCCCore.Core.Model.GameAssemblies
{
    public class LiteLoaderVersionModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("mcversion")]
        public string MCVersion { get; set; }
        [JsonProperty("hash")]
        public string Hash { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("__v")]
        public string __V { get; set; }
        [JsonProperty("build")]
        public LiteLoaderBuildModel Build { get; set; }
    }
    public class LiteLoaderBuildModel
    {
        [JsonProperty("tweakClass")]
        public string TweakClass { get; set; }
        [JsonProperty("stream")]
        public string Stream { get; set; }
        [JsonProperty("file")]
        public string FileName { get; set; }
        [JsonProperty("libraries")]
        public List<LiteLoaderLibraryModel> Libraries { get; set; } = new List<LiteLoaderLibraryModel>();
    }
    public class LiteLoaderLibraryModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
