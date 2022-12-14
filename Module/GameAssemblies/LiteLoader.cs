using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Wrapper;
using MMCCCore.Model.GameAssemblies;
using System.Net;
using Newtonsoft.Json;
using MMCCCore.Model.Core;
using Newtonsoft.Json.Linq;
using System.IO;
using MMCCCore.Model.Wrapper;
using MMCCCore.Module.Minecraft;

namespace MMCCCore.Module.GameAssemblies
{
    public class LiteLoader
    {
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
        public InstallerReponse InstallLiteLoader(string GameDir, LiteLoaderVersionModel InstallInfo, string VersionName, string VanillaJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(VersionName) || CoreWrapper.IsExistsVersion(GameDir, VersionName)) throw new Exception("版本名不可重名或留空");
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
                    DestPath = Path.Combine(GameDir, "libraries", $"com\\mumfrey\\liteloader\\{InstallInfo.Version}\\{InstallInfo.Build.FileName}"),
                    DownloadUrl = $"https://download.mcbbs.net/liteloader/download?version={InstallInfo.Version}",
                    MaxTryCount = 4
                });
                var FileDownloadResult = downloader.StartDownload();
                if (FileDownloadResult.Result == DownloadResult.Error)
                {
                    throw new Exception(message: "LiteLoader文件下载失败", innerException: FileDownloadResult.ErrorException);
                }
                return new InstallerReponse { isSuccess = true };
            }catch(Exception e)
            {
                return new InstallerReponse { isSuccess = false, Exception = e };
            }
        }
    }
}
