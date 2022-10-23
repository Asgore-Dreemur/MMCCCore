using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.Core;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using MMCCCore.Wrapper;
using MMCCCore.Model.Wrapper;
using MMCCCore.Module.Minecraft;
using MMCCCore.Model.MinecraftFiles;

namespace MMCCCore.Module.Minecraft
{
    public class MinecraftInstaller : InstallerModel
    {
        private string GameDir;
        private MCVersionModel InstallInfo;
        private GameSources DownloadSource;
        private string VersionName;
        private int MaxThreadCount;
        public MinecraftInstaller(string GameDir, MCVersionModel InstallInfo, GameSources DownloadSource, string VersionName, int MaxThreadCount = 64)
        {
            this.GameDir = GameDir;
            this.InstallInfo = InstallInfo;
            this.DownloadSource = DownloadSource;
            this.VersionName = VersionName;
            this.MaxThreadCount = MaxThreadCount;
        }
        public InstallerReponse InstallMinecraft(bool SkipDownloadedFile = true)
        {
            WebClient WebClient = new WebClient();
            if (CoreWrapper.IsExistsVersion(GameDir, VersionName) || string.IsNullOrWhiteSpace(VersionName)) return new InstallerReponse { Exception = new Exception("不能和现有版本重名或留空"), isSuccess = false };
            string LibrariesPath = Path.Combine(GameDir, "libraries");
            string AssetsObjectPath = Path.Combine(GameDir, "assets\\objects");
            string AssetIndexPath = Path.Combine(GameDir, "assets\\indexes");
            string VersionPath = Path.Combine(GameDir, "versions", VersionName);
            string NativesPath = Path.Combine(GameDir, "versions", VersionName, "natives");
            try
            {
                OtherTools.CreateDir(LibrariesPath);
                OtherTools.CreateDir(AssetsObjectPath);
                OtherTools.CreateDir(AssetIndexPath);
                OtherTools.CreateDir(VersionPath);
                OtherTools.CreateDir(NativesPath);
                OnProgressChanged(0.10, "下载版本Json");
                string VersionJsonStr = WebClient.DownloadString(DownloadSource == GameSources.Bmclapi ? $"https://bmclapi2.bangbang93.com/version/{InstallInfo.Id}/json"
                    : DownloadSource == GameSources.Mcbbs ? $"https://download.mcbbs.net/version/{InstallInfo.Id}/json"
                    : InstallInfo.JsonUrl);
                string JsonPath = Path.Combine(VersionPath, VersionName + ".json");
                string JarPath = Path.Combine(VersionPath, VersionName + ".jar");
                LocalMCVersionJsonModel VersionInfo = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(VersionJsonStr);
                VersionInfo.Id = VersionName;
                File.WriteAllText(JsonPath, JsonConvert.SerializeObject(VersionInfo));
                OnProgressChanged(0.00, "下载本体文件");
                FileDownloader downloader = new FileDownloader(new DownloadTaskInfo
                {
                    DestPath = JarPath,
                    DownloadUrl = DownloadSource == GameSources.Bmclapi ? $"https://bmclapi2.bangbang93.com/version/{InstallInfo.Id}/client"
                    : DownloadSource == GameSources.Mcbbs ? $"https://download.mcbbs.net/version/{InstallInfo.Id}/client"
                    : VersionInfo.Downloads.Client.Url,
                    MaxTryCount = 4
                });
                downloader.DownloadProgressChanged += (_e, status) => OnProgressChanged(status, "下载本体文件");
                var FileDownloadResult = downloader.StartDownload();
                if (FileDownloadResult.Result == DownloadResult.Error) throw new Exception(message: "本体文件下载失败", innerException: new Exception(FileDownloadResult.ErrorException.Message));
                MCLibrary library = new MCLibrary();
                OnProgressChanged(0.00, "下载支持库");
                library.ProgressChanged += (_e, status) => OnProgressChanged(status.Item1, "下载支持库");
                var result = library.DownloadLibraries(VersionInfo, GameDir, DownloadSource, SkipDownloadedFile, MaxThreadCount);
                if(result.DownloadResult == MinecraftFilesDownloadResult.Error)
                {
                    throw result.ErrorException;
                }
                OnProgressChanged(0.00, "下载AssetsIndex");
                string AssetsIndexStr = "";
                string VersionAssetIndexPath = Path.Combine(AssetIndexPath, VersionInfo.AssetIndex.Id + ".json");
                var vresult = OtherTools.VaildateSha1(VersionAssetIndexPath, VersionInfo.AssetIndex.Sha1);
                if (!vresult.isSuccess) throw vresult.ErrorException;
                if (vresult.isVaildated && SkipDownloadedFile)
                {
                    AssetsIndexStr = new StreamReader(new FileStream(VersionAssetIndexPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)).ReadToEnd();
                }
                else
                {
                    AssetsIndexStr = WebClient.DownloadString(
                        DownloadSource == GameSources.Bmclapi ? VersionInfo.AssetIndex.Url.Replace("launchermeta.mojang.com", "bmclapi2.bangbang93.com")
                        : DownloadSource == GameSources.Mcbbs ? VersionInfo.AssetIndex.Url.Replace("launchermeta.mojang.com", "download.mcbbs.net")
                        : VersionInfo.AssetIndex.Url);
                    File.WriteAllText(VersionAssetIndexPath, AssetsIndexStr);
                }
                OnProgressChanged(0.00, "下载资源文件");
                MCAssets assets = new MCAssets();
                assets.ProgressChanged += (_e, status)=> OnProgressChanged(status.Item1, "下载资源文件");
                result = assets.DownloadAssets(AssetsIndexStr, GameDir, DownloadSource, SkipDownloadedFile, MaxThreadCount);
                if(result.DownloadResult == MinecraftFilesDownloadResult.Error)
                {
                    throw result.ErrorException;
                }
                return new InstallerReponse { isSuccess = true, Exception = null };
            }
            catch(Exception e)
            {
                try { Directory.Delete(VersionPath, true); } catch (Exception) { }
                return new InstallerReponse { isSuccess = false, Exception = e };
            }
        }
    }
}
