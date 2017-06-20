using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEOP.Framework.Config;

namespace CDN.Test
{
    public static class Global
    {
        public static string _fileStorePath = Configuration.GetAppConfig("FileStorePath");

        public static bool _fileServer_Enabled = Configuration.GetAppConfig<bool>("FileServer_Enabled");
        public static int _fileServer_Port = Configuration.GetAppConfig<int>("FileServer_Port");

        public static bool _fileEnqueuer_Enabled = Configuration.GetAppConfig<bool>("FileEnqueuer_Enabled");
        public static Int32 _fileEnqueuer_Interval = Configuration.GetAppConfig<Int32>("FileEnqueuer_Interval");
        public static string _fileEnqueuer_SyncApi = Configuration.GetAppConfig("FileEnqueuer_SyncApi");

        public static bool _filePuller_Enabled = Configuration.GetAppConfig<bool>("FilePuller_Enabled");
        public static Int32 _filePuller_RetryTimes = Configuration.GetAppConfig<Int32>("FilePuller_RetryTimes");
        public static Int32 _filePuller_DownloadThreadCount =
        Configuration.GetAppConfig<Int32>("FilePuller_DownloadThreadCount");
        public static Int32 _filePuller_DownloadTimeout = Configuration.GetAppConfig<Int32>("FilePuller_DownloadTimeout");
        public static Int32 _filePuller_Interval = Configuration.GetAppConfig<Int32>("FilePuller_Interval");
    }
}
