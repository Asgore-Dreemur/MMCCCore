using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.GameAssemblies;
using System.Net;
using Newtonsoft.Json;
using MMCCCore.Core.Wrapper;
using System.IO;
using System.Diagnostics;
using MMCCCore.Core.Module.Minecraft;
using MMCCCore.Core.Model.Wrapper;
using MMCCCore.Core.Module.Authenticator;

namespace MMCCCore.Core.Module.GameAssemblies
{
    public class Optifine : InstallerModel
    {
        private static WebClient WebClient = new WebClient();
        public static List<OptifineVersionModel> GetOptifineVersionsFromVersion(string MCVersion)
        {
            string VersionStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/optifine/{MCVersion}");
            List<OptifineVersionModel> OptifineList = JsonConvert.DeserializeObject<List<OptifineVersionModel>>(VersionStr);
            OptifineList.Reverse();
            return OptifineList;
        }
        public static List<OptifineVersionModel> GetAllOptifineVersions()
        {
            string VersionStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/optifine/versionList");
            List<OptifineVersionModel> OptifineList = JsonConvert.DeserializeObject<List<OptifineVersionModel>>(VersionStr);
            OptifineList.Reverse();
            return OptifineList;
        }
        public static string GetOptifineDownloadUrl(OptifineVersionModel model) => $"https://bmclapi2.bangbang93.com/optifine/{model.MCVersion}/{model.Type}/{model.Patch}";
        public InstallerResponse InstallOptifine(string GameDir, string VersionName, OptifineVersionModel InstallInfo, string JavaPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(VersionName) || CoreWrapper.IsExistsVersion(GameDir, VersionName)) throw new Exception("版本名不可重名或留空");
                Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "MMCC"));
                string OptifinePath = Path.Combine(Path.GetTempPath(), "MMCC", "optifine.jar");
                string OptifineInstallPath = Path.Combine(Path.GetTempPath(), "optifine-installer.jar");
                File.WriteAllBytes(OptifineInstallPath, InstallerResources.optifine_installer);
                OnProgressChanged(0, "下载Optifine");
                FileDownloader downloader = new FileDownloader(new DownloadTaskInfo
                {
                    DownloadUrl = $"https://download.mcbbs.net/optifine/{InstallInfo.MCVersion}/{InstallInfo.Type}/{InstallInfo.Patch}",
                    DestPath = OptifinePath,
                    MaxTryCount = 4
                });
                downloader.DownloadProgressChanged += (_, e) => OnProgressChanged(e, "下载Optifine");
                var FileDownloadResult = downloader.StartDownload();
                if (FileDownloadResult.Result != DownloadResult.Success)
                    throw new Exception(message: "Optifine安装包下载失败", innerException:FileDownloadResult.ErrorException);
                OnProgressChanged(-1, "安装Optifine...");
                Process InstallProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        FileName = JavaPath,
                        Arguments = $"-cp \"{OptifinePath}{OtherTools.JavaCPSeparatorChar}{OptifineInstallPath}\" net.stevexmh.OptifineInstaller \"{GameDir}\" \"{VersionName}\""
                    }
                };
                InstallProcess.Start();
                InstallProcess.WaitForExit();
                if (InstallProcess.ExitCode != 0) throw new Exception(message: "Optifine安装失败");
                try { Directory.Delete(Path.Combine(Path.GetTempPath(), "MMCC"), true); } catch (Exception) { }
                return new InstallerResponse { Exception = null, isSuccess = true };
            }catch(Exception e)
            {
                return new InstallerResponse { Exception = e, isSuccess = false };
            }
        }
    }
}
