using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.Wrapper;
using System.Threading;

namespace MMCCCore.Core.Model.MinecraftFiles
{
    public class FilesDownloadStatusInfo
    {
        public DownloadTaskInfo TaskInfo { get; set; }
        public Stack<DownloadTaskInfo> DownloadStack { get; set; }
    }
    public class MinecraftFilesDownloadInfo
    {
        public MinecraftFilesDownloadResult DownloadResult { get; set; }
        public Exception ErrorException { get; set; }
    }
    public class HashVaildateResult
    {
        public bool isSuccess { get; set; }
        public bool isVaildated { get; set; }
        public Exception ErrorException { get; set; }
        public string FileSha1 { get; set; }
    } 
    public enum MinecraftFilesDownloadResult
    {
        Success = 0,
        Error = 1
    }
}
