using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using CDN.Infrastructure;
using CDN.Workers;
using DiskQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEOP.Framework.Config;
using SEOP.Framework.Infrastructure;
using static CDN.Test.Global;

namespace CDN.Test
{
    [TestClass]
    public class FileEnqueuerTest
    {
        [TestMethod]
        public void enque_files_for_pending_download()
        {
            var queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueueTest"));
            using (var session = queue.OpenSession())
            {
                while (session.Dequeue() != null) ;
                session.Flush();
            }

            //start file server
            IWorker fileServer = new FileServer(_fileServer_Port, _fileStorePath, queue);
            fileServer.Start();

            //clean queue
           
            //start worker
            IWorker fileEnqueuer = new FileEnqueuer(_fileStorePath,
                   _fileEnqueuer_SyncApi, _fileEnqueuer_Interval, queue);
            fileEnqueuer.Start();
            //wait work complete
            Thread.Sleep(3000);
            fileEnqueuer.Stop();

            //check if files in queue
            HttpClient client = new HttpClient();
            var get = client.GetAsync(_fileEnqueuer_SyncApi).Result;
            get.EnsureSuccessStatusCode();
            var result = get.Content.ReadAsStringAsync().Result.ToDynamicObject();
            if (result.res_code == 0)
            {
                using (var session = queue.OpenSession())
                    Assert.AreEqual((result.Result as IList<dynamic>).Count, queue.EstimatedCountOfItemsInQueue);
            }
        }
    }
}