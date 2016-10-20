using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CDN.Infrastructure;
using CDN.Workers;
using DiskQueue;
using SEOP.Framework.Config;
using SEOP.Framework.Infrastructure;
using static CDN.Infrastructure.ApplicationHelper;

namespace CDN.ConsoleApp
{
    internal class Program
    {
        private static String _fileStorePath = Configuration.GetAppConfig("FileStorePath");

        private static Boolean _fileServer_Enabled = Configuration.GetAppConfig<Boolean>("FileServer_Enabled");
        private static int _fileServer_Port = Configuration.GetAppConfig<int>("FileServer_Port");

        private static Boolean _fileEnqueuer_Enabled = Configuration.GetAppConfig<Boolean>("FileEnqueuer_Enabled");
        private static Int32 _fileEnqueuer_Interval = Configuration.GetAppConfig<Int32>("FileEnqueuer_Interval");
        private static String _fileEnqueuer_SyncApi = Configuration.GetAppConfig("FileEnqueuer_SyncApi");

        private static Boolean _filePuller_Enabled = Configuration.GetAppConfig<Boolean>("FilePuller_Enabled");
        private static Int32 _filePuller_DownloadTimeout = Configuration.GetAppConfig<Int32>("FilePuller_DownloadTimeout");
        private static Int32 _filePuller_Interval = Configuration.GetAppConfig<Int32>("FilePuller_Interval");
        private static Int32 _filePuller_RetryTimes = Configuration.GetAppConfig<Int32>("FilePuller_RetryTimes");

        private static Int32 _filePuller_DownloadThreadCount =
        Configuration.GetAppConfig<Int32>("FilePuller_DownloadThreadCount");

        private static Int32 _updateInterval = Configuration.GetAppConfig<Int32>("UpdateInterval");

        private static Mutex mutex = new Mutex(true, "{2d6af9a7-3c13-4ea4-84f9-7229c7d426c4}");

        private static void Main(string[] args)
        {
            List<IWorker> wokers = new List<IWorker>();
            var exitForUpdating = false;

            try
            {
                ApplicationHelper.CheckSingleRunning(mutex);
                ApplicationHelper.InitDeployQueryString();
                Info($"CDN Starting... Version : {ApplicationHelper.Version}");
                Info($"API_PARAMS:{GetDeployQueryString("SyncApiParam")}");

                var queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueue"));

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
                         _filePuller_Interval, _filePuller_DownloadTimeout, _filePuller_RetryTimes, queue);
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
                mutex.Close();

                if (exitForUpdating)
                {
                    Application.Restart();
                }
            }
        }
    }
}