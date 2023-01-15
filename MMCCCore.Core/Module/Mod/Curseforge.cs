using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.Mod;
using Newtonsoft.Json;
using System.Net;

namespace MMCCCore.Core.Module.Mod
{
    public class Curseforge
    {
        private WebClient WebClient = new WebClient();
        private string APIKey;
        public Curseforge(string APIKey)
        {
            this.APIKey = APIKey;
        }
        public CurseforgeModModel SearchMods(SearchModel SearchInfo)
        {
            if (SearchInfo == null) return null;
            string SearchUrl = $"https://api.curseforge.com/v1/mods/search?gameId={SearchInfo.GameId.ToString()}";
            if (SearchInfo.SearchFilter != null) SearchUrl += $"&searchFilter={SearchInfo.SearchFilter}";
            if (SearchInfo.ClassId != -1) SearchUrl += $"&classId={SearchInfo.ClassId.ToString()}";
            if (SearchInfo.CategoryId != -1) SearchUrl += $"&categoryId={SearchInfo.CategoryId.ToString()}";
            if (SearchInfo.GameVersion != null) SearchUrl += $"&gameVersion={SearchInfo.GameVersion}";
            if (SearchInfo.ModLoaderType != -1) SearchUrl += $"&modLoaderType={SearchInfo.ModLoaderType.ToString()}";
            WebClient.Headers.Add("x-api-key", APIKey);
            string ResStr = WebClient.DownloadString(SearchUrl);
            return JsonConvert.DeserializeObject<CurseforgeModModel>(ResStr);
        }
        public CurseforgeModFilesModel GetModFiles(int ModId)
        {
            string GetUrl = $"https://api.curseforge.com/v1/mods/{ModId}/files";
            WebClient.Headers.Add("x-api-key", APIKey);
            string ResStr = WebClient.DownloadString(GetUrl);
            return JsonConvert.DeserializeObject<CurseforgeModFilesModel>(ResStr);
        }
    }
}
