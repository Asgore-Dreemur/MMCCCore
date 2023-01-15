using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMCCCore.Core.Module.APIManager
{
    public static class DownloadAPIManager
    {
        public static DownloadAPIRoot Bmclapi { get; } = new DownloadAPIRoot
        {
            CoreJson = "https://bmclapi2.bangbang93.com/version/<version>/json",
            CoreJar = "https://bmclapi2.bangbang93.com/version/<version>/client",
            VersionManifest = "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
            Libraries = "https://bmclapi2.bangbang93.com/maven",
            Assets = "https://bmclapi2.bangbang93.com/assets/",
            AssetIndex = "bmclapi2.bangbang93.com"
        };
        public static DownloadAPIRoot Mcbbs { get; } = new DownloadAPIRoot
        {
            CoreJson = "https://download.mcbbs.net/version/<version>/json",
            CoreJar = "https://download.mcbbs.net/version/<version>/client",
            VersionManifest = "https://download.mcbbs.net/mc/game/version_manifest_v2.json",
            Libraries = "https://download.mcbbs.net/maven",
            Assets = "https://download.mcbbs.net/assets/",
            AssetIndex = "download.mcbbs.net"
        };
        public static DownloadAPIRoot Raw { get; } = new DownloadAPIRoot
        {
            VersionManifest = "http://launchermeta.mojang.com/mc/game/version_manifest_v2.json",
            Libraries = "https://libraries.minecraft.net",
            Assets = "http://resources.download.minecraft.net"
        };
        public static DownloadAPIRoot Current { get; set; }
    }
    public class DownloadAPIRoot
    {
        public string VersionManifest { get; set; }
        public string Libraries { get; set; }
        public string CoreJar { get; set; }
        public string CoreJson { get; set; }
        public string Assets { get; set; }
        public string AssetIndex { get; set; }
    }
}
