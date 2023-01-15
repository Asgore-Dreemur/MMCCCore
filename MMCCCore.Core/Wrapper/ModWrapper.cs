using System;
using Newtonsoft.Json;
using System.Threading;
using MMCCCore.Core.Model;
using System.IO.Compression;
using System.IO;
using System.Linq;
using MMCCCore.Core.Model.Mod;
using Tomlyn;
using Tomlyn.Model;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Core.Wrapper
{
    public class ModWrapper
    {
        public static ModInfo GetModInfo(string ModPath)
        {
            try
            {
                ModInfo minfo = new ModInfo();
                ZipArchive archive = new ZipArchive(new FileStream(ModPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                var entries = archive.Entries.ToList();
                var filelist = entries.Select(i => i.FullName);
                if (filelist.Contains("mcmod.info"))
                {
                    var modinfofile = entries.Find(i => i.FullName == "mcmod.info");
                    if (modinfofile == null) return null;
                    string ModInfoJson = new StreamReader(modinfofile.Open()).ReadToEnd();
                    var info = JsonConvert.DeserializeObject<ForgeOldVersionMCModFileModel>(ModInfoJson);
                    if (info == null) return null;
                    minfo.Data = info;
                    minfo.Type = ModTypes.Forge_Old;
                    return minfo;
                }
                else if (filelist.Contains("META-INF/mods.toml"))
                {
                    var modinfofile = entries.Find(i => i.FullName == "META-INF/mods.toml");
                    if (modinfofile == null) return null;
                    string ModInfoToml = new StreamReader(modinfofile.Open()).ReadToEnd();
                    var modinfo = ((TomlTableArray)(Toml.Parse(ModInfoToml).ToModel()["mods"]))[0];
                    ForgeNewVersionMCModProfileModel model = new ForgeNewVersionMCModProfileModel();
                    model.Authors = modinfo.ContainsKey("authors") ? modinfo["authors"].ToString() : string.Empty;
                    model.ModId = modinfo.ContainsKey("modId") ? modinfo["modId"].ToString() : string.Empty;
                    model.Version = modinfo.ContainsKey("version") ? modinfo["version"].ToString() : string.Empty;
                    model.Description = modinfo.ContainsKey("description") ? modinfo["description"].ToString() : string.Empty;
                    model.Name = modinfo.ContainsKey("displayName") ? modinfo["displayName"].ToString() : string.Empty;
                    minfo.Data = model;
                    minfo.Type = ModTypes.Forge_Old;
                    return minfo;
                }
                else if (filelist.Contains("fabric.mod.json"))
                {
                    var modinfofile = entries.Find(i => i.FullName == "fabric.mod.json");
                    if (modinfofile == null) return null;
                    string ModInfoJson = new StreamReader(modinfofile.Open()).ReadToEnd();
                    var info = JsonConvert.DeserializeObject<FabricVersionMCModFileModel>(ModInfoJson);
                    if (info == null) return null;
                    minfo.Data = info;
                    minfo.Type = ModTypes.Forge_Old;
                    return minfo;
                }
            }
            catch (Exception) { return null; }
            return null;
        }
    }
}