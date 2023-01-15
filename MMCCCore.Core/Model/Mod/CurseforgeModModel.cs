using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMCCCore.Core.Model.Mod
{
    public class CurseforgeModModel
    {
        [JsonProperty("data")]
        public List<ModInfoModel> Data { get; set; } = new List<ModInfoModel>();
        [JsonProperty("pagination")]
        public ModPaginationModel Pagination { get; set; }
    }
    public class CurseforgeModFilesModel
    {
        [JsonProperty("data")]
        public List<ModFilesModel> Data { get; set; } = new List<ModFilesModel>();
        [JsonProperty("pagination")]
        public ModPaginationModel Pagination { get; set; }
    }
    public class ModInfoModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("gameId")]
        public int GameId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("links")]
        public ModLinksModel Links { get; set; }
        [JsonProperty("summary")]
        public string Summary { get; set; }
        [JsonProperty("downloadCount")]
        public int DownloadCount { get; set; }
        [JsonProperty("categories")]
        public List<ModCategoriesModel> Categories { get; set; } = new List<ModCategoriesModel>();
        [JsonProperty("screenshots")]
        public List<ModScreenShotsModel> Screenshots { get; set; } = new List<ModScreenShotsModel>();
        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }
        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
        [JsonProperty("dateReleased")]
        public string DateReleased { get; set; }
        [JsonProperty("authors")]
        public List<ModAuthorModel> Author { get; set; } = new List<ModAuthorModel>();
        [JsonProperty("logo")]
        public ModScreenShotsModel Logo { get; set; }
        [JsonProperty("latestFiles")]
        public List<ModFilesModel> LatestFiles { get; set; } = new List<ModFilesModel>();
        [JsonProperty("latestFilesIndexes")]
        public List<LatestFilesIndexesModel> LatestFilesIndexes { get; set; } = new List<LatestFilesIndexesModel>();
    }
    public class ModLinksModel
    {
        [JsonProperty("websiteUrl")]
        public string WebsiteUrl { get; set; }
        [JsonProperty("wikiUrl")]
        public string WikiUrl { get; set; }
        [JsonProperty("issuesUrl")]
        public string IssuesUrl { get; set; }
        [JsonProperty("sourceUrl")]
        public string SourceUrl { get; set; }
    }
    public class ModCategoriesModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("gameId")]
        public int GameId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }
    }
    public class ModScreenShotsModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("modId")]
        public int ModId { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class ModAuthorModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class LatestFilesIndexesModel
    {
        [JsonProperty("fileId")]
        public int FileId { get; set; }
        [JsonProperty("gameVersion")]
        public string GameVersion { get; set; }
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("modLoader")]
        public int ModLoader { get; set; }
    }
    public class ModFilesModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("gameId")]
        public int GameId { get; set; }
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("fileLength")]
        public int FileLength { get; set; }
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }
        [JsonProperty("gameVersions")]
        public List<string> GameVersions { get; set; } = new List<string>();
    }
    public class ModPaginationModel
    {
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
        [JsonProperty("resultCount")]
        public int ResultCount { get; set; }
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
    }
    public class SearchModel
    {
        public string SearchFilter { get; set; }
        public int GameId { get; set; } = 432;
        public string GameVersion { get; set; }
        public int ModLoaderType { get; set; } = -1;
        public int ClassId { get; set; } = -1;
        public int CategoryId { get; set; } = -1;
    }
    public static class ModLoaderTypeList
    {
        public static int Forge { get; } = 1;
        public static int Fabric { get; } = 4;
        public static int Quilt { get; } = 5;
    }
    public static class ModClassIdList
    {
        public static int Worlds { get; } = 17;
        public static int BukkitPlugins { get; } = 5;
        public static int Customization { get; } = 4546;
        public static int Modpacks { get; } = 4471;
        public static int ResourcePacks { get; } = 12;
        public static int Addons { get; } = 4559;
        public static int Mods { get; } = 6;
    }
    public static class ModCategoryIdList
    {
        public static int Structures { get; } = 409;
        public static int PlayerTransport { get; } = 414;
        public static int Cosmetic { get; } = 424;
        public static int OresaAndResources { get; } = 408;
        public static int Energy { get; } = 417;
        public static int ArmorAndTools { get; } = 434;
        public static int Processing { get; } = 413;
        public static int TwitchIntegration { get; } = 4671;
        public static int MapAndInformation { get; } = 423;
        public static int Automation { get; } = 4843;
        public static int Utility { get; } = 5191;
        public static int WorldGen { get; } = 406;
        public static int Biomes { get; } = 407;
        public static int Adventure { get; } = 422;
        public static int EnergyAndItemTeleport { get; } = 415;
        public static int Magic { get; } = 419;
        public static int Farming { get; } = 416;
        public static int Library { get; } = 421;
        public static int Technology { get; } = 412;
        public static int Mobs { get; } = 411;
        public static int Redstone { get; } = 4558;
        public static int Server { get; } = 435;
        public static int Food { get; } = 436;
        public static int Miscellaneous { get; } = 425;
        public static int Storage { get; } = 420;
        public static int Dimensions { get; } = 410;
    }
    public static class ModPackCategoryIdList
    {
        public static int Vanilla = 5128;
        public static int Multiplayer = 4484;
        public static int MiniGame = 4477;
        public static int CombatPvP = 4483;
        public static int Exploration = 4476;
        public static int AdventureAndRPG = 4475;
        public static int FTBOfficialPack = 4487;
        public static int SmallLight = 4481;
        public static int Skyblock = 4736;
        public static int Quests = 4478;
        public static int MapBased = 4480;
        public static int Tech = 4472;
        public static int Magic = 4473;
        public static int SciFi = 4474;
        public static int ExtraLarge = 4482;
        public static int Hardcore = 4479;
    }
}
