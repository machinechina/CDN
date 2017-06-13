using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CDN.Infrastructure;
using DiskQueue;
using Microsoft.Owin.Hosting;
using SEOP.Framework.Config;

namespace CDN.Workers
{
    public class FileServer : Worker
    {
        int _port { get; }

        public FileServer(int port, string fileStorePath, IPersistentQueue queue) : base(-1)
        {
            _port = port;
            Startup._fileStorePath = fileStorePath;
            FileServerApiController._fileStorePath = fileStorePath;
            FileServerApiController._port = port;
            RedirectMiddleware._fileStorePath = fileStorePath;
            RedirectMiddleware._queue = queue;
        }

        protected override void DoWork()
        {
            var host = $"http://+:{_port}/CDN";
            using (WebApp.Start<Startup>(host))
            {
                while (!IsCancellationRequested)
                    Thread.Sleep(1000);
            };
        }
    }
}
