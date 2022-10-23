using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.ModAPI;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MMCCCore.Model.Version;
using System.IO;
using MMCCCore.Wrapper;
using System.Threading;
using System.Diagnostics;

namespace MMCCCore.Module.ModAPI
{
    public class Optifine
    {
        HttpWrapper HttpWrapper = new HttpWrapper();
        WebClient WebClient = new WebClient();
        public List<OptifineModel> GetOptifineVersionsFromVersion(string MCVersion)
        {
            string ResStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/optifine/{MCVersion}");
            return JsonConvert.DeserializeObject<List<OptifineModel>>(ResStr);
        }
        public List<OptifineModel> GetAllOptifineVersion()
        {
            string ResStr = WebClient.DownloadString("https://bmclapi2.bangbang93.com/optifine/versionList");
            return JsonConvert.DeserializeObject<List<OptifineModel>>(ResStr);
        }
        public bool InstallOptifineAsLib(OptifineModel OptifineInfo,  string GameDir, string VersionName)
        {
            string OptiFinePath = Path.Combine(Path.GetTempPath(), OptifineInfo.FileName);
            if (!HttpWrapper.HttpDownloadFile($"https://download.mcbbs.net/optifine/{OptifineInfo.MCVersion}/{OptifineInfo.Type}/{OptifineInfo.Patch}", OptiFinePath, 4)) return false;
            string OptifineInstallerPath = Path.Combine(Path.GetTempPath(), "optifine-installer.jar");
            File.WriteAllBytes(OptifineInstallerPath, InstallerResources.optifine_installer);
            Process InstallProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = "java",
                    Arguments = $"-cp \"{OptiFinePath};{OptifineInstallerPath}\" net.stevexmh.OptifineInstaller \"{GameDir}\" \"{VersionName}\""
                }
            };
            InstallProcess.Start();
            InstallProcess.WaitForExit();
            if (InstallProcess.ExitCode != 0) return false;
            return true;
        }
    }
}
