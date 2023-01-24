using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MMCCCore.Core.Model.Mod;
using System.Net;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Core.Module.Mod
{
    public class Modrinth
    {
        private WebClient WebClient = new WebClient();
        public Modrinth() {
            WebClient.Headers.Add("User-Agent", "MMCCCore v1.0");
        }
        public ModrinthModModel SearchMods(ModrinthSearchModel SearchInfo)
        {
            string SearchUrl = $"https://mcim.z0z0r4.top/modrinth/search";
            if (SearchInfo.Query != null) SearchUrl += $"?query={SearchInfo.Query}";
            if (SearchInfo.index != null) SearchUrl += SearchUrl.Last().Equals('h') ? $"?index={SearchInfo.index}" : $"&index={SearchInfo.index}";
            if(SearchInfo.ModFilter != null || SearchInfo.ModLoaderType != null || SearchInfo.Version != null)
            {
                JArray FacetsList = new JArray();
                if (SearchInfo.ModFilter != null)FacetsList.Add(new JArray() {$"categories:{SearchInfo.ModFilter}" });
                if (SearchInfo.ModLoaderType != null) FacetsList.Add(new JArray() { $"categories:{SearchInfo.ModLoaderType}" });
                if (SearchInfo.Version != null) FacetsList.Add(new JArray() { $"versions:{SearchInfo.Version}" });
                SearchUrl += SearchUrl.Last().Equals('h') ? $"?facets={JsonConvert.SerializeObject(FacetsList)}" : $"&facets={JsonConvert.SerializeObject(FacetsList)}";
            }
            string ResStr = WebClient.DownloadString(SearchUrl);
            return JsonConvert.DeserializeObject<ModrinthModModel>(ResStr);
        }
        
        public ModrinthModVersionsModel GetModVersions(string slug)
        {
            string ResStr = WebClient.DownloadString($"https://mcim.z0z0r4.top/modrinth/project/{slug}/versions");
            return JsonConvert.DeserializeObject<ModrinthModVersionsModel>(ResStr);
        }
    }
}
