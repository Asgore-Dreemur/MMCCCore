using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Model.ModAPI
{
    public class OptifineModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("mcversion")]
        public string MCVersion { get; set; }
        [JsonProperty("patch")]
        public string Patch { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("__v")]
        public int __V { get; set; }
        [JsonProperty("filename")]
        public string FileName { get; set; }
    }
}
