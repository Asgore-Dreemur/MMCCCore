using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using MMCCCore.Core.Wrapper;
using System.Security.Cryptography;
using MMCCCore.Core.Model.Core;
using MMCCCore.Core.Model.Wrapper;
using System.Threading;
using MMCCCore.Core.Model.MinecraftFiles;
using MMCCCore.Core.Module.APIManager;

namespace MMCCCore.Core.Module.Minecraft
{
    public class MCAssets : InstallerModel
    {
        private MultiFileDownloader downloader;
        public MinecraftFilesDownloadInfo DownloadAssets(string AssetIndexJson, string GameDir, bool isSkipDownloadedFile, int MaxThreadCount = 128)
        {
            try
            {
                if (DownloadAPIManager.Current == null) throw new Exception("未知的下载源");
                Stack<DownloadTaskInfo> DownloadStack = new Stack<DownloadTaskInfo>();
                JObject AssetIndexInfo = JObject.Parse(AssetIndexJson);
                Dictionary<string, JObject> AssetsDict = AssetIndexInfo["objects"].ToObject<Dictionary<string, JObject>>();
                foreach (JObject AssetInfo in AssetsDict.Values)
                {
                    string AssetPath = Path.Combine(GameDir, "assets\\objects", AssetInfo["hash"].ToString().Substring(0, 2));
                    OtherTools.CreateDir(AssetPath);
                    AssetPath = Path.Combine(AssetPath, AssetInfo["hash"].ToString());
                    string DownloadRoot = DownloadAPIManager.Current.Assets.TrimEnd('/');
                    DownloadStack.Push(new DownloadTaskInfo { DownloadUrl = $"{DownloadRoot}/{AssetInfo["hash"].ToString().Substring(0, 2)}/{AssetInfo["hash"].ToString()}",
                        DestPath = AssetPath,
                        MaxTryCount = 4,
                        Sha1 = AssetInfo["hash"].ToString(),
                        Sha1Vaildate = true,
                        isSkipDownloadedFile = isSkipDownloadedFile
                    });
                }
                if(DownloadStack.Count == 0)
                {
                    return new MinecraftFilesDownloadInfo
                    {
                        DownloadResult = MinecraftFilesDownloadResult.Success
                    };
                }
                downloader = new MultiFileDownloader(DownloadStack, MaxThreadCount);
                downloader.ProgressChanged += Downloader_ProgressChanged;
                downloader.StartDownload();
                downloader.WaitDownloadComplete();
                return new MinecraftFilesDownloadInfo
                {
                    DownloadResult = MinecraftFilesDownloadResult.Success
                };
            }catch(Exception e)
            {
                return new MinecraftFilesDownloadInfo { DownloadResult = MinecraftFilesDownloadResult.Error, ErrorException = e };
            }
        }

        private void Downloader_ProgressChanged(object sender, (int, int, DownloadResultModel) e)
        {
            if (e.Item3.Result == DownloadResult.Error)
            {
                downloader.StopDownload();
                throw new Exception($"下载{e.Item3.DownloadInfo.DownloadUrl}时出现错误:{e.Item3.ErrorException.Message}");
            }
            double DownloadedProgress = (double)Math.Round((decimal)e.Item1 / e.Item2, 2);
            OnProgressChanged(DownloadedProgress, null);
        }
    }
}
