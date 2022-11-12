using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Module.GameAssemblies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Model.Core
{
    public class MCVersionListModel
    {
        [JsonProperty("latest")]
        public MCLatestVersionModel LastVersions { get; set; }
        [JsonProperty("versions")]
        public List<MCVersionModel> AllVersions { get; set; }
    }
    public class MCVersionModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("releaseTime")]
        public string ReleaseTime { get; set; }
        [JsonProperty("url")]
        public string JsonUrl { get; set; }
        
    }
    public class MCLatestVersionModel
    {
        [JsonProperty("release")]
        public string ReleaseVersion { get; set; }
        [JsonProperty("snapshot")]
        public string SnapshotVersion { get; set; }
    }
    public class LocalMCVersionJsonModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("mainClass")]
        public string MainClass { get; set; }
        [JsonProperty("minimumLauncherVersion")]
        public int MinimumLauncherVersion { get; set; }
        [JsonProperty("releaseTime")]
        public string ReleaseTime { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("libraries")]
        public List<MCLibraryFileModel> Libraries { get; set; } = new List<MCLibraryFileModel>();
        [JsonProperty("inheritsFrom", NullValueHandling = NullValueHandling.Ignore)]
        public string InheritsFrom { get; set; }
        [JsonProperty("arguments")]
        public GameArgumentsModel Arguments { get; set; }
        [JsonProperty("downloads", NullValueHandling = NullValueHandling.Ignore)]
        public GameDownloadsModel Downloads { get; set; }
        [JsonProperty("assetIndex")]
        public GameAssetsIndexModel AssetIndex { get; set; }
        [JsonProperty("minecraftArguments", NullValueHandling = NullValueHandling.Ignore)]
        public string MinecraftArguments { get; set; }
    }
    public class GameDownloadsModel
    {
        [JsonProperty("client")]
        public GameDownloadClientModel Client { get; set; }
    }
    public class GameDownloadClientModel
    {
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class MCLibraryFileModel
    {
        [JsonProperty("downloads")]
        public GameLibraryDownloadModel Downloads { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("natives")]
        public Dictionary<string, string> Natives { get; set; }

        [JsonProperty("rules")]
        public IEnumerable<LibraryRules> Rules { get; set; }
        [JsonProperty("checksums")]
        public List<string> CheckSums { get; set; }
        [JsonProperty("clientreq")]
        public bool? ClientReq { get; set; }
    }
    public class MCFileModel
    {
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
    public class LibraryRules
    {
        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }
        [JsonProperty("os", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Os { get; set; }
    }
    public class GameLibraryDownloadModel
    {
        [JsonProperty("artifact")]
        public MCFileModel Artifact { get; set; }

        [JsonProperty("classifiers")]
        public Dictionary<string, MCFileModel> Classifiers { get; set; }
    }
    public class GameLibraryArtifactModel
    {
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonProperty("size")]
        public int FileSize { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class GameAssetsIndexModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("totalSize")]
        public int TotalSize { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class GameArgumentsModel
    {
        [JsonProperty("game")]
        public List<JToken> Game { get; set; } = new List<JToken>();
        [JsonProperty("jvm")]
        public List<JToken> Jvm { get; set; } = new List<JToken>();
    }
    public class LocalGameInfoModel
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
        public GameAPIType APIType { get; set; }
        public GameVersionType VersionType { get; set; }
        public LocalMCVersionJsonModel VersionJson { get; set; }
        public string GameRootDir { get; set; }
    }
    public class MCLibraryInfo
    {
        public string Path { get; set; }
        public string Url { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }
        public bool isEnabled { get; set; }
        public string CheckSum { get; set; }
        public bool isNative { get; set; }
        public MCLibraryFileModel LibraryJson { get; set; }
    }
    public enum GameSources
    {
        Bmclapi = 0,
        Mcbbs = 1,
        Vanilla = 2
    }
    public enum GameVersionType
    {
        Release = 0,
        Snapshot = 1,
        Beta = 2,
        Alpha = 3
    }
    public enum GameAPIType
    {
        Vanilla = 0,
        Optifine = 1,
        Forge = 2,
        Fabric = 3,
        LiteLoader = 4
    }
}
