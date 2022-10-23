using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.ModAPI;
using MMCCCore.Model.Version;

namespace MMCCCore.Model
{
    class MinecraftInstalllerModel
    {
        public string MCGameDir { get; set; }
        public string VersionName { get; set; }
        public OptifineModel Optifine { get; set; }
        public GameSources InstallSource { get; set; }
        public string GameVersion { get; set; }
        public string VanillaJsonUrl { get; set; }
    }
}
