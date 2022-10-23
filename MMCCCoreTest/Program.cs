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
            /*foreach(var item in CoreWrapper.GetMCVersions(GameSources.Mcbbs).AllVersions)
            {
                if(item.Id == "1.16.5")
                {
                    MinecraftInstaller installer = new MinecraftInstaller("C:\\MMCCTest.Minecraft", item, GameSources.Mcbbs, "1.16.5APITest");
                    installer.ProgressChanged += Installer_ProgressChanged;
                    var result = installer.InstallMinecraft(true);
                    if (result.Exception != null) Console.WriteLine(result.Exception.Message);
                    break;
                }
            }
            var item = CoreWrapper.GetCoreForId("C:\\MMCCTest.Minecraft", "1.16.5Test71");
            MinecraftLauncher launcher = new MinecraftLauncher(item, OfflineAuthenticator.OfflineAuthenticate("Asriel"),
                new LauncherSettings
                {
                    JvmSettings = new LauncherJvmSettings
                    {
                        JavawPath = "java",
                        MinMemory = 512,
                        MaxMemory = 1000
                    }
                });
            launcher.Minecraft_LogAdded += (_, e) => Console.WriteLine(e);
            var result = launcher.LaunchMinecraft();
            result.MCProcess.WaitForExit();*/
            Optifine optifine = new Optifine();
            var ilist = Optifine.GetOptifineVersionsFromVersion("1.16.5");
            foreach(var item in ilist)
            {
                if(item.FileName == "OptiFine_1.16.5_HD_U_G8.jar")
                {
                    optifine.ProgressChanged += Optifine_ProgressChanged;
                    optifine.InstallOptifine("C:\\MMCCTest.Minecraft", "1.16.5OptifineTest", item);
                    break;
                }
            }
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
