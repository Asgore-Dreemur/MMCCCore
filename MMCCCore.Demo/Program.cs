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

namespace MMCCCore.Core.Core.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            DownloadAPIManager.Current = DownloadAPIManager.Mcbbs;
            /*var LaunchSettings = new LauncherSettings()
            {
                JvmSettings = new LauncherJvmSettings
                {
                    JavawPath = @"C:\ndmdl\jre-9.0.4_windows-x64_bin\jre-9.0.4\bin\java.exe",
                    MaxMemory = 600,
                    MinMemory = 300
                }
            };
            var item = CoreWrapper.GetCoreForId("C:\\MMCCTest.Minecraft", "1.16.5-forge-36.0.32");
            MinecraftLauncher launcher = new MinecraftLauncher(item,
                OfflineAuthenticator.OfflineAuthenticate("Asriel"),
                LaunchSettings);
            launcher.Minecraft_LogAdded += Launcher_Minecraft_LogAdded;
            launcher.Minecraft_Exited += Launcher_Minecraft_Exited;
            var result = launcher.LaunchMinecraft();
            Console.WriteLine($"Result:{Convert.ToString(result.LaunchResult)}");
            result.MCProcess.WaitForExit();*/
            /*var item = Fabric.GetFabricVersionsFromMCVersion("1.16.5");
            Fabric fabric = new Fabric();
            fabric.ProgressChanged += Fabric_ProgressChanged;
            var res = fabric.InstallFabric("C:\\MMCCTest.Minecraft", "1.16.5Fabric2", item[0], @"1.16.5");*/
            Forge forge = new Forge();
            forge.ProgressChanged += Forge_ProgressChanged;
            var forgeinfo = Forge.GetForgeVersionsFromVersion("1.16.5")[2];
            var res = forge.InstallForge(Forge.GetForgeVersionsFromVersion("1.16.5").Last(), "C:\\MMCCTest.Minecraft", "1.16.5ForgeTest76", @"I:\Program Files\Java\jdk1.8.0_202\bin\java.exe");
            /*string DownloadUrls = "";
            Stack<DownloadTaskInfo> stack = new Stack<DownloadTaskInfo>();
            List<string> DownloadList = DownloadUrls.Split('\n').ToList();
            foreach(var item in DownloadList)
            {
                string DestPath = $"C:\\InstallForgeTest\\{item.Split('/').Last()}";
                stack.Push(new DownloadTaskInfo
                {
                    DestPath = DestPath,
                    DownloadUrl = item,
                    isSkipDownloadedFile = true,
                    MaxTryCount = 4,
                    Sha1Vaildate = false
                });
            }
            MultiFileDownloader downloader = new MultiFileDownloader(stack, 64);
            downloader.ProgressChanged += Downloader_ProgressChanged;
            downloader.StartDownload();
            downloader.WaitDownloadComplete();*/
            /*DownloadAPIManager.Current = DownloadAPIManager.Mcbbs;
            var result = CoreWrapper.GetMCVersions();
            var installcore = result.AllVersions.Find(i => i.Id == "1.16.5");
            MinecraftInstaller installer = new MinecraftInstaller("C:\\MMCCTest.Minecraft", installcore, "1.16.5");
            installer.ProgressChanged += Forge_ProgressChanged;
            var res = installer.InstallMinecraft();*/
            /*Console.WriteLine(Convert.ToString(res.isSuccess));
            if (!res.isSuccess) Console.WriteLine(res.Exception.Message);*/
            /*Optifine optifine = new Optifine();
            optifine.ProgressChanged += Optifine_ProgressChanged;
            var info = Optifine.GetOptifineVersionsFromVersion("1.16.5")[0];
            var res = optifine.InstallOptifine("C:\\MMCCTest.Minecraft", "Optifine1.16.5", info, @"I:\Program Files\Java\jdk1.8.0_202\bin\java.exe");*/
            /*Console.WriteLine(Convert.ToString(res.isSuccess));
            if (!res.isSuccess) Console.WriteLine(res.Exception.Message);*/
            /*MCLibrary library = new MCLibrary();
            library.ProgressChanged += Library_ProgressChanged;
            library.DownloadLibraries(CoreWrapper.GetCoreForId("C:\\MMCCTest.Minecraft", "1.16.5Fabric").VersionJson, "C:\\MMCCTest.Minecraft", true);*/
            ;
        }

        private static void Authenticator_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Library_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Optifine_ProgressChanged(object sender, (double, string) e)
        {
            Console.WriteLine($"status:{e.Item1 * 100}({e.Item2})");
        }

        private static void Downloader_ProgressChanged(object sender, (int, int, DownloadResultModel) e)
        {
            double DownloadedProgress = (double)Math.Round((decimal)e.Item1 / e.Item2, 2);
            Console.WriteLine($"Download status:{DownloadedProgress * 100}");
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
