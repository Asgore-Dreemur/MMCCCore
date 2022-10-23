using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.Core;
using System.IO;
using System.Security.Cryptography;
using MMCCCore.Wrapper;
using MMCCCore.Model.Wrapper;
using System.Threading;
using MMCCCore.Model.MinecraftFiles;
using System.IO.Compression;
using System.Net;

namespace MMCCCore.Module.Minecraft
{
    public class MCLibrary : InstallerModel
    {
        public MinecraftFilesDownloadInfo DownloadLibraries(LocalMCVersionJsonModel GameInfo, string GameDir, GameSources DownloadSource, bool isSkipDownloadedFile , int MaxThreadCount = 128)
        {
            try
            {
                Stack<DownloadTaskInfo> DownloadStack = new Stack<DownloadTaskInfo>();
                var AllLibraries = GetAllLibraries(GameInfo);
                foreach (MCLibraryInfo LibraryInfo in AllLibraries)
                {
                    if (LibraryInfo.isEnabled)
                    {
                        string LibraryPath = Path.Combine(GameDir, "libraries", LibraryInfo.Path.Replace('/', '\\'));
                        //if (OtherTools.VaildateSha1(LibraryPath, LibraryInfo.CheckSum)) continue;
                        OtherTools.CreateDir(LibraryPath.Substring(0, LibraryPath.LastIndexOf('\\')));
                        var DownloadInfo = new DownloadTaskInfo { DownloadUrl = GetLibrariesDownloadAddr(LibraryInfo, DownloadSource),
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
                    if(e.Item3.Result == DownloadResult.Error)
                    {
                        downloader.needStop = true;
                    }
                    double DownloadedProgress = (double)Math.Round((decimal)e.Item1 / e.Item2, 2);
                    OnProgressChanged(DownloadedProgress, "下载支持库");
                };
                downloader.StartDownload();
                downloader.WaitDownloadComplete();
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
                                if (!vresult.isSuccess) throw vresult.ErrorException;
                                if (!vresult.isVaildated)
                                {
                                    if (File.Exists(CNativePath)) File.Delete(CNativePath);
                                    file.ExtractToFile(CNativePath);
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
        private string GetLibrariesDownloadAddr(MCLibraryInfo info, GameSources sources)
        {
            switch (sources)
            {
                case GameSources.Bmclapi:
                    return info.Url.Replace("https://libraries.minecraft.net/", "https://bmclapi2.bangbang93.com/maven/");
                case GameSources.Mcbbs:
                    return info.Url.Replace("https://libraries.minecraft.net/", "https://download.mcbbs.net/maven/");
                case GameSources.Vanilla:
                    return info.Url;
                default:
                    return info.Url;
            }
        }
    }
}
