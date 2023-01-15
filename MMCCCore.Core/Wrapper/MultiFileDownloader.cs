using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.Wrapper;
using System.Threading;
using System.IO;

namespace MMCCCore.Core.Wrapper
{
    public class MultiFileDownloader
    {
        private Stack<DownloadTaskInfo> DownloadStack;
        private int AllFileCount, ThreadCount;
        public event EventHandler<(int, int, DownloadResultModel)> ProgressChanged;
        private Queue<Thread> ThreadQueue = new Queue<Thread>();
        public DownloadResultModel ErrorFile;
        private List<DownloadResultModel> DownloadedFile = new List<DownloadResultModel>();
        private static object locker = new object();

        public MultiFileDownloader(Stack<DownloadTaskInfo> DownloadStack, int ThreadCount = 32)
        {
            if (DownloadStack == null) throw new ArgumentNullException("DownloadStack");
            this.DownloadStack = DownloadStack;
            this.AllFileCount = DownloadStack.Count;
            this.ThreadCount = ThreadCount;
        }

        public void StopDownload()
        {
            foreach (var item in ThreadQueue) item.Abort();
        }

        private void DownloadThread()
        {
            try
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
                    }
                    if (result.Result != DownloadResult.Error) DownloadedFile.Add(result);
                    lock (locker) OnProgressChanged(result);
                }
            }
            catch (ThreadAbortException) { }
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
        }
    }
}
