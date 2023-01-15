using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.GameAssemblies;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using MMCCCore.Core.Model.Core;
using MMCCCore.Core.Wrapper;
using Newtonsoft.Json.Linq;
using MMCCCore.Core.Model.Wrapper;
using MMCCCore.Core.Module.Minecraft;
using System.Threading;

namespace MMCCCore.Core.Module.GameAssemblies
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
        public InstallerResponse InstallFabric(string GameDir, string VersionName, FabricVersionModel InstallInfo, string VanillaVersionName, int MaxThreadCount = 64)
        {
            try
            {
                string VanillaJsonPath = Path.Combine(GameDir, "versions", VanillaVersionName, VanillaVersionName + ".json");
                if (string.IsNullOrWhiteSpace(VersionName) || CoreWrapper.IsExistsVersion(GameDir, VersionName)) throw new Exception("版本名不可重名或留空");
                OnProgressChanged(0.0, "准备信息...");
                LocalMCVersionJsonModel VanilaJsonModel = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(File.ReadAllText(VanillaJsonPath));
                LocalMCVersionJsonModel VersionJson = VanilaJsonModel;
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
                OnProgressChanged(0.0, "下载支持库...");
                Stack<DownloadTaskInfo> DownloadStack = new Stack<DownloadTaskInfo>();
                if (InstallInfo.LauncherMeta.Libraries.Client.Count > 0)
                {
                    foreach (FabricLibraryModel LibraryInfo in InstallInfo.LauncherMeta.Libraries.Client)
                    {
                        string LibraryNoAbsName = MCLibrary.GetMavenFilePathFromName(LibraryInfo.Name);
                        string LibraryUrl = $"{LibraryInfo.Url.TrimEnd('/')}/{LibraryNoAbsName}";
                        string LibraryPath = Path.Combine(GameDir, "libraries", LibraryNoAbsName).Replace('/', '\\');
                        OtherTools.CreateDir(LibraryPath.Substring(0, LibraryPath.LastIndexOf('\\')));
                        DownloadStack.Push(new DownloadTaskInfo
                        {
                            DestPath = LibraryPath,
                            DownloadUrl = LibraryUrl,
                            MaxTryCount = 4,
                            Sha1Vaildate = false,
                            isSkipDownloadedFile = true
                        });
                        VersionJson.Libraries.Add(new MCLibraryFileModel() { Name = LibraryInfo.Name, Url = LibraryInfo.Url });
                    }
                }
                if (InstallInfo.LauncherMeta.Libraries.Common.Count > 0)
                {
                    foreach (FabricLibraryModel LibraryInfo in InstallInfo.LauncherMeta.Libraries.Common)
                    {
                        string LibraryNoAbsName = MCLibrary.GetMavenFilePathFromName(LibraryInfo.Name);
                        string LibraryUrl = $"{LibraryInfo.Url.TrimEnd('/')}/{LibraryNoAbsName}";
                        string LibraryPath = Path.Combine(GameDir, "libraries", LibraryNoAbsName).Replace('/', '\\');
                        OtherTools.CreateDir(LibraryPath.Substring(0, LibraryPath.LastIndexOf('\\')));
                        DownloadStack.Push(new DownloadTaskInfo
                        {
                            DestPath = LibraryPath,
                            DownloadUrl = LibraryUrl,
                            MaxTryCount = 4,
                            Sha1Vaildate = false,
                            isSkipDownloadedFile = true
                        });
                        VersionJson.Libraries.Add(new MCLibraryFileModel() { Name = LibraryInfo.Name, Url = LibraryInfo.Url });
                    }
                }
                string LoaderNoAbsPath = MCLibrary.GetMavenFilePathFromName(InstallInfo.Loader.Maven);
                string InterNoAbsPath = MCLibrary.GetMavenFilePathFromName(InstallInfo.Intermediary.Maven);
                string LoaderUrl = $"https://maven.fabricmc.net/{LoaderNoAbsPath}";
                string LoaderPath = Path.Combine(GameDir, "libraries", LoaderNoAbsPath).Replace('/', '\\');
                string InterUrl = $"https://maven.fabricmc.net/{InterNoAbsPath}";
                string InterPath = Path.Combine(GameDir, "libraries", InterNoAbsPath).Replace('/', '\\');
                OtherTools.CreateDir(LoaderPath.Substring(0, LoaderPath.LastIndexOf('\\')));
                OtherTools.CreateDir(InterPath.Substring(0, LoaderPath.LastIndexOf('\\')));
                DownloadStack.Push(new DownloadTaskInfo
                {
                    DestPath = LoaderPath,
                    DownloadUrl = LoaderUrl,
                    MaxTryCount = 4,
                    Sha1Vaildate = false,
                    isSkipDownloadedFile = true
                });
                DownloadStack.Push(new DownloadTaskInfo
                {
                    DestPath = InterPath,
                    DownloadUrl = InterUrl,
                    MaxTryCount = 4,
                    Sha1Vaildate = false,
                    isSkipDownloadedFile = true
                });
                VersionJson.Libraries.Add(new MCLibraryFileModel() { Name = InstallInfo.Loader.Maven, Url = "https://maven.fabricmc.net/" });
                VersionJson.Libraries.Add(new MCLibraryFileModel() { Name = InstallInfo.Intermediary.Maven, Url = "https://maven.fabricmc.net/" });
                AllFileCount = DownloadStack.Count;
                MultiFileDownloader downloader = new MultiFileDownloader(DownloadStack, 64);
                downloader.ProgressChanged += Downloader_ProgressChanged;
                downloader.StartDownload();
                downloader.WaitDownloadComplete();
                File.WriteAllText(Path.Combine(new string[] { GameDir, "versions", VersionName, VersionName + ".json" }), JsonConvert.SerializeObject(VersionJson));
                OtherTools.WaitAllTaskExit(DownloadList);
                return new InstallerResponse { isSuccess = true };
            }catch(Exception e)
            {
                return new InstallerResponse { Exception = e, isSuccess = false };
            }
        }

        private void Downloader_ProgressChanged(object sender, (int, int, DownloadResultModel) e)
        {
            if (e.Item3.Result == DownloadResult.Error)
            {
                ((MultiFileDownloader)sender).StopDownload();
                throw new Exception($"下载{e.Item3.DownloadInfo.DownloadUrl}时出现错误:{e.Item3.ErrorException.Message}");
            }
            double DownloadedProgress = (double)Math.Round((decimal)e.Item1 / e.Item2, 2);
            OnProgressChanged(DownloadedProgress, "下载支持库");
        }
    }
}
