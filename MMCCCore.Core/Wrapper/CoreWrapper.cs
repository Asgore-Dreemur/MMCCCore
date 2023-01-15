using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.Core;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using MMCCCore.Core.Module.APIManager;

namespace MMCCCore.Core.Wrapper
{
    public class CoreWrapper
    {
        private static WebClient WebClient = new WebClient();
        public static MCVersionListModel GetMCVersions()
        {
            if (DownloadAPIManager.Current == null) throw new Exception("未知的下载源");
            string GetAddr = DownloadAPIManager.Current.VersionManifest;
            string ResStr = WebClient.DownloadString(GetAddr);
            return JsonConvert.DeserializeObject<MCVersionListModel>(ResStr);
        }

        public static List<LocalGameInfoModel> GetMCVersionsFromDir(string GameDir)
        {
            List<LocalGameInfoModel> MCVersionList = new List<LocalGameInfoModel>();
            string GameVersionsDir = Path.Combine(GameDir, "versions");
            if (!Directory.Exists(GameVersionsDir)) return null;
            DirectoryInfo VersionsDirInfo = new DirectoryInfo(GameVersionsDir);
            foreach(DirectoryInfo VersionDirInfo in VersionsDirInfo.GetDirectories())
            {
                string VersionJsonPath = Path.Combine(VersionDirInfo.FullName, VersionDirInfo.Name + ".json");
                if (!File.Exists(VersionJsonPath)) continue;
                try
                {
                    LocalMCVersionJsonModel MCVersionInfo = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(new StreamReader(VersionJsonPath).ReadToEnd());
                    LocalGameInfoModel VersionInfo = new LocalGameInfoModel();
                    VersionInfo.VersionJson = MCVersionInfo;
                    VersionInfo.Id = MCVersionInfo.Id;
                    VersionInfo.Time = DateTime.Parse(MCVersionInfo.Time);
                    VersionInfo.GameRootDir = GameDir;
                    VersionInfo.APIType = GetCoreModAPIType(VersionInfo);
                    VersionInfo.VersionType = GetCoreVersionType(VersionInfo);
                    MCVersionList.Add(VersionInfo);
                }
                catch(JsonException) { continue; }
            }
            return MCVersionList;
        }

        public static bool IsExistsVersion(string GameDir, string VersionName)
        {
            string VersionDir = Path.Combine(GameDir, "versions", VersionName);
            string VersionJsonDir = Path.Combine(VersionDir, VersionName + ".json");
            return Directory.Exists(VersionDir) && File.Exists(VersionJsonDir);
        }

        public static LocalGameInfoModel GetCoreForId(string GameDir, string id)
        {
            string GameVersionsDir = Path.Combine(GameDir, "versions");
            if (!Directory.Exists(GameVersionsDir)) return null;
            DirectoryInfo VersionsDirInfo = new DirectoryInfo(GameVersionsDir);
            foreach (DirectoryInfo VersionDirInfo in VersionsDirInfo.GetDirectories())
            {
                string VersionJsonPath = Path.Combine(VersionDirInfo.FullName, VersionDirInfo.Name + ".json");
                if (!File.Exists(VersionJsonPath)) continue;
                try
                {
                    LocalMCVersionJsonModel MCVersionInfo = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(new StreamReader(VersionJsonPath).ReadToEnd());
                    LocalGameInfoModel VersionInfo = new LocalGameInfoModel();
                    if(MCVersionInfo.Id == id)
                    {
                        VersionInfo.VersionJson = MCVersionInfo;
                        VersionInfo.Id = MCVersionInfo.Id;
                        VersionInfo.Time = DateTime.Parse(MCVersionInfo.Time);
                        VersionInfo.GameRootDir = GameDir;
                        VersionInfo.APIType = GetCoreModAPIType(VersionInfo);
                        VersionInfo.VersionType = GetCoreVersionType(VersionInfo);
                        return VersionInfo;
                    }
                }
                catch (JsonException) { continue; }
            }
            return null;
        }

        public static GameAPIType GetCoreModAPIType(LocalGameInfoModel GameCore)
        {
            if (GameCore.VersionJson.InheritsFrom != null)
            {
                if (GameCore.VersionJson.MainClass.Contains("net.fabricmc.loader")) return GameAPIType.Fabric;
                else return GameAPIType.Forge;
            }
            else
            {
                if (GameCore.VersionJson.Arguments == null) return GameAPIType.Vanilla;
                else
                {
                    if (GameCore.VersionJson.Arguments.Game.Count >= 2)
                    {
                        if (GameCore.VersionJson.Arguments.Game.Find(i => i.ToString().Contains("com.mumfrey.liteloader")) != default) return GameAPIType.LiteLoader;
                        else return GameAPIType.Vanilla;
                    }
                    else return GameAPIType.Vanilla;
                }
            }
        }

        public static void GenerateLauncherProfile(string GameDir){
            File.WriteAllText(Path.Combine(GameDir, "launcher_profiles.json"), "{}");
        }

        public static GameVersionType GetCoreVersionType(LocalGameInfoModel GameCore)
        {
            switch (GameCore.VersionJson.Type)
            {
                case "release":
                    {
                        return GameVersionType.Release;
                    }
                case "snapshot":
                    {
                        return GameVersionType.Snapshot;
                    }
                case "old_beta":
                    {
                        return GameVersionType.Beta;
                    }
                case "old_alpha":
                    {
                        return GameVersionType.Alpha;
                    }
                default:return default;
            }
        }
    }
}
