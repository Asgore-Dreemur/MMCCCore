using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MMCCCore.Core.Model.Core;

namespace MMCCCore.Core.Model.GameAssemblies
{
    public class ForgeVersionModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("build")]
        public int Build { get; set; }
        [JsonProperty("__v")]
        public int __V { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("modified")]
        public string ModifiedTime { get; set; }
        [JsonProperty("mcversion")]
        public string MCVersion { get; set; }

    }

    public class ForgeInstallProfileModel
    {
        [JsonProperty("__comment__")]
        public List<string> Comment { get; set; } = new List<string>();
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("json")]
        public string Json { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("data")]
        public Dictionary<string, Dictionary<string, string>> Data { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        [JsonProperty("processors")]
        public List<ForgeProcessorModel> Processors { get; set; } = new List<ForgeProcessorModel>();
        [JsonProperty("libraries")]
        public List<MCLibraryFileModel> Libraries { get; set; } = new List<MCLibraryFileModel>();
    }

    public class ForgeProcessorModel
    {
        [JsonProperty("sides")]
        public List<string> Sides { get; set; } = new List<string>();
        [JsonProperty("jar")]
        public string Jar { get; set; }
        [JsonProperty("classpath")]
        public List<string> ClassPath { get; set; } = new List<string>();
        [JsonProperty("args")]
        public List<string> Args { get; set; } = new List<string>();
        [JsonProperty("outputs")]
        public Dictionary<string, string> Outputs { get; set; } = new Dictionary<string, string>();
    }
}
