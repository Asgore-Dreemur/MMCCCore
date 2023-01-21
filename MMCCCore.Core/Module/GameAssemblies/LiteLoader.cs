using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Wrapper;
using MMCCCore.Core.Model.GameAssemblies;
using System.Net;
using Newtonsoft.Json;
using MMCCCore.Core.Model.Core;
using Newtonsoft.Json.Linq;
using System.IO;
using MMCCCore.Core.Model.Wrapper;
using MMCCCore.Core.Module.Minecraft;

namespace MMCCCore.Core.Module.GameAssemblies
{
    public class LiteLoader
    {
        public string GameDir { get; set; }
        public string VersionName { get; set; }
        public string VanillaVersionName { get; set; }


        private static WebClient WebClient = new WebClient();
        public static LiteLoaderVersionModel GetLiteLoaderVersionFromVersion(string MCVersion)
        {
            string VersionStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/liteloader/list?mcversion={MCVersion}");
            return JsonConvert.DeserializeObject<LiteLoaderVersionModel>(VersionStr);
        }
        public static LiteLoaderVersionModel GetAllLiteLoaderVersions()
        {
            string VersionStr = WebClient.DownloadString("https://bmclapi2.bangbang93.com/liteloader/list");
            return JsonConvert.DeserializeObject<LiteLoaderVersionModel>(VersionStr);
        }

        public InstallerResponse InstallLiteLoader(LiteLoaderVersionModel InstallInfo) => InstallLiteLoaderTaskAsync(InstallInfo).GetAwaiter().GetResult();

        public async Task<InstallerResponse> InstallLiteLoaderTaskAsync(LiteLoaderVersionModel InstallInfo)
        {
            try
            {
                GameDir = OtherTools.FormatPath(GameDir);
                if (string.IsNullOrWhiteSpace(VersionName) || CoreWrapper.IsExistsVersion(GameDir, VersionName)) throw new Exception("版本名不可重名或留空");
                string VanillaJson = File.ReadAllText(Path.Combine(GameDir, "versions", VanillaVersionName, VanillaVersionName + ".json"));
                LocalMCVersionJsonModel VersionInfo = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(VanillaJson);
                if (!CoreWrapper.IsExistsVersion(GameDir, VersionInfo.Id)) throw new Exception("找不到原版核心");
                string VersionPath = Path.Combine(new string[] { GameDir, "versions", VersionName });
                OtherTools.CreateDir(VersionPath);
                string VanillaGameJar = Path.Combine(GameDir, "versions", VersionInfo.Id, VersionInfo.Id + ".jar");
                string LiteLoaderGameJar = Path.Combine(GameDir, "versions", VersionName, VersionName + ".jar");
                File.Copy(VanillaGameJar, LiteLoaderGameJar, true);
                foreach (LiteLoaderLibraryModel LibraryInfo in InstallInfo.Build.Libraries)
                {
                    VersionInfo.Libraries.Add(new MCLibraryFileModel() { Name = LibraryInfo.Name });
                }
                VersionInfo.Id = VersionName;
                VersionInfo.MainClass = "net.minecraft.launchwrapper.Launch";
                VersionInfo.Libraries.Add(new MCLibraryFileModel() { Name = $"com.mumfrey:liteloader:{InstallInfo.Version.ToString()}", Url = "http://dl.liteloader.com/versions/" });
                VersionInfo.Arguments = new GameArgumentsModel();
                VersionInfo.Arguments.Game.AddRange(new JToken[] { "--tweakClass", InstallInfo.Build.TweakClass });
                File.WriteAllText(Path.Combine(VersionPath, VersionName + ".json"), JsonConvert.SerializeObject(VersionInfo));
                FileDownloader downloader = new FileDownloader(new DownloadTaskInfo
                {
                    DestPath = Path.Combine(GameDir, "libraries", "com", "mumfrey", "liteloader", InstallInfo.Version.ToString(), InstallInfo.Build.FileName),
                    DownloadUrl = $"https://download.mcbbs.net/liteloader/download?version={InstallInfo.Version}",
                    MaxTryCount = 4
                });
                var FileDownloadResult = downloader.StartDownload();
                if (FileDownloadResult.Result == DownloadResult.Error)
                {
                    throw new Exception(message: "LiteLoader文件下载失败", innerException: FileDownloadResult.ErrorException);
                }
                return new InstallerResponse { isSuccess = true };
            }catch(Exception e)
            {
                return new InstallerResponse { isSuccess = false, Exception = e };
            }
        }
    }
}
