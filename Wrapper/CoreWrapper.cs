using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.Core;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Wrapper
{
    public class CoreWrapper
    {
        private static WebClient WebClient = new WebClient();
        public static MCVersionListModel GetMCVersions(GameSources GetSource)
        {
            string GetAddr = GetSource == GameSources.Bmclapi ? "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json" : GetSource == GameSources.Mcbbs ? "https://download.mcbbs.net/mc/game/version_manifest_v2.json" : "http://launchermeta.mojang.com/mc/game/version_manifest_v2.json";
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
                    VersionInfo.GameDir = GameDir;
                    switch (MCVersionInfo.Type)
                    {
                        case "release":
                            {
                                VersionInfo.VersionType = GameVersionType.Release;
                                break;
                            }
                        case "snapshot":
                            {
                                VersionInfo.VersionType = GameVersionType.Snapshot;
                                break;
                            }
                        case "old_beta":
                            {
                                VersionInfo.VersionType = GameVersionType.Beta;
                                break;
                            }
                        case "old_alpha":
                            {
                                VersionInfo.VersionType = GameVersionType.Alpha;
                                break;
                            }
                    }
                    if (MCVersionInfo.InheritsFrom != null)
                    {
                        if (MCVersionInfo.MainClass.Contains("net.fabricmc.loader.impl.launch.knot.KnotClient")) VersionInfo.APIType = GameAPIType.Fabric;
                        else VersionInfo.APIType = GameAPIType.Forge;
                    }
                    else
                    {
                        if (MCVersionInfo.Arguments == null) VersionInfo.APIType = GameAPIType.Vanilla;
                        else
                        {
                            if (MCVersionInfo.Arguments.Game.Count != 0)
                            {
                                if (MCVersionInfo.Arguments.Game[1].ToString().Equals("com.mumfrey.liteloader.launch.LiteLoaderTweaker")) VersionInfo.APIType = GameAPIType.LiteLoader;
                                else VersionInfo.APIType = GameAPIType.Vanilla;
                            }
                            else VersionInfo.APIType = GameAPIType.Vanilla;
                        }
                    }
                    if (VersionInfo.APIType == GameAPIType.Optifine || VersionInfo.APIType == GameAPIType.Forge || VersionInfo.APIType == GameAPIType.Fabric)
                    {
                        MCVersionList.Add(VersionInfo);
                        continue;
                    }
                    foreach (MCLibraryFileModel LibraryInfo in MCVersionInfo.Libraries)
                    {
                        if (LibraryInfo.Name.StartsWith("optifine"))
                        {
                            VersionInfo.APIType = GameAPIType.Optifine;
                            MCVersionList.Add(VersionInfo);
                            break;
                        }
                        else if (LibraryInfo.Name.StartsWith("net.fabricmc"))
                        {
                            VersionInfo.APIType = GameAPIType.Fabric;
                            MCVersionList.Add(VersionInfo);
                            break;
                        }

                        else if (LibraryInfo.Name.StartsWith("net.minecraftforge"))
                        {
                            VersionInfo.APIType = GameAPIType.Forge;
                            MCVersionList.Add(VersionInfo);
                            break;
                        }
                    }
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
            string VersionJarDir = Path.Combine(VersionDir, VersionName + ".jar");
            return Directory.Exists(VersionDir) && File.Exists(VersionJsonDir) && File.Exists(VersionJarDir);
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
                        VersionInfo.GameDir = GameDir;
                        return VersionInfo;
                    }
                }
                catch (JsonException) { continue; }
            }
            return null;
        }
    }
}
