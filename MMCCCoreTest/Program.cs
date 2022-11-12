using System;
using System.Threading.Tasks;
using MMCCCore;
using MMCCCore.Module.Authenticator;
using MMCCCore.Model.Authenticator;
using System.Security.Permissions;
using MMCCCore.Wrapper;
using MMCCCore.Module.GameAssemblies;
using System.Collections.Generic;
using MMCCCore.Model.Assemblies;
using MMCCCore.Module.Minecraft;
using MMCCCore.Model.Mod;
using MMCCCore.Module.Mod;
using MMCCCore.Module.Launcher;
using MMCCCore.Model.Core;

namespace MMCCCoreTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string PlayerName = "Asriel";
            string MinecraftDir = ".minecraft";
            string VersionName = "1.16.5";
            Account account = OfflineAuthenticator.OfflineAuthenticate(PlayerName);
            var LaunchCore = CoreWrapper.GetCoreForId(MinecraftDir, VersionName);
            LauncherSettings settings = new LauncherSettings
            {
                JvmSettings = new LauncherJvmSettings
                {
                    JavawPath = "java",
                    MaxMemory = 1100,
                    MinMemory = 512
                },
            };
            MinecraftLauncher launcher = new MinecraftLauncher(LaunchCore, account, settings);
            //launcher.Minecraft_LogAdded += MinecraftLauncher_LogAdded;
            //launcher.Minecraft_Exited += MinecraftLauncher_Exited;
            launcher.LaunchMinecraft();

        }

        private static void Authenticator_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"Status:{(int)(e.Item1 * 100)}({e.Item2})");
        }

        private static void Optifine_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Installer_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }
    }
}
