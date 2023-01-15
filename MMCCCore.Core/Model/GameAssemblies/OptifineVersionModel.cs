using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMCCCore.Core.Model.GameAssemblies
{
    public class OptifineVersionModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("mcversion")]
        public string MCVersion { get; set; }
        [JsonProperty("patch")]
        public string Patch { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("__v")]
        public string __V { get; set; }
        [JsonProperty("filename")]
        public string FileName { get; set; }
    }
}
