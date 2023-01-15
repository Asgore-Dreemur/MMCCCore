using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
}
