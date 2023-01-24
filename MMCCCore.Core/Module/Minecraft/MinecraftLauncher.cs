using MMCCCore.Core.Model.Authenticator;
using MMCCCore.Core.Model.Core;
using MMCCCore.Core.Model.Launch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MMCCCore.Core.Module.Minecraft;
using MMCCCore.Core.Wrapper;
using Newtonsoft.Json;

namespace MMCCCore.Core.Module.Launcher
{
    public class MinecraftLauncher
    {
        private LocalGameInfoModel LaunchCore;
        private Account LaunchAccount;
        private LauncherSettings LaunchSetting;
        public event EventHandler<string> Minecraft_LogAdded;
        public event EventHandler<int> Minecraft_Exited;
        public MinecraftLauncher(LocalGameInfoModel LauncherCore, Account LauncherAccount, LauncherSettings LauncherSetting)
        {
            this.LaunchAccount = LauncherAccount;
            this.LaunchCore = LauncherCore;
            this.LaunchSetting = LauncherSetting;
        }

        public MCLaunchResponse LaunchMinecraft()
        {
            try
            {
                string LibrariesDir = Path.Combine(LaunchCore.GameRootDir, "libraries");
                string AssetPath = Path.Combine(LaunchCore.GameRootDir, "assets");
                if (!Directory.Exists(LibrariesDir) ||
                    !Directory.Exists(AssetPath)) return new MCLaunchResponse()
                {
                    LaunchResult = LaunchStatus.Error,
                    LaunchAccount = LaunchAccount,
                    LaunchArgs = LaunchSetting,
                    LaunchCore = LaunchCore,
                    ErrorMessage = "启动需要的目录不存在"
                };
                string LaunchArgs = BuildArguments();
                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = LaunchSetting.JvmSettings.JavawPath,
                        Arguments = LaunchArgs,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = false
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.OutputDataReceived += (_, e) => { Minecraft_LogAdded?.Invoke(this, e.Data); };
                process.ErrorDataReceived += (_, e) => { Minecraft_LogAdded?.Invoke(this, e.Data); };
                process.Exited += (_, e) => Minecraft_Exited?.Invoke(this, process.ExitCode);
                return new MCLaunchResponse()
                {
                    LaunchResult = LaunchStatus.Success,
                    LaunchAccount = LaunchAccount,
                    LaunchArgs = LaunchSetting,
                    LaunchCore = LaunchCore,
                    MCProcess = process,
                    Arguments = LaunchArgs
                };
            }
            catch (Exception e)
            {
                return new MCLaunchResponse()
                {
                    LaunchResult = LaunchStatus.Error,
                    LaunchAccount = LaunchAccount,
                    LaunchArgs = LaunchSetting,
                    LaunchCore = LaunchCore,
                    ErrorMessage = e.Message
                };
            }
        }
        private List<string> GetLibraries()
        {
            string LibrariesPath = Path.Combine(LaunchCore.GameRootDir, "libraries");
            List<string> GameLibraries = new List<string>();
            foreach (MCLibraryInfo LibraryInfo in MCLibrary.GetAllLibraries(LaunchCore.VersionJson))
            {
                if (!LibraryInfo.isEnabled) continue;
                if (!LibraryInfo.isNative) GameLibraries.Add(Path.Combine(LibrariesPath, LibraryInfo.Path.Replace('/', '\\')));
            }
            GameLibraries.Add(LaunchCore.VersionJson.InheritsFrom == null
                ? Path.Combine(LaunchCore.GameRootDir, "versions", LaunchCore.Id, LaunchCore.Id + ".jar")
                : Path.Combine(LaunchCore.GameRootDir, "versions", LaunchCore.VersionJson.InheritsFrom, LaunchCore.VersionJson.InheritsFrom + ".jar"));
            return GameLibraries;
        }

        private string BuildArguments()
        {
            var HandledCore = HandleCurrentCore();
            return string.Join(" ", GetJvmArguments(HandledCore).Concat(GetGCArguments(HandledCore)).ToList());
        }

        private List<string> GetJvmArguments(LocalGameInfoModel GameCore)
        {
            Dictionary<string, string> LaunchJvmArguments = new Dictionary<string, string>()
            {
                {"${natives_directory}", 
                    GameCore.APIType == GameAPIType.Vanilla ? "\"" + Path.Combine(GameCore.GameRootDir, "versions", GameCore.Id, "natives") + "\""
                    : "\"" + Path.Combine(GameCore.GameRootDir, "versions", (GameCore.VersionJson.InheritsFrom != null ?
                    GameCore.VersionJson.InheritsFrom :
                    GameCore.VersionJson.ClientVersion), "natives") + "\""
                },
                {"${launcher_name}", "MMCCCore.Core" },
                {"${launcher_version}", "1.0" },
                {"${classpath}", "\"" + string.Join(OtherTools.JavaCPSeparatorChar.ToString(), GetLibraries()) + "\"" }
            };
            List<string> JvmArguments = new List<string>();
            JvmArguments.AddRange(new string[] { $"-Xmn{LaunchSetting.JvmSettings.MinMemory}m", $"-Xmx{LaunchSetting.JvmSettings.MaxMemory}m" });
            if (LaunchSetting.JvmSettings.OtherArguments.Count > 0) JvmArguments = JvmArguments.Concat(LaunchSetting.JvmSettings.OtherArguments).ToList();
            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                JvmArguments.Add("-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump");
                if (Environment.OSVersion.Version.Major == 10) JvmArguments.Add("-Dos.name=\"Windows 10\" -Dos.version=\"10.0\"");
            }
            if (!Environment.Is64BitOperatingSystem) JvmArguments.Add("-Xss1M");
            if (Environment.OSVersion.Platform == PlatformID.MacOSX) JvmArguments.Add("-XstartOnFirstThread");
            if (LaunchCore.VersionJson.Arguments != null)
            {
                if (LaunchCore.VersionJson.Arguments.Jvm.Count > 0)
                {
                    foreach (JToken JvmArg in LaunchCore.VersionJson.Arguments.Jvm)
                    {
                        if (JvmArg.Type == JTokenType.Object) continue;
                        JvmArguments.Add(JvmArg.ToString());
                    }
                }
                else JvmArguments.AddRange(new string[] { "-Djava.library.path=${natives_directory}", "-Dminecraft.launcher.brand=${launcher_name}", "-Dminecraft.launcher.version=${launcher_version}", "-cp ${classpath}" });
            }
            else JvmArguments.AddRange(new string[] { "-Djava.library.path=${natives_directory}", "-Dminecraft.launcher.brand=${launcher_name}", "-Dminecraft.launcher.version=${launcher_version}", "-cp ${classpath}" });
            foreach (KeyValuePair<string, string> JvmRepArg in LaunchJvmArguments)
            {
                for (int i = 0; i < JvmArguments.Count; i++)
                {
                    if (JvmArguments[i].Contains(JvmRepArg.Key)) { JvmArguments[i] = JvmArguments[i].Replace(JvmRepArg.Key, JvmRepArg.Value); break; }
                }
            }
            JvmArguments.Add(LaunchCore.VersionJson.MainClass);
            return JvmArguments;
        }

        private LocalGameInfoModel HandleCurrentCore()
        {
            LocalGameInfoModel rmodel = this.LaunchCore;
            if(rmodel.APIType != GameAPIType.Vanilla)
            {
                string VanilaJsonPath = Path.Combine(rmodel.GameRootDir,
                    "versions",
                    rmodel.VersionJson.InheritsFrom,
                    rmodel.VersionJson.InheritsFrom + ".json");
                if (!File.Exists(VanilaJsonPath)) throw new Exception("此核心非原版,但它的原版json不存在");
                var VanilaJson = JsonConvert.DeserializeObject<LocalMCVersionJsonModel>(File.ReadAllText(VanilaJsonPath));
                if (VanilaJson == null) throw new Exception("此核心非原版,但它的原版json无效");
                rmodel.VersionJson.AssetIndex = VanilaJson.AssetIndex;
                if (rmodel.VersionJson.Arguments != null)
                {
                    rmodel.VersionJson.Arguments.Jvm = VanilaJson.Arguments.Jvm.Concat(rmodel.VersionJson.Arguments.Jvm).Distinct().ToList();
                    rmodel.VersionJson.Arguments.Game = VanilaJson.Arguments.Game.Concat(rmodel.VersionJson.Arguments.Game).Distinct().ToList();
                }
                rmodel.VersionJson.Libraries = rmodel.VersionJson.Libraries.Concat(VanilaJson.Libraries).Distinct().ToList();
                rmodel.VersionJson.Downloads = VanilaJson.Downloads;
            }
            return rmodel;
        }

        private List<string> GetGCArguments(LocalGameInfoModel GameCore)
        {
            Dictionary<string, string> LaunchGCArguments = new Dictionary<string, string>()
            {
                {"${auth_player_name}", LaunchAccount.Name },
                {"${version_name}", LaunchCore.Id},
                {"${game_directory}", "\"" + Path.Combine(GameCore.GameRootDir, "versions", GameCore.Id)  + "\""},
                {"${assets_root}", "\"" + Path.Combine(GameCore.GameRootDir, "assets") + "\"" },
                {"${assets_index_name}", GameCore.VersionJson.AssetIndex.Id },
                {"${auth_uuid}", LaunchAccount.Uuid.ToString("N") },
                {"${auth_access_token}", LaunchAccount.AccessToken },
                {"${user_type}", LaunchAccount.LoginType == AccountType.Offline ? "Legacy" : "Mojang" },
                {"${version_type}", GameCore.VersionJson.Type },
                {"${user_properties}", "{}" }
            };

            List<string> GCArguments = new List<string>();
            if (LaunchCore.VersionJson.MinecraftArguments != null)
            {
                GCArguments = GCArguments.Concat(GameCore.VersionJson.MinecraftArguments.Split(' ').ToList()).ToList();
            }
            if (LaunchCore.VersionJson.Arguments != null && LaunchCore.VersionJson.Arguments.Game.Count > 0)
            {
                foreach (JToken GCArg in GameCore.VersionJson.Arguments.Game)
                {
                    if (GCArg.Type == JTokenType.Object) continue;
                    GCArguments.Add(GCArg.ToString());
                }
            }
            if (LaunchSetting.GameWindowSetting != null)
            {
                if (LaunchSetting.GameWindowSetting.isFullScreen) GCArguments.Add("--fullscreen");
                else if (LaunchSetting.GameWindowSetting.WindowsHeight != -1 && LaunchSetting.GameWindowSetting.WindowsWidth != -1)
                {
                    GCArguments.Add($"--width {LaunchSetting.GameWindowSetting.WindowsWidth} --height {LaunchSetting.GameWindowSetting.WindowsHeight}");
                }
            }
            if (LaunchSetting.ServerSetting != null)
            {
                GCArguments.Add($"--server {LaunchSetting.ServerSetting.ServerIP} --port {LaunchSetting.ServerSetting.ServerPort}");
            }
            foreach (KeyValuePair<string, string> GCRepArg in LaunchGCArguments)
            {
                for (int i = 0; i < GCArguments.Count; i++)
                {
                    if (GCArguments[i].Contains(GCRepArg.Key)) { GCArguments[i] = GCArguments[i].Replace(GCRepArg.Key, GCRepArg.Value); break; }
                }
            }
            return GCArguments;
        }
    }
}
