using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Model.Wrapper;
using System.Net;
using System.IO;

namespace MMCCCore.Core.Wrapper
{
    public class FileDownloader : FileDownloadProgressModel
    {
        private DownloadTaskInfo DownloadInfo;
        public FileDownloader(DownloadTaskInfo DownloadInfo) => this.DownloadInfo = DownloadInfo;

        public DownloadResultModel StartDownload()
        {
            DownloadInfo.DestPath = OtherTools.FormatPath(DownloadInfo.DestPath);
            if (DownloadInfo.isSkipDownloadedFile)
            {
                var result = OtherTools.VaildateSha1(DownloadInfo.DestPath, DownloadInfo.Sha1);
                if (result.isVaildated) return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Skipped };
            }
            int ErrorCount = 0;
            Exception exception = null;
            while (ErrorCount <= DownloadInfo.MaxTryCount)
            {
                try
                {
                    HttpWebRequest request = HttpWebRequest.Create(DownloadInfo.DownloadUrl) as HttpWebRequest;
                    request.Referer = DownloadInfo.DownloadUrl;
                    request.Method = "GET";
                    request.UserAgent = "MMCCCore.Core 1.0/HttpWrapper";
                    request.AllowAutoRedirect = false;
                    request.ContentType = "application/octet-stream";
                    request.Timeout = 10 * 1000;
                    request.AllowAutoRedirect = true;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    WebResponse wb = response;
                    int FileSize = (int)wb.ContentLength;
                    int Downloaded = 0;
                    using (Stream _stream = wb.GetResponseStream())
                    {
                        byte[] buffer = new byte[1024 * 50];
                        using (Stream threadfile = new FileStream(DownloadInfo.DestPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            int offset = -1;
                            while ((offset = _stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                threadfile.Write(buffer, 0, offset);
                                Downloaded += offset;
                                OnDownloadProgressChanged((double)Math.Round((decimal)Downloaded / FileSize, 2));
                            }
                            threadfile.Close();
                            _stream.Close();
                        }
                    }
                    
                    return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Success };
                }
                catch (Exception e)
                {
                    ++ErrorCount;
                    exception = e;
                }
            }
            Console.WriteLine(exception.Message);
            return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Error, ErrorException = exception };
        }

        public static DownloadResultModel StartDownload(DownloadTaskInfo DownloadInfo)
        {
            DownloadInfo.DestPath = OtherTools.FormatPath(DownloadInfo.DestPath);
            DownloadResultModel FileDownloadResult = new DownloadResultModel() { DownloadInfo = DownloadInfo };
            if (DownloadInfo.isSkipDownloadedFile && !string.IsNullOrWhiteSpace(DownloadInfo.Sha1))
            {
                var result = OtherTools.VaildateSha1(DownloadInfo.DestPath, DownloadInfo.Sha1);
                if (result.isVaildated)
                {
                    FileDownloadResult.Result = DownloadResult.Skipped;
                    return FileDownloadResult;
                }
            }
            int ErrorCount = 0;
            Exception exception = null;
            while (ErrorCount < DownloadInfo.MaxTryCount)
            {
                try
                {
                    HttpWebRequest request = HttpWebRequest.Create(DownloadInfo.DownloadUrl) as HttpWebRequest;
                    request.Referer = DownloadInfo.DownloadUrl;
                    request.Method = "GET";
                    request.UserAgent = "MMCCCore.Core 1.0/HttpWrapper";
                    request.AllowAutoRedirect = false;
                    request.ContentType = "application/octet-stream";
                    request.Timeout = 10 * 1000;
                    request.AllowAutoRedirect = true;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    WebResponse wb = response;
                    int FileSize = (int)wb.ContentLength;
                    int Downloaded = 0;
                    using (Stream _stream = wb.GetResponseStream())
                    {
                        byte[] buffer = new byte[1024 * 50];
                        using (Stream threadfile = new FileStream(DownloadInfo.DestPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            int offset = -1;
                            while ((offset = _stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                threadfile.Write(buffer, 0, offset);
                                Downloaded += offset;
                            }
                            threadfile.Close();
                            _stream.Close();
                        }
                    }
                    if (DownloadInfo.Sha1Vaildate && !string.IsNullOrWhiteSpace(DownloadInfo.Sha1))
                    {
                        var result = OtherTools.VaildateSha1(DownloadInfo.DestPath, DownloadInfo.Sha1);
                        if (!result.isSuccess)
                        {
                            FileDownloadResult.Result = DownloadResult.Error;
                            FileDownloadResult.ErrorException = result.ErrorException;
                            return FileDownloadResult;
                        }
                        if (result.isVaildated)
                        {
                            FileDownloadResult.Result = DownloadResult.Success;
                            return FileDownloadResult;
                        }
                        else if (!result.isVaildated)
                        {
                            FileDownloadResult.Result = DownloadResult.Error;
                            FileDownloadResult.ErrorException = result.ErrorException;
                            return FileDownloadResult;
                        }
                    }
                    else return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Success };
                }
                catch (Exception e)
                {
                    ++ErrorCount;
                    exception = e;
                }
            }
            return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Error, ErrorException = exception };
        }
    }

    public class FileDownloaderTaskAsync : FileDownloadProgressModel
    {
        private DownloadTaskInfo DownloadInfo;
        public FileDownloaderTaskAsync(DownloadTaskInfo DownloadInfo) => this.DownloadInfo = DownloadInfo;
        public async Task<DownloadResultModel> StartDownload()
        {
            int ErrorCount = 0;
            Exception exception = null;
            DownloadInfo.DestPath = OtherTools.FormatPath(DownloadInfo.DestPath);
            while (ErrorCount < DownloadInfo.MaxTryCount)
            {
                try
                {
                    HttpWebRequest request = HttpWebRequest.Create(DownloadInfo.DownloadUrl) as HttpWebRequest;
                    request.Referer = DownloadInfo.DownloadUrl;
                    request.Method = "GET";
                    request.UserAgent = "MMCCCore.Core 1.0/HttpWrapper";
                    request.AllowAutoRedirect = false;
                    request.ContentType = "application/octet-stream";
                    request.Timeout = 10 * 1000;
                    request.AllowAutoRedirect = true;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    WebResponse wb = response;
                    int FileSize = (int)wb.ContentLength;
                    int Downloaded = 0;
                    using (Stream _stream = wb.GetResponseStream())
                    {
                        byte[] buffer = new byte[1024 * 50];
                        using (Stream threadfile = new FileStream(DownloadInfo.DestPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            int offset = -1;
                            while ((offset = _stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                threadfile.Write(buffer, 0, offset);
                                Downloaded += offset;
                                OnDownloadProgressChanged((double)Math.Round((decimal)Downloaded / FileSize, 2));
                            }
                            threadfile.Close();
                            _stream.Close();
                        }
                    }
                    return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Success };
                }
                catch (Exception e)
                {
                    ++ErrorCount;
                    exception = e;
                }
            }
            return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Error, ErrorException = exception };
        }

        public static async Task<DownloadResultModel> StartDownload(DownloadTaskInfo DownloadInfo)
        {
            int ErrorCount = 0;
            Exception exception = null;
            DownloadInfo.DestPath = OtherTools.FormatPath(DownloadInfo.DestPath);
            while (ErrorCount < DownloadInfo.MaxTryCount)
            {
                try
                {
                    HttpWebRequest request = HttpWebRequest.Create(DownloadInfo.DownloadUrl) as HttpWebRequest;
                    request.Referer = DownloadInfo.DownloadUrl;
                    request.Method = "GET";
                    request.UserAgent = "MMCCCore.Core 1.0/HttpWrapper";
                    request.AllowAutoRedirect = false;
                    request.ContentType = "application/octet-stream";
                    request.Timeout = 10 * 1000;
                    request.AllowAutoRedirect = true;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    WebResponse wb = response;
                    int FileSize = (int)wb.ContentLength;
                    int Downloaded = 0;
                    using (Stream _stream = wb.GetResponseStream())
                    {
                        byte[] buffer = new byte[1024 * 50];
                        using (Stream threadfile = new FileStream(DownloadInfo.DestPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            int offset = -1;
                            while ((offset = _stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                threadfile.Write(buffer, 0, offset);
                                Downloaded += offset;
                            }
                            threadfile.Close();
                            _stream.Close();
                        }
                    }
                    return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Success };
                }
                catch (Exception e)
                {
                    ++ErrorCount;
                    exception = e;
                }
            }
            return new DownloadResultModel { DownloadInfo = DownloadInfo, Result = DownloadResult.Error, ErrorException = exception };
        }
    }
}
