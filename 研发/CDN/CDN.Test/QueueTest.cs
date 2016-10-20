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

namespace CDN.Test
{
    [TestClass]
    public class QueueTest
    {
        private readonly String _fileStorePath = Configuration.GetAppConfig("FileStorePath");

        [TestMethod]
        public void enqueue_dequeue_same_time()
        {

            var t0 = new Thread(() =>
            {
                while (true)
                {
                    using (var queue = new PersistentQueue("queue_a"))
                    using (var session = queue.OpenSession())
                    {
                        session.Enqueue(Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                        session.Flush();

                        Thread.Sleep(100);
                    }
                }
            });
            var t1 = new Thread(() =>
            {
                while (true)
                {
                    using (var queue = new PersistentQueue("queue_a"))
                    using (var session = queue.OpenSession())
                    {
                        session.Enqueue(Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                        session.Flush();

                        Thread.Sleep(100);
                    }
                }
            });
            var t2 = new Thread(() =>
            {
                while (true)
                {
                    using (var queue = new PersistentQueue("queue_a"))
                    using (var session = queue.OpenSession())
                    {
                        var data = session.Dequeue();
                        if (data == null) { Thread.Sleep(100); continue; }

                        session.Flush();
                        Thread.Sleep(100);
                    }
                }
            });
            var t3 = new Thread(() =>
            {
                while (true)
                {
                    using (var queue = new PersistentQueue("queue_a"))
                    using (var session = queue.OpenSession())
                    {
                        var data = session.Dequeue();
                        if (data == null) { Thread.Sleep(100); continue; }

                        session.Flush();
                        Thread.Sleep(100);
                    }
                }
            });
            t0.Start();
            t1.Start();
            t2.Start();
            t3.Start();
            Thread.Sleep(10000);
            //using (var queue = PersistentQueue.WaitFor("queue_a", TimeSpan.FromSeconds(30)))
            ////IPersistentQueue queue1 = new PersistentQueue("queue_a");
            //using (var session = queue.OpenSession())
            //{
            //    while (session.Dequeue() != null)
            //        session.Flush();
            //}
        }
    }
}
