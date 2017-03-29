using System;
using System.IO;
using System.Text;
using System.Threading;
using CDN.Infrastructure;
using DiskQueue;
using static CDN.Infrastructure.ApplicationHelper;
using static CDN.Infrastructure.UrlHelper;

namespace CDN.Workers
{
    public class FilePuller : Worker
    {
        private String _fileStorePath { get; }
        private int _downloadTimeout { get; }
        private int _maxRetryTimes { get; }
        private IPersistentQueue _queue { get; }

        public FilePuller(String fileStorePath, int interval, int downloadTimeout, int retryTimes, IPersistentQueue queue) : base(interval)
        {
            _fileStorePath = fileStorePath;
            _downloadTimeout = downloadTimeout;
            _maxRetryTimes = retryTimes;
            _queue = queue;
        }

        protected override void DoWork()
        {
            using (var session = _queue.OpenSession())
            {
                byte[] data;
                while ((data = session.Dequeue()) != null)
                {
                    var url = Encoding.UTF8.GetString(data);
                    try
                    {
                        Info("Begin download:" + url);

                        //下载资源 比较修改日期
                        //默认是最新,如果获取不到当作最新的
                        DateTime lastModified; string fileName;
                        GetHeaderFromUrl(url, out fileName, out lastModified);

                        var filePathName = GetPhysicalPathByOriginalUrl(url, _fileStorePath, fileName);
                        if (lastModified > File.GetLastWriteTime(filePathName))
                        {
                            //同步下载直到完成
                            var _retry = _maxRetryTimes;
                            while (_retry >= 0)
                            {
                                try
                                {
                                    using ( var webClient = new TimeoutWebClient(_downloadTimeout))
                                        webClient.DownloadFile(url, filePathName);

                                    //使文件修改时间和服务器一致,以比较是否更新
                                    File.SetLastWriteTime(filePathName, lastModified);
                                }
                                catch (Exception)
                                {
                                    if (_retry == 0) throw;
                                    Thread.Sleep(5000);
                                    Info($"Retry download:({_retry})" + url);
                                    _retry--;
                                }
                                _retry = -1;
                            }

                            Info("Finish download:" + url);
                        }
                        else
                        {
                            Info("File already exists:" + url);
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        Info("Timeout downloading:" + url);
                        throw new Exception("Timeout downloading:" + url, ex);
                    }
                    catch (Exception ex)
                    {
                        Info("Error downloading:" + url);
                        throw new Exception("Error downloading:" + url, ex);
                    }
                    finally
                    {
                        session.Flush();
                    }
                }
            }
        }
    }
}