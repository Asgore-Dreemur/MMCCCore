using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.IO;
using MMCCCore.Core.Module.Minecraft;
using MMCCCore.Core.Model.GameAssemblies;
using Newtonsoft.Json;
using MMCCCore.Core.Wrapper;
using MMCCCore.Core.Model.Wrapper;
using System.IO.Compression;
using MMCCCore.Core.Model.Core;
using System.Diagnostics;
using MMCCCore.Core.Module.APIManager;
using System.Text.RegularExpressions;

namespace MMCCCore.Core.Module.GameAssemblies
{
    public class Forge : InstallerModel
    {
        private static WebClient WebClient = new WebClient();
        public string GameDir { get; set; }
        public int MaxThreadCount { get; set; } = 64;
        public string VersionName { get; set; }
        public  string JavaPath { get; set; }


        public static List<ForgeVersionModel> GetForgeVersionsFromVersion(string MCVersion)
        {
            string VersionStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/forge/minecraft/{MCVersion}");
            List<ForgeVersionModel> ForgeVersionList = JsonConvert.DeserializeObject<List<ForgeVersionModel>>(VersionStr);
            ForgeVersionList.Sort((a, b) =>
            {
                if (a.Build > b.Build) return a.Build;
                else return b.Build;
            });

            return ForgeVersionList;
        }

        public static List<string> GetForgeSupportedVersion()
        {
            string VersionStr = WebClient.DownloadString($"https://bmclapi2.bangbang93.com/forge/minecraft");
            List<string> ForgeVersionList = JsonConvert.DeserializeObject<List<string>>(VersionStr);
            return ForgeVersionList;
        }

        public InstallerResponse InstallForge(ForgeVersionModel InstallInfo) => InstallForgeTaskAsync(InstallInfo).GetAwaiter().GetResult();

        public async Task<InstallerResponse> InstallForgeTaskAsync(ForgeVersionModel InstallInfo)
        {
            try
            {
                if (DownloadAPIManager.Current == null) throw new Exception("未知的下载源");
                GameDir = OtherTools.FormatPath(GameDir);
                if (string.IsNullOrWhiteSpace(VersionName) || CoreWrapper.IsExistsVersion(GameDir, VersionName)) throw new Exception("版本名不可重名或留空");
                string ForgePath = Path.Combine(Path.GetTempPath(), "forge.jar");
                OnProgressChanged(0, "下载forge...");
                FileDownloader downloader = new FileDownloader(new DownloadTaskInfo
                {
                    DestPath = ForgePath,
                    DownloadUrl = $"https://download.mcbbs.net/forge/download/{InstallInfo.Build.ToString()}",
                    MaxTryCount = 4
                });
                downloader.DownloadProgressChanged += (_, e) => OnProgressChanged(e, "下载forge...");
                var FileDownloaderResult = downloader.StartDownload();
                if (FileDownloaderResult.Result != DownloadResult.Success) throw new Exception(message: "Forge下载失败", innerException: FileDownloaderResult.ErrorException);
                ZipArchive archive = new ZipArchive(new FileStream(ForgePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                if (archive.GetEntry("version.json") == null)
                {
                    return InstallLowerForge(InstallInfo, GameDir, VersionName, ForgePath);
                }
                LocalMCVersionJsonModel ForgeVersionInfo = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(new StreamReader(archive.GetEntry("version.json").Open()).ReadToEnd());
                ForgeVersionInfo.Id = VersionName;
                ForgeVersionInfo.InheritsFrom = InstallInfo.MCVersion;
                string VersionPath = Path.Combine(GameDir, "versions", VersionName);
                OtherTools.CreateDir(VersionPath);
                VersionPath = Path.Combine(VersionPath, VersionName + ".json");
                File.WriteAllText(VersionPath, JsonConvert.SerializeObject(ForgeVersionInfo));
                OnProgressChanged(0, "下载支持库...");
                Stack<DownloadTaskInfo> DownloadStack = new Stack<DownloadTaskInfo>();
                var LibrariesList = MCLibrary.GetAllLibraries(ForgeVersionInfo);
                foreach (var LibraryInfo in LibrariesList)
                {
                    if (string.IsNullOrEmpty(LibraryInfo.Url)) continue;
                    string LibraryUrl = DownloadAPIManager.Current == DownloadAPIManager.Raw ? LibraryInfo.Url : LibraryInfo.Url.Replace("maven.minecraftforge.net", DownloadAPIManager.Current.Libraries.TrimStart("http://".ToCharArray()).TrimStart("https://".ToCharArray()));
                    string LibraryDir = Path.Combine(GameDir, "libraries", LibraryInfo.Path.Substring(0, LibraryInfo.Path.LastIndexOf('/')));
                    string LibraryPath = Path.Combine(GameDir, "libraries", LibraryInfo.Path);
                    OtherTools.CreateDir(LibraryDir);
                    DownloadStack.Push(new DownloadTaskInfo
                    {
                        DownloadUrl = LibraryUrl,
                        DestPath = LibraryPath,
                        MaxTryCount = 4,
                        isSkipDownloadedFile = true,
                        Sha1Vaildate = true,
                        Sha1 = LibraryInfo.CheckSum
                    });
                }
                ForgeInstallProfileModel ForgeInstallInfo = JsonConvert.DeserializeObject<ForgeInstallProfileModel>(new StreamReader(archive.GetEntry("install_profile.json").Open()).ReadToEnd());
                LocalMCVersionJsonModel InstallInfo2Json = new LocalMCVersionJsonModel { Libraries = ForgeInstallInfo.Libraries };
                LibrariesList = MCLibrary.GetAllLibraries(InstallInfo2Json);
                foreach (var LibraryInfo in LibrariesList)
                {
                    if (string.IsNullOrEmpty(LibraryInfo.Url)) continue;
                    string LibraryUrl = DownloadAPIManager.Current == DownloadAPIManager.Raw ? LibraryInfo.Url : LibraryInfo.Url.Replace("maven.minecraftforge.net", DownloadAPIManager.Current.Libraries.TrimStart("http://".ToCharArray()).TrimStart("https://".ToCharArray()).TrimEnd('/'));
                    string LibraryDir = Path.Combine(GameDir, "libraries", LibraryInfo.Path.Substring(0, LibraryInfo.Path.LastIndexOf('/')));
                    string LibraryPath = Path.Combine(GameDir, "libraries", LibraryInfo.Path);
                    OtherTools.CreateDir(LibraryDir);
                    DownloadStack.Push(new DownloadTaskInfo
                    {
                        DownloadUrl = LibraryUrl,
                        DestPath = LibraryPath,
                        MaxTryCount = 4
                    });
                }
                MultiFileDownloader mdownloader = new MultiFileDownloader(DownloadStack, 64);
                mdownloader.ProgressChanged += Mdownloader_ProgressChanged;
                mdownloader.StartDownload();
                var result = mdownloader.WaitDownloadComplete();
                if (result.Result == DownloadResult.Error) throw new Exception("下载支持库时出现一个或多个文件错误", result.ErrorException);
                OtherTools.CreateDir(Path.Combine(GameDir, "libraries", "data"));
                var LzmaPath = Path.Combine(GameDir, "libraries", "data", "client.lzma");
                if (File.Exists(LzmaPath)) File.Delete(LzmaPath);
                var entry = archive.GetEntry("data/client.lzma");
                entry.ExtractToFile(LzmaPath);
                int Steps = ForgeInstallInfo.Processors.Count;
                int CurrentStep = 1;
                double CurrentProgress = Math.Round((double)Math.Round((decimal)CurrentStep / Steps, 2));
                OnProgressChanged(CurrentProgress, "安装forge");
                foreach (var item in ForgeInstallInfo.Processors)
                {
                    if (item.Sides.Count > 0 && item.Sides[0] == "server") continue;
                    string InstallJarPath = LibrariesList.Find(i => i.Name == item.Jar).Path;
                    InstallJarPath = Path.Combine(GameDir, "libraries", InstallJarPath);
                    List<string> Classpath = LibrariesList.FindAll(i => item.ClassPath.Contains(i.Name))
                        .Select(i => OtherTools.FormatPath(Path.Combine(GameDir, "libraries", i.Path))).ToList();
                    Classpath.Add(InstallJarPath);
                    var jarinfo = LibrariesList.Find(i => i.Name == item.Jar);
                    var jarpath = OtherTools.FormatPath(Path.Combine(GameDir, "libraries", jarinfo.Path));
                    var jararchive = new ZipArchive(new FileStream(jarpath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                    string Manifest = new StreamReader(jararchive.GetEntry("META-INF/MANIFEST.MF").Open()).ReadToEnd();
                    var ManProp = Manifest.Split("\r\n".ToCharArray()).ToList();
                    ManProp = ManProp.FindAll(i => !string.IsNullOrEmpty(i)).ToList();
                    if (ManProp.Count == 0) throw new Exception("找不到支持库的MainClass");
                    string MainClass = ManProp.Find(i =>
                    {
                        var split = i.Split(':');
                        if (split[0].Trim() == "Main-Class") return true;
                        return false;
                    }).Split(':')[1];
                    List<string> Args = new List<string>();
                    foreach (var info in item.Args)
                    {
                        string arg = "";
                        if (info == "{MINECRAFT_JAR}")
                        {
                            arg = OtherTools.FormatPath(Path.Combine(GameDir, "versions", InstallInfo.MCVersion, InstallInfo.MCVersion + ".jar"));
                            arg = "\"" + arg + "\"";
                        }
                        else
                        {
                            if (info.StartsWith("[") && info.EndsWith("]"))
                            {
                                arg = GetForgeFilePath(info.TrimStart('[').TrimEnd(']'));
                                arg = arg.TrimStart("\\".ToCharArray()).TrimStart("/".ToCharArray());
                                arg = OtherTools.FormatPath(Path.Combine(GameDir, "libraries", arg));
                                OtherTools.CreateDir(arg.Substring(0, arg.LastIndexOf(Path.DirectorySeparatorChar)));
                                arg = "\"" + arg + "\"";
                            }
                            else if (info.StartsWith("{") && info.EndsWith("}"))
                            {
                                arg = ForgeInstallInfo.Data[info.TrimStart('{').TrimEnd('}')]["client"];
                                if(arg.StartsWith("[") && arg.EndsWith("]")) arg = GetForgeFilePath(arg.TrimStart('[').TrimEnd(']'));
                                arg = arg.TrimStart("\\".ToCharArray()).TrimStart("/".ToCharArray());
                                arg = OtherTools.FormatPath(Path.Combine(GameDir, "libraries", arg));
                                OtherTools.CreateDir(arg.Substring(0, arg.LastIndexOf(Path.DirectorySeparatorChar)));
                                arg = "\"" + arg + "\"";
                            }
                            else arg = info;
                        }
                        Args.Add(arg);
                    }
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = JavaPath,
                            Arguments = $"-cp \"{string.Join(OtherTools.JavaCPSeparatorChar.ToString(), Classpath)}\" {MainClass} {string.Join(" ", Args)}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        }
                    };
                    string output = "";
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.OutputDataReceived += (_, e) =>
                    {
                        output += e.Data;
                        OnProgressChanged(CurrentProgress, e.Data);
                    };
                    process.ErrorDataReceived += (_, e) => {
                        output += e.Data;
                        OnProgressChanged(CurrentProgress, e.Data);
                    };
                    process.WaitForExit();
                    if (process.ExitCode != 0) throw new Exception($"forge安装失败", new Exception(output));
                    output = "";
                    ++CurrentStep;
                    CurrentProgress = CurrentStep / Steps;
                }
                return new InstallerResponse { isSuccess = true, Exception = null };
            }
            catch (Exception e)
            {
                return new InstallerResponse { isSuccess = false, Exception = e };
            }
        }

        private string GetForgeFilePath(string Name)
        {
            List<string> FileExtenstionList = Name.Split('@').ToList();
            string LibraryName = FileExtenstionList[0];
            string LibraryPath = MCLibrary.GetMavenFilePathFromName(LibraryName);
            if (FileExtenstionList.Count >= 2) LibraryPath = LibraryPath.TrimEnd(".jar".ToCharArray()) + $".{FileExtenstionList[1]}";
            return LibraryPath;
        }

        private void Mdownloader_ProgressChanged(object sender, (int, int, DownloadResultModel) e)
        {
            double DownloadedProgress = (double)Math.Round((decimal)e.Item1 / e.Item2, 2);
            OnProgressChanged(DownloadedProgress, "下载支持库");
        }

        public InstallerResponse InstallLowerForge(ForgeVersionModel InstallInfo, string GameDir, string VersionName, string ForgePath)
        {
            try
            {
                string ForgeVersion = $"{InstallInfo.MCVersion}-{InstallInfo.Version}";
                OnProgressChanged(0.1, "读取jar");
                ZipArchive archive = new ZipArchive(new FileStream(ForgePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                var universaljar = archive.GetEntry($"forge-{ForgeVersion}-universal.jar");
                if (universaljar == null) throw new Exception("找不到universal文件,无法继续安装");
                var uarchive = new ZipArchive(universaljar.Open());
                var versionjson = uarchive.GetEntry("version.json");
                if (versionjson == null) throw new Exception("找不到version.json");
                string ForgeJsonStr = new StreamReader(versionjson.Open()).ReadToEnd();
                var ForgeJson = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(ForgeJsonStr);
                Stack<DownloadTaskInfo> DownloadFiles = new Stack<DownloadTaskInfo>();
                OnProgressChanged(0.0, "下载支持库");
                foreach (var item in ForgeJson.Libraries)
                {
                    if ((item.ClientReq.HasValue && item.ClientReq.Value == false) || string.IsNullOrEmpty(item.Url)) continue;
                    string LibraryPath = OtherTools.FormatPath(MCLibrary.GetMavenFilePathFromName(item.Name)).TrimStart('/');
                    string DownloadPath = OtherTools.FormatPath(Path.Combine(GameDir, "libraries", LibraryPath));
                    OtherTools.CreateDir(DownloadPath.Substring(0, DownloadPath.LastIndexOf(Path.DirectorySeparatorChar)));
                    string DownloadUrl = item.Url.TrimEnd('/') + "/" + LibraryPath.Replace('\\', '/');
                    if (DownloadAPIManager.Current != DownloadAPIManager.Raw)
                        DownloadUrl = DownloadUrl.Replace(item.Url.TrimEnd('/'), DownloadAPIManager.Current.Libraries.TrimEnd('/'));
                    DownloadTaskInfo info = new DownloadTaskInfo
                    {
                        DestPath = DownloadPath,
                        DownloadUrl = DownloadUrl,
                        MaxTryCount = 4
                    };
                    if(item.CheckSums.Count > 0)
                    {
                        info.isSkipDownloadedFile = true;
                        info.Sha1Vaildate = true;
                        info.Sha1 = item.CheckSums[0];
                    }
                    DownloadFiles.Push(info);
                }
                MultiFileDownloader downloader = new MultiFileDownloader(DownloadFiles, MaxThreadCount);
                downloader.ProgressChanged += Mdownloader_ProgressChanged;
                downloader.StartDownload();
                var result = downloader.WaitDownloadComplete();
                if (result.Result == DownloadResult.Error) throw new Exception($"一个或多个文件下载错误", result.ErrorException);
                string ForgeLibPath = OtherTools.FormatPath(Path.Combine(GameDir, "libraries", "net", "minecraftforge", "forge", ForgeVersion));
                OnProgressChanged(0.6, "复制支持库");
                OtherTools.CreateDir(ForgeLibPath);
                ForgeLibPath = Path.Combine(ForgeLibPath, $"forge-{ForgeVersion}.jar");
                if (File.Exists(ForgeLibPath)) File.Delete(ForgeLibPath);
                universaljar.ExtractToFile(ForgeLibPath);
                ForgeJson.Id = VersionName;
                string VersionPath = OtherTools.FormatPath(Path.Combine(GameDir, "versions", VersionName));
                OtherTools.CreateDir(VersionPath);
                string VersionJsonPath = OtherTools.FormatPath(Path.Combine(VersionPath, VersionName + ".json"));
                OnProgressChanged(0.8, "写入Json");
                File.WriteAllText(VersionJsonPath, JsonConvert.SerializeObject(ForgeJson));
                return new InstallerResponse { isSuccess = true, Exception = null };
            }
            catch (Exception e)
            {
                return new InstallerResponse { isSuccess = false, Exception = e };
            }
        }
    }
}
