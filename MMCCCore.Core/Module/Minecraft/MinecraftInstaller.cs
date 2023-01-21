using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.Core;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using MMCCCore.Core.Wrapper;
using MMCCCore.Core.Model.Wrapper;
using MMCCCore.Core.Module.Minecraft;
using MMCCCore.Core.Model.MinecraftFiles;
using MMCCCore.Core.Module.APIManager;

namespace MMCCCore.Core.Module.Minecraft
{
    public class MinecraftInstaller : InstallerModel
    {
        private string GameDir;
        private MCVersionModel InstallInfo;
        private string VersionName;
        private int MaxThreadCount;
        public MinecraftInstaller(string GameDir, MCVersionModel InstallInfo, string VersionName, int MaxThreadCount = 64)
        {
            this.GameDir = GameDir;
            this.InstallInfo = InstallInfo;
            this.VersionName = VersionName;
            this.MaxThreadCount = MaxThreadCount;
        }

        public InstallerResponse InstallMinecraft(bool isSkipDownloadedFile = true) => InstallMinecraftTaskAsync(isSkipDownloadedFile).GetAwaiter().GetResult();
        public async Task<InstallerResponse> InstallMinecraftTaskAsync(bool isSkipDownloadedFile = true)
        {
            WebClient WebClient = new WebClient();
            if (CoreWrapper.IsExistsVersion(GameDir, VersionName) || string.IsNullOrWhiteSpace(VersionName)) 
                return new InstallerResponse { Exception = new Exception("不能和现有版本重名或留空"), isSuccess = false };
            string LibrariesPath = Path.Combine(GameDir, "libraries");
            string AssetsObjectPath = Path.Combine(GameDir, "assets\\objects");
            string AssetIndexPath = Path.Combine(GameDir, "assets\\indexes");
            string VersionPath = Path.Combine(GameDir, "versions", VersionName);
            string NativesPath = Path.Combine(GameDir, "versions", VersionName, "natives");
            try
            {
                if (DownloadAPIManager.Current == null) throw new Exception("未知的下载源");
                OtherTools.CreateDir(LibrariesPath);
                OtherTools.CreateDir(AssetsObjectPath);
                OtherTools.CreateDir(AssetIndexPath);
                OtherTools.CreateDir(VersionPath);
                OtherTools.CreateDir(NativesPath);
                OnProgressChanged(0.10, "下载版本Json");
                string VersionJsonStr = WebClient.DownloadString(
                    DownloadAPIManager.Current.CoreJson != null ? DownloadAPIManager.Current.CoreJson.Replace("<version>", InstallInfo.Id)
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
                    DownloadUrl = DownloadAPIManager.Current.CoreJar != null ? DownloadAPIManager.Current.CoreJar.Replace("<version>", VersionInfo.Id)
                    : VersionInfo.Downloads.Client.Url,
                    MaxTryCount = 4
                });
                downloader.DownloadProgressChanged += (_e, status) => OnProgressChanged(status, "下载本体文件");
                var FileDownloadResult = downloader.StartDownload();
                if (FileDownloadResult.Result == DownloadResult.Error) throw new Exception(message: "本体文件下载失败", innerException: new Exception(FileDownloadResult.ErrorException.Message));
                MCLibrary library = new MCLibrary();
                OnProgressChanged(0.00, "下载支持库");
                library.ProgressChanged += (_e, status) => OnProgressChanged(status.Item1, "下载支持库");
                var result = library.DownloadLibraries(VersionInfo, GameDir, isSkipDownloadedFile, MaxThreadCount);
                if(result.DownloadResult == MinecraftFilesDownloadResult.Error)
                {
                    throw result.ErrorException;
                }
                OnProgressChanged(0.00, "下载AssetsIndex");
                string AssetsIndexStr = "";
                string VersionAssetIndexPath = Path.Combine(AssetIndexPath, VersionInfo.AssetIndex.Id + ".json");
                var vresult = OtherTools.VaildateSha1(VersionAssetIndexPath, VersionInfo.AssetIndex.Sha1);
                if (vresult.isVaildated && isSkipDownloadedFile)
                {
                    AssetsIndexStr = new StreamReader(new FileStream(VersionAssetIndexPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)).ReadToEnd();
                }
                else
                {
                    AssetsIndexStr = WebClient.DownloadString(
                        DownloadAPIManager.Current.AssetIndex != null ? VersionInfo.AssetIndex.Url.Replace("launchermeta.mojang.com", DownloadAPIManager.Current.AssetIndex)
                        : VersionInfo.AssetIndex.Url);
                    File.WriteAllText(VersionAssetIndexPath, AssetsIndexStr);
                }
                OnProgressChanged(0.00, "下载资源文件");
                MCAssets assets = new MCAssets();
                assets.ProgressChanged += (_e, status)=> OnProgressChanged(status.Item1, "下载资源文件");
                result = assets.DownloadAssets(AssetsIndexStr, GameDir, isSkipDownloadedFile, MaxThreadCount);
                if(result.DownloadResult == MinecraftFilesDownloadResult.Error)
                {
                    throw result.ErrorException;
                }
                return new InstallerResponse { isSuccess = true, Exception = null };
            }
            catch(Exception e)
            {
                try { Directory.Delete(VersionPath, true); } catch (Exception) { }
                return new InstallerResponse { isSuccess = false, Exception = e };
            }
        }
    }
}
