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
        public static String _fileStorePath = Configuration.GetAppConfig("FileStorePath");

        public static Boolean _fileServer_Enabled = Configuration.GetAppConfig<Boolean>("FileServer_Enabled");
        public static int _fileServer_Port = Configuration.GetAppConfig<int>("FileServer_Port");

        public static Boolean _fileEnqueuer_Enabled = Configuration.GetAppConfig<Boolean>("FileEnqueuer_Enabled");
        public static Int32 _fileEnqueuer_Interval = Configuration.GetAppConfig<Int32>("FileEnqueuer_Interval");
        public static String _fileEnqueuer_SyncApi = Configuration.GetAppConfig("FileEnqueuer_SyncApi");

        public static Boolean _filePuller_Enabled = Configuration.GetAppConfig<Boolean>("FilePuller_Enabled");
        public static Int32 _filePuller_RetryTimes = Configuration.GetAppConfig<Int32>("FilePuller_RetryTimes");
        public static Int32 _filePuller_DownloadThreadCount =
        Configuration.GetAppConfig<Int32>("FilePuller_DownloadThreadCount");
        public static Int32 _filePuller_DownloadTimeout = Configuration.GetAppConfig<Int32>("FilePuller_DownloadTimeout");
        public static Int32 _filePuller_Interval = Configuration.GetAppConfig<Int32>("FilePuller_Interval");
    }
}
