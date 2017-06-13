using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CDN.Infrastructure;
using CDN.Workers;
using DiskQueue;
using static CDN.Infrastructure.ApplicationHelper;

namespace CDN.ConsoleApp
{
    internal class Program
    {
        private static Mutex mutex = new Mutex(true, "{2d6af9a7-3c13-4ea4-84f9-7229c7d426c4}");

        private static void Main(string[] args)
        {
            List<IWorker> wokers = new List<IWorker>();
            PersistentQueue queue = null;
            var exitForUpdating = false;

            try
            {
                Info($"CDN Starting... Version : {ApplicationHelper.Version}");
                ApplicationHelper.CheckSingleRunning(mutex);
                ApplicationHelper.InitDeployQueryString("SyncApiParam");

                #region Get Configs From url or app.config

                string _fileStorePath = GetConfigFromDeployThenAppConfig<string>("FileStorePath");

                Boolean _fileServer_Enabled = GetConfigFromDeployThenAppConfig<Boolean>("FileServer_Enabled");

                int _fileServer_Port = GetConfigFromDeployThenAppConfig<int>("FileServer_Port");

                Boolean _fileEnqueuer_Enabled = GetConfigFromDeployThenAppConfig<Boolean>("FileEnqueuer_Enabled");

                Int32 _fileEnqueuer_Interval = GetConfigFromDeployThenAppConfig<Int32>("FileEnqueuer_Interval");

                string _fileEnqueuer_SyncApi = GetConfigFromDeployThenAppConfig<string>("FileEnqueuer_SyncApi");

                Boolean _filePuller_Enabled = GetConfigFromDeployThenAppConfig<Boolean>("FilePuller_Enabled");

                Int32 _filePuller_DownloadTimeout = GetConfigFromDeployThenAppConfig<Int32>("FilePuller_DownloadTimeout");

                Int32 _filePuller_Interval = GetConfigFromDeployThenAppConfig<Int32>("FilePuller_Interval");

                Int32 _filePuller_RetryTimes = GetConfigFromDeployThenAppConfig<Int32>("FilePuller_RetryTimes");

                Int32 _filePuller_DownloadThreadCount =
               GetConfigFromDeployThenAppConfig<Int32>("FilePuller_DownloadThreadCount");

                Int32 _updateInterval = GetConfigFromDeployThenAppConfig<Int32>("UpdateInterval");

                #endregion Get Configs From url or app.config

                queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueue"));

                if (_fileEnqueuer_Enabled)
                {
                    IWorker fileEnqueuer = new FileEnqueuer(_fileStorePath,
                        _fileEnqueuer_SyncApi, _fileEnqueuer_Interval, queue);
                    fileEnqueuer.Start();
                    wokers.Add(fileEnqueuer);
                    Info("FileEnqueuer Started");
                }

                if (_filePuller_Enabled)
                {
                    for (int i = 0; i < _filePuller_DownloadThreadCount; i++)
                    {
                        IWorker filePuller = new FilePuller(_fileStorePath,
                         _filePuller_Interval, _filePuller_DownloadTimeout, 
                         _filePuller_RetryTimes, queue);
                        filePuller.Start();
                        wokers.Add(filePuller);
                        Info($"FilePuller {i} Started");
                    }
                }

                if (_fileServer_Enabled)
                {
                    IWorker fileServer = new FileServer(_fileServer_Port, _fileStorePath, queue);
                    fileServer.Start();
                    wokers.Add(fileServer);

                    Info($"FileServer Started at port {_fileServer_Port}");
                }

                IWorker updateWorker = new UpdateCheckingWorker(_updateInterval);
                updateWorker.Start();
                updateWorker.Wait();
                Info("Restart for updating...");
                exitForUpdating = true;
            }
            catch (Exception ex)
            {
                Info(ex.Message);
                Console.ReadLine();
            }
            finally
            {
                //Clean
                Parallel.ForEach(wokers, w => w.Stop());
                queue?.Dispose();
                mutex.Close();

                if (exitForUpdating)
                {
                    Application.Restart();
                }
            }
        }

    }
}