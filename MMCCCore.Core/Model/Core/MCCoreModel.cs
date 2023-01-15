using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Module.GameAssemblies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Core.Model.Core
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
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public string Time { get; set; }
        [JsonProperty("libraries", NullValueHandling = NullValueHandling.Ignore)]
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
        [JsonProperty("clientVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientVersion { get; set; }
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
        [JsonProperty("downloads", NullValueHandling = NullValueHandling.Ignore)]
        public GameLibraryDownloadModel Downloads { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("natives", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Natives { get; set; }

        [JsonProperty("rules", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<LibraryRules> Rules { get; set; }
        [JsonProperty("checksums", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> CheckSums { get; set; }
        [JsonProperty("clientreq", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ClientReq { get; set; }
    }

    public class MCFileModel
    {
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }
        [JsonProperty("sha1", NullValueHandling = NullValueHandling.Ignore)]
        public string Sha1 { get; set; }
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int Size { get; set; }
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
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
        [JsonProperty("artifact", NullValueHandling = NullValueHandling.Ignore)]
        public MCFileModel Artifact { get; set; }

        [JsonProperty("classifiers", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, MCFileModel> Classifiers { get; set; }
    }

    public class GameAssetsIndexModel
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        [JsonProperty("sha1", NullValueHandling = NullValueHandling.Ignore)]
        public string Sha1 { get; set; }
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int Size { get; set; }
        [JsonProperty("totalSize", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalSize { get; set; }
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
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

    public enum GameVersionType
    {
        Release = 0,
        Snapshot = 1,
        Beta = 2,
        Alpha = 3
    }

    public enum GameAPIType
    {
        None = 0,
        Vanilla = 1,
        Optifine = 2,
        Forge = 3,
        Fabric = 4,
        LiteLoader = 5
    }
}
