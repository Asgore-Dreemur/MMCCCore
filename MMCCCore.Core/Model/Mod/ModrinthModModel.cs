using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMCCCore.Core.Model.Mod
{
    public class ModrinthModModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("data")]
        public ModrinthDataModel Data { get; set; }
    }
    public class ModrinthDataModel
    {
        [JsonProperty("hits")]
        public List<ModrinthModInfoModel> Hits { get; set; } = new List<ModrinthModInfoModel>();
        [JsonProperty("offset")]
        public int Offset { get; set; }
        [JsonProperty("limit")]
        public int Limit { get; set; }
        [JsonProperty("total_hits")]
        public int TotalHits { get; set; }
    }
    public class ModrinthModInfoModel
    {
        [JsonProperty("project_id")]
        public string PorjectId { get; set; }
        [JsonProperty("project_type")]
        public string ProjectType { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("categories")]
        public List<string> Categories { get; set; } = new List<string>();
        [JsonProperty("display_categories")]
        public List<string> DisplayCategories { get; set; } = new List<string>();
        [JsonProperty("versions")]
        public List<string> Versions { get; set; } = new List<string>();
        [JsonProperty("downloads")]
        public int Downloads { get; set; }
        [JsonProperty("follows")]
        public int Follows { get; set; }
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
        [JsonProperty("date_created")]
        public string DateCreated { get; set; }
        [JsonProperty("date_modified")]
        public string DateModified { get; set; }
        [JsonProperty("latest_version")]
        public string LatestVersion { get; set; }
        [JsonProperty("license")]
        public string License { get; set; }
        [JsonProperty("client_side")]
        public string ClientSide { get; set; }
        [JsonProperty("server_side")]
        public string ServerSide { get; set; }
        [JsonProperty("gallery")]
        public List<string> Gallery { get; set; } = new List<string>();
    }
    public class ModrinthModVersionsModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("data")]
        public List<ModrinthModVersionInfoModel> Data { get; set; } = new List<ModrinthModVersionInfoModel>();
    }
    public class ModrinthModVersionInfoModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("files")]
        public List<ModrinthFileModel> Files { get; set; } = new List<ModrinthFileModel>();
        [JsonProperty("loaders")]
        public List<string> Loaders { get; set; } = new List<string>();
        [JsonProperty("date_published")]
        public string DatePublished { get; set; }
        [JsonProperty("game_versions")]
        public List<string> GameVersions { get; set; } = new List<string>();
        [JsonProperty("downloads")]
        public int Downloads { get; set; }
    }
    public class ModrinthFileModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("hashes")]
        public ModrinthFileHashModel Hashes { get; set; }
        [JsonProperty("primary")]
        public bool Primary { get; set; }
        [JsonProperty("filename")]
        public string FileName { get; set; }
    }
    public class ModrinthFileHashModel
    {
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonProperty("sha512")]
        public int Sha512 { get; set; }
    }
    public static class ModrinthModFilterList
    {
        public static string Adventure { get; } = "adventure";
        public static string Cursed { get; } = "cursed";
        public static string Decoration { get; } = "decoration";
        public static string Equipment { get; } = "equipment";
        public static string Food { get; } = "food";
        public static string GameMechanics{get;} = "game-mechanics";
        public static string Library { get; } = "library";
        public static string Magic { get; } = "magic";
        public static string Management { get; } = "management";
        public static string Minigame { get; } = "minigame";
        public static string Mobs { get; } = "mobs";
        public static string Optimization { get; } = "optimization";
        public static string Storage { get; } = "storage";
        public static string Technology { get; } = "technology";
        public static string Transportation { get; } = "transportation";
        public static string Utility { get; } = "utility";
        public static string Worldgen { get; } = "worldgen";
    }
    public class ModrinthSearchModel
    {
        public string Query { get; set; }
        public int limit { get; set; } = -1;
        public string index { get; set; }
        public string Version { get; set; }
        public string ModFilter { get; set; }
        public string ModLoaderType { get; set; }
    }
    public static class ModSearchIndexes
    {
        public static string Relevance { get; } = "relevance";
        public static string Downloads { get; } = "downloads";
        public static string Follows { get; } = "follows";
        public static string Newest { get; } = "newest";
        public static string Updated { get; } = "updated";
    }

    public static class ModLoaderAPIs
    {
        public static string Forge { get; } = "forge";
        public static string Fabric { get; } = "fabric";
    }
    public static class ModPackFilterList
    {
        public static string Adventure = "adventure";
        public static string Challenging = "challenging";
        public static string Combat = "combat";
        public static string KitchenSink = "kitchen-sink";
        public static string Lightweight = "lightweight";
        public static string Magic = "magic";
        public static string Multiplayer = "multiplayer";
        public static string Optimization = "optimization";
        public static string Quests = "quests";
        public static string Technology = "technology";
    }
}
