using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.Wrapper;
using System.Threading;
using System.IO;

namespace MMCCCore.Wrapper
{
    public class MultiFileDownloader
    {
        private Stack<DownloadTaskInfo> DownloadStack;
        private int AllFileCount, ThreadCount;
        public event EventHandler<(int, int, DownloadResultModel)> ProgressChanged;
        private Queue<Thread> ThreadQueue = new Queue<Thread>();
        public bool needStop = false;
        private DownloadResultModel ErrorFile;
        private List<DownloadResultModel> DownloadedFile = new List<DownloadResultModel>();
        private static Object locker = new Object();

        public MultiFileDownloader(Stack<DownloadTaskInfo> DownloadStack, int ThreadCount = 32)
        {
            if (DownloadStack == null) throw new ArgumentNullException("DownloadStack");
            this.DownloadStack = DownloadStack;
            this.AllFileCount = DownloadStack.Count;
            this.ThreadCount = ThreadCount;
        }
        private void DownloadThread()
        {
            while (true)
            {
                DownloadTaskInfo info = null;
                lock (locker)
                {
                    if (DownloadStack.Count <= 0) return;
                    info = DownloadStack.Pop();
                }
                var result = FileDownloader.StartDownload(info);
                if (result.Result == DownloadResult.Error)
                {
                    ErrorFile = result;
                    if (needStop)
                    {
                        return;
                    }
                    continue;
                }
                if (info.Sha1Vaildate)
                {
                    var vresult = OtherTools.VaildateSha1(info.DestPath, info.Sha1);
                    if (!vresult.isSuccess)
                    {
                        result.ErrorException = new Exception("验证Sha1时出现问题", innerException:vresult.ErrorException);
                        result.Result = DownloadResult.Error;
                    }
                    else
                    {
                        if (!vresult.isVaildated)
                        {
                            File.Delete(info.DestPath);
                            result.ErrorException = new Exception($"下载{info.DestPath}时出现错误", new Exception($"sha1不匹配:给定sha1:{info.Sha1},文件sha1:{vresult.FileSha1}"));
                            result.Result = DownloadResult.Error;
                        }
                    }
                }
                if (result.Result != DownloadResult.Error) DownloadedFile.Add(result);
                lock (locker) OnProgressChanged(result);
                if (needStop)
                {
                    return;
                }
            }
        }
        private void OnProgressChanged(DownloadResultModel result)
        {
            ProgressChanged?.Invoke(this, (DownloadedFile.Count, AllFileCount, result));
        }
        public void StartDownload()
        {
            for(int i = 0; i < ThreadCount; i++)
            {
                Thread thread = new Thread(DownloadThread);
                ThreadQueue.Enqueue(thread);
                thread.Start();
            }
        }
        public void WaitDownloadComplete()
        {
            OtherTools.WaitForAllThreadExit(ThreadQueue);
            if (needStop) throw new Exception($"下载{ErrorFile.DownloadInfo.DestPath}时出现问题", ErrorFile.ErrorException);
        }
    }
}
