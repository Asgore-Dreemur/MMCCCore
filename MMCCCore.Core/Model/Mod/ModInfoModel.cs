using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Core.Model.Mod
{
    public class ModInfo
    {
        public ModTypes Type { get; set; }
        public object Data { get; set; }
    }

    public enum ModTypes
    {
        Forge_Old = 0,
        Forge_New = 1,
        Fabric = 2
    }

    public class ForgeOldVersionMCModFileModel
    {
        [JsonProperty("modid")]
        public string ModId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("mcversion")]
        public string MCVersion { get; set; }
        [JsonProperty("authorList")]
        public List<string> AuthorList { get; set; } = new List<string>();
    }

    public class FabricVersionMCModFileModel
    {
        [JsonProperty("id")]
        public string ModId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("authors")]
        public List<string> AuthorList { get; set; } = new List<string>();
        [JsonProperty("depends")]
        public Dictionary<string, string> Depends { get; set; } = new Dictionary<string, string>();
    }

    public class ForgeNewVersionMCModProfileModel
    {
        public string ModId { get; set; }
        public string Authors { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }
}