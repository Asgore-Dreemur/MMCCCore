using MMCCCore.Core.Model.Wrapper;
using MMCCCore.Core.Module.APIManager;
using System;
using System.Linq;
using System.IO;
using MMCCCore.Core.Core;
using MMCCCore.Core.Model.Authenticator;
using MMCCCore.Core.Module.Authenticator;
using MMCCCore.Core.Model.MinecraftFiles;
using MMCCCore.Core.Module.Minecraft;
using MMCCCore.Core.Module.GameAssemblies;
using MMCCCore.Core.Model.GameAssemblies;
using MMCCCore.Core.Model.Core;
using MMCCCore.Core.Module.Launcher;

namespace MMCCCore.Core.Core.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            DownloadAPIManager.Current = DownloadAPIManager.Mcbbs;
            
        }

        public static InstallerResponse InstallVanillaMinecraft(string GameRoot, MCVersionModel model, string VersionName, int MaxThreadCount = 64, bool isSkipFile = true)
        {
            MinecraftInstaller installer = new MinecraftInstaller(GameRoot, model, VersionName, MaxThreadCount);
            installer.ProgressChanged += Installer_ProgressChanged;
            return installer.InstallMinecraft(isSkipFile);
        }

        public static InstallerResponse InstallForge(string GameRoot, ForgeVersionModel model, string VersionName, string JavaPath, int MaxThreadCount)
        {
            Forge forge = new Forge();
            forge.GameDir = GameRoot;
            forge.VersionName = VersionName;
            forge.JavaPath = JavaPath;
            forge.MaxThreadCount = MaxThreadCount;
            forge.ProgressChanged += Forge_ProgressChanged;
            return forge.InstallForge(model);
        }

        public static InstallerResponse InstallFabric(string GameRoot, FabricVersionModel model, string VersionName, string VanillaVersionName, int MaxThreadCount)
        {
            Fabric fabric = new Fabric();
            fabric.GameDir = GameRoot;
            fabric.VersionName = VersionName;
            fabric.MaxThreadCount = MaxThreadCount;
            fabric.VanillaVersionName = VanillaVersionName;
            fabric.ProgressChanged += Fabric_ProgressChanged;
            return fabric.InstallFabric(model);
        }

        public static InstallerResponse InstallLiteLoader(string GameRoot, LiteLoaderVersionModel model, string VersionName, string VanillaVersionName)
        {
            LiteLoader liteloader = new LiteLoader();
            liteloader.GameDir = GameRoot;
            liteloader.VersionName = VersionName;
            liteloader.VanillaVersionName = VanillaVersionName;
            return liteloader.InstallLiteLoader(model);
        }

        public static InstallerResponse InstallOptifine(string GameRoot, OptifineVersionModel model, string VersionName, string JavaPath)
        {
            Optifine optifine = new Optifine();
            optifine.GameDir = GameRoot;
            optifine.VersionName = VersionName;
            optifine.JavaPath = JavaPath;
            optifine.ProgressChanged += Optifine_ProgressChanged;
            return optifine.InstallOptifine(model);
        }

        public static MicrosoftAccount MicrosoftAuth(string clientid)
        {
            MicrosoftAuthenticator authenticator = new MicrosoftAuthenticator(clientid);
            var result1 = authenticator.OAuth2AuthenticateTaskAsync().GetAwaiter().GetResult();
            //这里提醒用户验证
            var result2 = authenticator.OAuth2TokenAuthenticateTaskAsync(result1).GetAwaiter().GetResult();
            authenticator.OAuth2Response = result2;
            authenticator.ProgressChanged += Authenticator_ProgressChanged;
            var result3 = authenticator.Authenticate(false);
            return result3;
        }

        public static YggdrasilAccount YggdrasilAuth(string YggdrasilServerAddr, string Username, string Password)
        {
            YggdrasilAuthenticator authenticator = new YggdrasilAuthenticator(YggdrasilServerAddr, Username, Password);
            return authenticator.Authenticate();
        }

        public static void LaunchMinecraftAsYggdrasil(LocalGameInfoModel LauncherCore, YggdrasilAccount LauncherAccount, LauncherSettings LauncherSetting, string authlib)
        {
            LauncherSetting.JvmSettings.OtherArguments = LauncherSetting.JvmSettings.OtherArguments.Concat(YggdrasilAuthenticator.GenerateYggdrasilLaunchArgs(authlib, LauncherAccount.ServerAddr).GetAwaiter().GetResult()).ToList();
            MinecraftLauncher launcher = new MinecraftLauncher(LauncherCore, LauncherAccount, LauncherSetting);
            launcher.Minecraft_LogAdded += Launcher_Minecraft_LogAdded;
            launcher.Minecraft_Exited += Launcher_Minecraft_Exited;
            launcher.LaunchMinecraft();
        }

        public static void LaunchMinecraft(LocalGameInfoModel LauncherCore, Account LauncherAccount, LauncherSettings LauncherSetting, string authlib)
        {
            MinecraftLauncher launcher = new MinecraftLauncher(LauncherCore, LauncherAccount, LauncherSetting);
            launcher.Minecraft_LogAdded += Launcher_Minecraft_LogAdded;
            launcher.Minecraft_Exited += Launcher_Minecraft_Exited;
            launcher.LaunchMinecraft();
        }

        private static void Installer_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Authenticator_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Optifine_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Forge_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Fabric_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Launcher_Minecraft_Exited(object sender, int e)
        {
            Console.WriteLine($"Exited,Return Code:{e}");
        }

        private static void Launcher_Minecraft_LogAdded(object sender, string e)
        {
            Console.WriteLine(e);
        }
    }
}
