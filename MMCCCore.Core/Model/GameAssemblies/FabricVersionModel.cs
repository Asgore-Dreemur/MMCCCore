using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Core.Model.GameAssemblies
{
    public class FabricVersionModel
    {
        [JsonProperty("loader")]
        public FabricLoaderModel Loader { get; set; }
        [JsonProperty("intermediary")]
        public FabricLoaderModel Intermediary { get; set; }
        [JsonProperty("launcherMeta")]
        public FabricMetaModel LauncherMeta { get; set; }
    }
    public class FabricLoaderModel{
        [JsonProperty("separator")]
        public string Separator { get; set; }
        [JsonProperty("build")]
        public int Build { get; set; }
        [JsonProperty("maven")]
        public string Maven { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("stable")]
        public bool Stable { get; set; }
    }
    public class FabricIntermediaryModel
    {
        [JsonProperty("maven")]
        public string Maven { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("stable")]
        public bool Stable { get; set; }

    }
    public class FabricMetaModel
    {
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("libraries")]
        public FabricMetaLibrariesModel Libraries { get; set; }
        [JsonProperty("mainClass")]
        public JToken MainClass { get; set; }
    }
    public class FabricMetaLibrariesModel
    {
        [JsonProperty("client")]
        public List<FabricLibraryModel> Client { get; set; } = new List<FabricLibraryModel>();
        [JsonProperty("common")]
        public List<FabricLibraryModel> Common { get; set; } = new List<FabricLibraryModel>();
    }
    public class FabricLibraryModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class FabricMainClassModel
    {
        [JsonProperty("client")]
        public string Client { get; set; }
        [JsonProperty("server")]
        public string Server { get; set; }
    }
}
