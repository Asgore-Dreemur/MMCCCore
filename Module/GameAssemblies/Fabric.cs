using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.Assemblies;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using MMCCCore.Model.Core;
using MMCCCore.Wrapper;
using Newtonsoft.Json.Linq;
using MMCCCore.Model.Wrapper;
using MMCCCore.Module.Minecraft;
using System.Threading;

namespace MMCCCore.Module.GameAssemblies
{
    public class Fabric : InstallerModel
    {
        private static WebClient WebClient = new WebClient();
        private int AllFileCount;
        public static List<FabricVersionModel> GetFabricVersionsFromMCVersion(string MCVersion)
        {
            string VersionStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/fabric-meta/v2/versions/loader/{MCVersion}");
            return JsonConvert.DeserializeObject<List<FabricVersionModel>>(VersionStr);
        }
        public static List<FabricVersionModel> GetFabricAllVersions()
        {
            string VersionStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/fabric-meta/v2/versions/loader/");
            return JsonConvert.DeserializeObject<List<FabricVersionModel>>(VersionStr);
        }
        public InstallerReponse InstallFabric(string GameDir, string VersionName, FabricVersionModel InstallInfo, int MaxThreadCount = 64)
        {
            try
            {
                OnProgressChanged(-1, "准备信息...");
                LocalMCVersionJsonModel VersionJson = new LocalMCVersionJsonModel();
                VersionJson.Id = VersionName;
                VersionJson.InheritsFrom = InstallInfo.Intermediary.Version;
                VersionJson.Time = DateTime.Now.ToString("yyyy-MM-dd{a}hh:mm:ss{b}zzz").Replace("{a}", "T").Replace("{b}", "");
                VersionJson.ReleaseTime = DateTime.Now.ToString("yyyy-MM-dd{a}hh:mm:ss{b}zzz").Replace("{a}", "T").Replace("{b}", "");
                VersionJson.MainClass = InstallInfo.LauncherMeta.MainClass.Type == JTokenType.Object
                    ? InstallInfo.LauncherMeta.MainClass.ToObject<Dictionary<string, string>>()["client"]
                    : string.IsNullOrEmpty(InstallInfo.LauncherMeta.MainClass.ToString())
                        ? "net.minecraft.client.main.Main"
                        : InstallInfo.LauncherMeta.MainClass.ToString();
                if (VersionJson.MainClass == "net.minecraft.client.main.Main")
                {
                    throw new Exception("无法解析主类");
                }
                VersionJson.Arguments = new GameArgumentsModel();
                VersionJson.Type = "release";
                OtherTools.CreateDir(Path.Combine(GameDir, "versions", VersionName));
                List<Task> DownloadList = new List<Task>();
                OnProgressChanged(-1, "下载支持库...");
                Stack<DownloadTaskInfo> DownloadStack = new Stack<DownloadTaskInfo>();
                if (InstallInfo.LauncherMeta.Libraries.Client.Count > 0)
                {
                    foreach (FabricLibraryModel LibraryInfo in InstallInfo.LauncherMeta.Libraries.Client)
                    {
                        string LibraryPath = Path.Combine(GameDir, "libraries", GetFabricLibraryPathFromName(LibraryInfo.Name));
                        string LibraryUrl = GetFabricLibraryUrlFromName(LibraryInfo.Name);
                        OtherTools.CreateDir(LibraryPath);
                        DownloadStack.Push(new DownloadTaskInfo
                        {
                            DestPath = LibraryPath,
                            DownloadUrl = LibraryUrl,
                            MaxTryCount = 4
                        });
                        VersionJson.Libraries.Add(new MCLibraryFileModel() { Name = LibraryInfo.Name, Url = LibraryInfo.Url });
                    }
                }
                if (InstallInfo.LauncherMeta.Libraries.Common.Count > 0)
                {
                    foreach (FabricLibraryModel LibraryInfo in InstallInfo.LauncherMeta.Libraries.Common)
                    {
                        string LibraryPath = Path.Combine(GameDir, "libraries", GetFabricLibraryPathFromName(LibraryInfo.Name));
                        string LibraryUrl = GetFabricLibraryUrlFromName(LibraryInfo.Name);
                        OtherTools.CreateDir(LibraryPath);
                        DownloadStack.Push(new DownloadTaskInfo
                        {
                            DestPath = LibraryPath,
                            DownloadUrl = LibraryUrl,
                            MaxTryCount = 4
                        });
                        VersionJson.Libraries.Add(new MCLibraryFileModel() { Name = LibraryInfo.Name, Url = LibraryInfo.Url });
                    }
                }
                Tuple<string, string> FabricLoaderInfo = GetFabricLoaderPathAndUrlFromName(InstallInfo.Loader.Maven);
                string LoaderUrl = FabricLoaderInfo.Item1;
                string LoaderPath = Path.Combine(GameDir, "libraries", FabricLoaderInfo.Item2);
                Tuple<string, string> FabricInterInfo = GetFabricInterPathAndUrlFromName(InstallInfo.Intermediary.Maven);
                string InterUrl = FabricInterInfo.Item1;
                string InterPath = Path.Combine(GameDir, "libraries", FabricInterInfo.Item2);
                OtherTools.CreateDir(LoaderPath);
                OtherTools.CreateDir(InterPath);
                DownloadStack.Push(new DownloadTaskInfo
                {
                    DestPath = LoaderPath,
                    DownloadUrl = LoaderUrl,
                    MaxTryCount = 4
                });
                DownloadStack.Push(new DownloadTaskInfo
                {
                    DestPath = InterPath,
                    DownloadUrl = InterUrl,
                    MaxTryCount = 4
                });
                AllFileCount = DownloadStack.Count;
                MultiFileDownloader downloader = new MultiFileDownloader(DownloadStack, 64);
                downloader.StartDownload();
                downloader.WaitDownloadComplete();
                File.WriteAllText(Path.Combine(new string[] { GameDir, "versions", VersionName, VersionName + ".json" }), JsonConvert.SerializeObject(VersionJson));
                OtherTools.WaitAllTaskExit(DownloadList);
                return new InstallerReponse { isSuccess = true };
            }catch(Exception e)
            {
                return new InstallerReponse { Exception = e, isSuccess = false };
            }
        }
        private static string GetFabricLibraryPathFromName(string LibraryName)
        {
            string LibraryPath = "";
            string[] SplitName = LibraryName.Split(':');
            foreach (string i in SplitName) LibraryPath += i.Replace('.', '/') + "/";
            LibraryPath += SplitName.Last() + "/";
            return LibraryPath;
        }
        private string GetFabricLibraryUrlFromName(string LibraryName)
        {
            string LibraryUrl = "https://meta.fabricmc.net/";
            string[] SplitName = LibraryName.Split(':');
            for (int i = 0; i < SplitName.Count() - 1; i++)
            {
                LibraryUrl += SplitName[i].Replace('.', '/') + "/";
            }
            LibraryUrl += SplitName[SplitName.Count() - 1] + "/" + SplitName[SplitName.Count() - 2] + "-" + SplitName.Last() + ".jar";
            return LibraryUrl;
        }
        private Tuple<string, string> GetFabricLoaderPathAndUrlFromName(string LoaderName)
        {
            string LoaderUrl = "https://maven.fabricmc.net/";
            string LoaderPath = "";
            string[] SplitName = LoaderName.Split(':');
            for (int i = 0; i < SplitName.Count() - 1; i++)
            {
                LoaderUrl += SplitName[i].Replace('.', '/') + "/";
                LoaderPath += SplitName[i].Replace('.', '/') + "/";
            }
            LoaderUrl += SplitName[SplitName.Count() - 1] + "/";
            LoaderPath += SplitName[SplitName.Count() - 1] + "/";
            LoaderUrl += SplitName[SplitName.Count() - 2] + "-" + SplitName.Last() + ".jar";
            return new Tuple<string, string>(LoaderUrl, LoaderPath);
        }

        private Tuple<string, string> GetFabricInterPathAndUrlFromName(string InterName)
        {
            string LoaderUrl = "https://maven.fabricmc.net/";
            string LoaderPath = "";
            string[] SplitName = InterName.Split(':');
            for (int i = 0; i < SplitName.Count() - 1; i++)
            {
                LoaderUrl += SplitName[i].Replace('.', '/') + "/";
                LoaderPath += SplitName[i].Replace('.', '/') + "/";
            }
            LoaderUrl += SplitName[SplitName.Count() - 1] + "/";
            LoaderPath += SplitName[SplitName.Count() - 1] + "/";
            LoaderUrl += SplitName[SplitName.Count() - 2] + "-" + SplitName.Last() + ".jar";
            return new Tuple<string, string>(LoaderUrl, LoaderPath);
        }
    }
}
