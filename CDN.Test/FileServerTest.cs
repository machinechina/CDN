using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using CDN.Infrastructure;
using CDN.Workers;
using DiskQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEOP.Framework.Config;
using static CDN.Test.Global;
namespace CDN.Test
{
    [TestClass]
    public class FileServerTest
    {

        [TestMethod]
        public void Start_file_server_for_10_minute()
        {
            var queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueueTest"));
            using (var session = queue.OpenSession())
            {
                while (session.Dequeue() != null) ;
                session.Flush();
            }

            IWorker fileServer = new FileServer(_fileServer_Port, _fileStorePath, queue);
            fileServer.Start();

            IWorker filePuller = new FilePuller(_fileStorePath,
                     _filePuller_Interval, _filePuller_DownloadTimeout, _filePuller_RetryTimes, queue);
            filePuller.Start();

            Thread.Sleep(600000);
        }


    }
}
