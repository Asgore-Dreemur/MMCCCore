using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model;
using System.Threading;
using System.IO;
using System.Net;
using MMCCCore.Model.Version;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MMCCCore.Wrapper;

namespace MMCCCore.Module
{
    class MinecraftInstaller
    {
        private MinecraftInstalllerModel InstallerInfo;
        private Thread InstallThread;
        private WebClient WebClient = new WebClient();
        private HttpWrapper HttpWrapper = new HttpWrapper();
        public MinecraftInstaller(MinecraftInstalllerModel InstallerInfo) 
        {
            this.InstallerInfo = InstallerInfo;
            InstallThread = new Thread(InstallGameMain);
        }
        public void Start()
        {
            InstallThread.Start();
        }
        public void InstallGameMain()
        {
            Directory.CreateDirectory(InstallerInfo.MCGameDir);
            string LibraryPath = Path.Combine(InstallerInfo.MCGameDir, "libraries");
            string AssetsPath = Path.Combine(InstallerInfo.MCGameDir, "assets");
            string VersionsPath = Path.Combine(InstallerInfo.MCGameDir, "versions");
            string AssetsObjectPath = Path.Combine(AssetsPath, "objects");
            string AssetsIndexPath = Path.Combine(AssetsPath, "indexes");
            string VersionPath = Path.Combine(VersionsPath, InstallerInfo.VersionName);
            string VersionJsonPath = Path.Combine(VersionPath, InstallerInfo.VersionName + ".json");
            string VersionJarPath = Path.Combine(VersionPath, InstallerInfo.VersionName + ".jar");
            Directory.CreateDirectory(LibraryPath);
            Directory.CreateDirectory(AssetsPath);
            Directory.CreateDirectory(VersionsPath);
            Directory.CreateDirectory(AssetsObjectPath);
            Directory.CreateDirectory(AssetsIndexPath);
            Directory.CreateDirectory(VersionPath);
            List<Task> TaskList = new List<Task>();
            string GameJsonStr = WebClient.DownloadString(InstallerInfo.InstallSource == GameSources.Bmclapi ? $"https://bmclapi2.bangbang93.com/version/{InstallerInfo.GameVersion}/json"
                : InstallerInfo.InstallSource == GameSources.Mcbbs ? $"https://download.mcbbs.net/version/{InstallerInfo.GameVersion}/json"
                : InstallerInfo.VanillaJsonUrl);
            LocalMCVersionJsonModel VersionInfo = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(GameJsonStr);
            VersionInfo.Id = InstallerInfo.VersionName;
            File.WriteAllText(VersionJsonPath, JsonConvert.SerializeObject(VersionInfo));
            string JarDownloadUrl = InstallerInfo.InstallSource == GameSources.Bmclapi ? $"https://bmclapi2.bangbang93.com/version/{InstallerInfo.GameVersion}/client"
                : InstallerInfo.InstallSource == GameSources.Mcbbs ? $"https://download.mcbbs.net/version/{InstallerInfo.GameVersion}/client"
                : VersionInfo.Downloads.Client.Url;
            TaskList.Add(HttpWrapper.HttpDownloadFileAsync(JarDownloadUrl, VersionJarPath, 4)) ;
            foreach(GameLibraryModel LibraryInfo in VersionInfo.Libraries)
            {
                if(LibraryInfo.Download.Artifact != null)
                {
                    CreateDir(LibraryInfo.Download.Artifact.Path.Substring(0, LibraryInfo.Download.Artifact.Path.LastIndexOf('/')));
                    TaskList.Add(HttpWrapper.HttpDownloadFileAsync(LibraryInfo.Download.Artifact.Url, Path.Combine(LibraryPath, LibraryInfo.Download.Artifact.Path), 4));
                }
                else
                {
                    if(LibraryInfo.Download.Classifiers.NativesWindows != null)
                    {
                        CreateDir(LibraryInfo.Download.Classifiers.NativesWindows.Path.Substring(0, LibraryInfo.Download.Classifiers.NativesWindows.Path.LastIndexOf('/')));
                        TaskList.Add(HttpWrapper.HttpDownloadFileAsync(LibraryInfo.Download.Classifiers.NativesWindows.Url, Path.Combine(LibraryPath, LibraryInfo.Download.Classifiers.NativesWindows.Path), 4));
                    }
                    else
                    {
                        if (Environment.Is64BitOperatingSystem)
                        {
                            if (LibraryInfo.Download.Classifiers.NativesWindows64 == null) continue;
                            else
                            {
                                CreateDir(LibraryInfo.Download.Classifiers.NativesWindows64.Path.Substring(0, LibraryInfo.Download.Classifiers.NativesWindows64.Path.LastIndexOf('/')));
                                TaskList.Add(HttpWrapper.HttpDownloadFileAsync(LibraryInfo.Download.Classifiers.NativesWindows64.Url, Path.Combine(LibraryPath, LibraryInfo.Download.Classifiers.NativesWindows64.Path), 4));
                            }
                        }
                        else
                        {
                            if (LibraryInfo.Download.Classifiers.NativesWindows32 == null) continue;
                            else
                            {
                                CreateDir(LibraryInfo.Download.Classifiers.NativesWindows32.Path.Substring(0, LibraryInfo.Download.Classifiers.NativesWindows32.Path.LastIndexOf('/')));
                                TaskList.Add(HttpWrapper.HttpDownloadFileAsync(LibraryInfo.Download.Classifiers.NativesWindows32.Url, Path.Combine(LibraryPath, LibraryInfo.Download.Classifiers.NativesWindows32.Path), 4));
                            }
                        }
                    }
                }
            }

        }
        private void CreateDir(string path)
        {
            string[] pathtree = path.Split('\\');
            if (pathtree.Count() != 1)
            {
                string pathd = pathtree[0] + "\\";
                for (int i = 1; i < pathtree.Count(); i++)
                {
                    Directory.CreateDirectory(Path.Combine(pathd, pathtree[i]));
                    pathd = Path.Combine(pathd, pathtree[i]);
                }
            }
        }
    }
}
