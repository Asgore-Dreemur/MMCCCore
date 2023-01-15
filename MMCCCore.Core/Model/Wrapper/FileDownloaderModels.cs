using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMCCCore.Core.Model.Wrapper
{
    public abstract class FileDownloadProgressModel
    {
        public event EventHandler<double> DownloadProgressChanged;
        protected virtual void OnDownloadProgressChanged(double status) => DownloadProgressChanged?.Invoke(this, status);
    }
    public class DownloadTaskInfo
    {
        public string DownloadUrl { get; set; }
        public string DestPath { get; set; }
        public int MaxTryCount { get; set; }
        public string Sha1 { get; set; }
        public bool Sha1Vaildate { get; set; }
        public bool isSkipDownloadedFile { get; set; }
    }
    public class DownloadResultModel
    {
        public DownloadResult Result { get; set; }
        public Exception ErrorException { get; set; }
        public DownloadTaskInfo DownloadInfo { get; set; }
    }
    public enum DownloadResult
    {
        Success = 0,
        Error = 1,
        Skipped = 2
    }
}
