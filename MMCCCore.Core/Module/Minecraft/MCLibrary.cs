using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.Core;
using System.IO;
using System.Security.Cryptography;
using MMCCCore.Core.Wrapper;
using MMCCCore.Core.Model.Wrapper;
using System.Threading;
using MMCCCore.Core.Model.MinecraftFiles;
using System.IO.Compression;
using System.Net;
using MMCCCore.Core.Module.APIManager;

namespace MMCCCore.Core.Module.Minecraft
{
    public class MCLibrary : InstallerModel
    {
        public MinecraftFilesDownloadInfo DownloadLibraries(LocalMCVersionJsonModel GameInfo, string GameDir, bool isSkipDownloadedFile , int MaxThreadCount = 128)
        {
            try
            {
                if (DownloadAPIManager.Current == null) throw new Exception("未知的下载源");
                Stack<DownloadTaskInfo> DownloadStack = new Stack<DownloadTaskInfo>();
                var AllLibraries = GetAllLibraries(GameInfo);
                foreach (MCLibraryInfo LibraryInfo in AllLibraries)
                {
                    if (LibraryInfo.isEnabled)
                    {
                        string LibraryPath = Path.Combine(GameDir, "libraries", LibraryInfo.Path.Replace('/', '\\'));
                        OtherTools.CreateDir(LibraryPath.Substring(0, LibraryPath.LastIndexOf('\\')));
                        var DownloadInfo = new DownloadTaskInfo { DownloadUrl = DownloadAPIManager.Current.Libraries.TrimEnd('/') + $"/{LibraryInfo.Path}",
                            DestPath = LibraryPath,
                            MaxTryCount = 4,
                            Sha1Vaildate = true,
                            Sha1 = LibraryInfo.CheckSum,
                            isSkipDownloadedFile = isSkipDownloadedFile };
                        DownloadStack.Push(DownloadInfo);
                    }
                }
                MultiFileDownloader downloader = new MultiFileDownloader(DownloadStack, MaxThreadCount);
                downloader.ProgressChanged += (_, e) =>
                {
                    double DownloadedProgress = (double)Math.Round((decimal)e.Item1 / e.Item2, 2);
                    OnProgressChanged(DownloadedProgress, "下载支持库");
                };
                downloader.StartDownload();
                var result = downloader.WaitDownloadComplete();
                if (result.Result == DownloadResult.Error) throw new Exception("下载支持库时出现一个或多个文件错误", result.ErrorException);
                string NativesPath = Path.Combine(GameDir, "versions", GameInfo.Id, "natives");
                foreach(var item in AllLibraries)
                {
                    if (item.isNative && item.isEnabled)
                    {
                        string NativePath = Path.Combine(GameDir, "libraries", item.Path.Replace('/', '\\'));
                        ZipArchive archive = new ZipArchive(new FileStream(NativePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                        foreach (var file in archive.Entries)
                        {
                            if (file.Name == "") continue;
                            if (file.Name.Substring(file.Name.LastIndexOf('.')) == ".dll")
                            {
                                string CNativePath = Path.Combine(NativesPath, file.Name);
                                ZipArchiveEntry entry = archive.GetEntry(file.Name + ".sha1");
                                if (entry == null)
                                {
                                    if (File.Exists(CNativePath)) File.Delete(CNativePath);
                                    file.ExtractToFile(CNativePath);
                                    continue;
                                }
                                string FileSha1 = new StreamReader(entry.Open()).ReadToEnd();
                                var vresult = OtherTools.VaildateSha1(CNativePath, FileSha1);
                                if (!vresult.isVaildated)
                                {
                                    if (File.Exists(CNativePath)) File.Delete(CNativePath);
                                    file.ExtractToFile(CNativePath);
                                    vresult = OtherTools.VaildateSha1(CNativePath, FileSha1);
                                    if (vresult.ErrorException != null) throw vresult.ErrorException;
                                }
                            }
                        }
                        archive.Dispose();
                    }
                }
                return new MinecraftFilesDownloadInfo()
                {
                    DownloadResult = MinecraftFilesDownloadResult.Success,
                };
            }catch(Exception e)
            {
                return new MinecraftFilesDownloadInfo
                {
                    DownloadResult = MinecraftFilesDownloadResult.Error,
                    ErrorException = e
                };
            }
        }

        public static string GetMavenFilePathFromName(string Name)
        {
            var SplitArray = Name.Split(':').ToList();
            string NamespacePath = SplitArray[0].Replace('.', Path.DirectorySeparatorChar);
            string PackageName = SplitArray[1];
            string PackageVersion = SplitArray[2];
            if(SplitArray.Count > 3)
            {
                var extarray = SplitArray.GetRange(3, SplitArray.Count - 3);
                string JarName = PackageVersion + $"-{string.Join("-", extarray)}";
                return $"{NamespacePath}{Path.DirectorySeparatorChar}{PackageName}{Path.DirectorySeparatorChar}{PackageVersion}{Path.DirectorySeparatorChar}{PackageName}-{JarName}.jar";
            }
            return $"{NamespacePath}{Path.DirectorySeparatorChar}{PackageName}{Path.DirectorySeparatorChar}{PackageVersion}{Path.DirectorySeparatorChar}{PackageName}-{PackageVersion}.jar";
        }

        public static List<MCLibraryInfo> GetAllLibraries(LocalMCVersionJsonModel GameInfo)
        {
            List<MCLibraryInfo> LibrariesList = new List<MCLibraryInfo>();
            foreach(MCLibraryFileModel model in GameInfo.Libraries)
            {
                MCLibraryInfo info = new MCLibraryInfo
                {
                    LibraryJson = model,
                    Url = ((model.Downloads?.Artifact?.Url) ?? string.Empty) + model.Url,
                    Size = (model.Downloads?.Artifact?.Size == null) ? 0 : (int)model.Downloads?.Artifact?.Size,
                    Name = model.Name,
                    isEnabled = true,
                    isNative = false,
                    Path = model.Downloads?.Artifact?.Path ?? string.Empty,
                    CheckSum = model.Downloads?.Artifact?.Sha1 ?? string.Empty
                };
                if(model.Downloads == null)
                {
                    if (model.Name == null) continue;
                    info.Url = DownloadAPIManager.Current.Libraries.TrimEnd('/') + $"/{GetMavenFilePathFromName(model.Name)}";
                    info.Path = GetMavenFilePathFromName(model.Name);
                    LibrariesList.Add(info);
                    continue;
                }
                if (model.Downloads.Classifiers != null)
                {
                    info.isNative = true;
                    if (model.Rules != null) info.isEnabled = isTheLibraryCanRunOnThisSystem(model);
                    info.isEnabled = IsTheNativeCanRunOnThisSystem(model);
                    if (info.isEnabled)
                    {
                        info.Name += $":{model.Natives[OtherTools.GetSystemPlatformName()].Replace("${arch}", OtherTools.GetArch().ToString())}";
                        var Nativefile = model.Downloads.Classifiers[model.Natives[OtherTools.GetSystemPlatformName()].Replace("${arch}", OtherTools.GetArch().ToString())];
                        info.Size = Nativefile.Size;
                        info.Path = Nativefile.Path;
                        info.Url = Nativefile.Url;
                        info.CheckSum = Nativefile.Sha1;
                        LibrariesList.Add(info);
                    }
                    else LibrariesList.Add(info);
                }
                else if (model.Downloads.Artifact != null)
                {
                    if (model.Rules != null) info.isEnabled = isTheLibraryCanRunOnThisSystem(model);
                    info.CheckSum = model.Downloads.Artifact.Sha1;
                    LibrariesList.Add(info);
                }
            }
            string tmp = "";
            foreach (var item in LibrariesList) tmp += item.Url + ";";
            return LibrariesList;
        }
        private static bool isTheLibraryCanRunOnThisSystem(MCLibraryFileModel model)
        {
            bool windows, unix, macos;
            windows = unix = macos = false;
            foreach(var item in model.Rules)
            {
                if (item.Action == "allow")
                {
                    if (item.Os == null)
                    {
                        windows = macos = unix = true;
                        continue;
                    }
                    else
                    {
                        foreach (var os in item.Os)
                        {
                            switch (os.Value)
                            {
                                case "windows":
                                    windows = true;
                                    break;
                                case "linux":
                                    unix = true;
                                    break;
                                case "osx":
                                    macos = true;
                                    break;
                            }
                        }
                    }
                }
                else if(item.Action == "disallow"){
                    if (item.Os == null)
                    {
                        windows = macos = unix = false;
                        continue;
                    }
                    else
                    {
                        foreach (var os in item.Os)
                        {
                            switch (os.Value)
                            {
                                case "windows":
                                    windows = false;
                                    break;
                                case "linux":
                                    unix = false;
                                    break;
                                case "osx":
                                    macos = false;
                                    break;
                            }
                        }
                    }
                }
            }
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                    return false;
                case PlatformID.Win32Windows:
                    return false;
                case PlatformID.Win32NT:
                    return windows;
                case PlatformID.WinCE:
                    return false;
                case PlatformID.Unix:
                    return unix;
                case PlatformID.Xbox:
                    return false;
                case PlatformID.MacOSX:
                    return macos;
                default:
                    return false;
            }
        }
        private static bool IsTheNativeCanRunOnThisSystem(MCLibraryFileModel model)
        {
            if (model.Natives.ContainsKey(OtherTools.GetSystemPlatformName())) return true;
            return false;
        }
    }
}
