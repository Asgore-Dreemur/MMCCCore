using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMCCCore.Core.Model.Core
{
    public class LauncherSettings
    {
        public LauncherJvmSettings JvmSettings { get; set; }
        public GameWindowSettings GameWindowSetting { get; set; }
        public AutoJoinServerSettings ServerSetting { get; set; }
        public bool isDemoUser { get; set; }
        public string AdvancedArguments { get; set; }
        public LauncherSettings() { }
        public LauncherSettings(LauncherJvmSettings jvmSettings) => this.JvmSettings = jvmSettings;
    }
    public class LauncherJvmSettings
    {
        public string JavawPath { get; set; }
        public int MaxMemory { get; set; }
        public int MinMemory { get; set; }
        public List<string> OtherArguments { get; set; } = new List<string>();
    }
    public class GameWindowSettings
    {
        public int WindowsWidth { get; set; } = -1;
        public int WindowsHeight { get; set; } = -1;
        public bool isFullScreen { get; set; } = false;
    }
    public class AutoJoinServerSettings
    {
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
    }
}
