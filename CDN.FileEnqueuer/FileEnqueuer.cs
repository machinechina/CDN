using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using CDN.Infrastructure;
using DiskQueue;
using SEOP.Framework.Infrastructure;
using System.Linq;
using static CDN.Infrastructure.ApplicationHelper;

namespace CDN.Workers
{
    public class FileEnqueuer : Worker
    {
        private string _fileStorePath { get; }
        private string _syncApi { get; }
        private IPersistentQueue _queue { get; }
        
        public FileEnqueuer(string fileStorePath, string syncApi, int interval, IPersistentQueue queue) : base(interval)
        {
            _fileStorePath = fileStorePath;
            _syncApi = syncApi;
            _queue = queue;
        }

        protected override void DoWork()
        {
            Info("Begin searching file list...");
            HttpClient client = new HttpClient();
            string syncUrl = null;
            //TODO:构造函数传进来,不要耦合ClickOnce发布方式
      
            var syncUrlParams = GetConfigFromDeployThenAppConfig<string>("SyncApiParam");
            try
            {
                //find file store path & get last sync time
                //pass extra params from DEPLOY URL
                var lastSyncTime = File.GetLastWriteTime(Path.Combine(_fileStorePath, "_SyncStamp")).ToString();
                syncUrl = String.Format(_syncApi, new[] { lastSyncTime }.Concat(syncUrlParams.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)).ToArray());


                //call api to get file list
                var get = client.GetAsync(syncUrl).Result;
                get.EnsureSuccessStatusCode();
                var result = get.Content.ReadAsStringAsync().Result.ToDynamicObject();

                //enqueue urls
                if (result.res_code == 0)
                {
                    var urls = result.Result as IEnumerable<dynamic>;
                    if (urls != null && urls.Count() > 0)
                        using (var session = _queue.OpenSession())
                        {
                            urls.ForEach(url =>
                            {
                                Info("New file to download:" + url);
                                session.Enqueue(Encoding.UTF8.GetBytes(url));
                            });
                            session.Flush();
                        }

                    //urls are now stored in queue,stamp to _SyncStamp to mark this api call succeeded
                    File.AppendAllText(Path.Combine(_fileStorePath, "_SyncStamp"), DateTime.Now.ToString() + "\n");
                }

                Info("End searching file list...");
            }
            catch (Exception ex)
            {
                if (syncUrl == null)
                {
                    var msg = $"Error parsing url: {_syncApi} with params {syncUrlParams}";
                    Info(msg);
                    throw new Exception(msg, ex);
                }
                else
                {
                    var msg = $"Request downloading list failed:{syncUrl}";
                    Info(msg);
                    throw new Exception(msg, ex);
                }
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}