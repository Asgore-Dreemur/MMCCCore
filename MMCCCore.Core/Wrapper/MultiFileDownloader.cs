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
        public List<DownloadResultModel> ErrorFiles { get; set; } = new List<DownloadResultModel>();
        private List<DownloadResultModel> DownloadedFile = new List<DownloadResultModel>();
        private bool shouldStop { get; set; } = false;
        private static object locker = new object();

        public MultiFileDownloader(Stack<DownloadTaskInfo> DownloadStack, int ThreadCount = 32)
        {
            if (DownloadStack == null) throw new ArgumentNullException("DownloadStack");
            this.DownloadStack = DownloadStack;
            this.AllFileCount = DownloadStack.Count;
            if (ThreadCount <= 0) throw new Exception("线程数量不可是负数或0");
            this.ThreadCount = ThreadCount;
        }

        public void StopDownload()
        {
            shouldStop = true;
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
                    lock (locker) OnProgressChanged(result);
                    DownloadedFile.Add(result);
                    if (result.Result == DownloadResult.Error)
                    {
                        ErrorFiles.Add(result);
                        return;
                    }
                }
            }
            catch (Exception) { }
        }

        private void OnProgressChanged(DownloadResultModel result)
        {
            if (result.Result == DownloadResult.Error) StopDownload();
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

        public DownloadResultModel WaitDownloadComplete()
        {
            bool isCompleted = true;
            while (true)
            {
                isCompleted = true;
                foreach (var item in ThreadQueue)
                {
                    if (shouldStop) return ErrorFiles.Last();
                    if (item.IsAlive) isCompleted = false;
                    break;
                }
                if (isCompleted) return new DownloadResultModel { Result = DownloadResult.Success, ErrorException = null, DownloadInfo = null };
            }
        }
    }
}
