using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CDN.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CDN.Test
{
    [TestClass]
    public class WorkerTest
    {
        [TestMethod]
        public void One_worker_can_only_start_once()
        {
            var worker = new FakeWorker();

            worker.Start();
            worker.Stop();

            for (int i = 0; i < 100; i++)
            {
                new Thread(worker.Start).Start();
            }

            for (int i = 0; i < 100; i++)
            {
                new Thread(worker.Stop).Start();
            }

            Thread.Sleep(2000);
            Assert.AreEqual(1, worker.startCounter);
            Assert.AreEqual(1, worker.stopCounter);
        }

    }

    public class FakeWorker
    {
        public int startCounter = 0;
        public int stopCounter = 0;
        public Object obj = new object();

        public bool IsRunning { get; set; }
        public void Start()
        {
            lock (this)
            {
                if (IsRunning) return;

                startCounter++;
                Thread.Sleep(10);
                IsRunning = true;

            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (!IsRunning) return;

                stopCounter++;
                IsRunning = false;
                Thread.Sleep(10);
                IsRunning = false;
            }
        }

    }
}
