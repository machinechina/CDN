using System;
using System.IO;
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
    public class FilePullerTest
    {
        [TestMethod]
        public void Enque_remote_files_and_download()
        {
            var queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueueTest"));
            using (var session = queue.OpenSession())
            {
                //clean queue
                while (session.Dequeue() != null) ;
                for (int i = 0; i < 2; i++)
                {
                    session.Enqueue(Encoding.UTF8.GetBytes(@"http://jx.taedu.gov.cn:83//Resource.Portal.Web/SubjectResource/DownloadById/030183bb-409f-43be-b3dd-4dd683e7dffc"));

                    session.Enqueue(Encoding.UTF8.GetBytes(@"http://jx.taedu.gov.cn:83/Storages/温秀云/2016/8/1/image/师说2_104707/jpg/师说2.jpg"));

                    session.Enqueue(Encoding.UTF8.GetBytes(@"http://jx.taedu.gov.cn:83/Storages/张华/2016/9/12/image/劝学01_110337/jpg/劝学01.jpg"));

                    session.Enqueue(Encoding.UTF8.GetBytes(@"http://jx.taedu.gov.cn:83/Storages/晁阳/2016/9/8/flash/正多边形内角和_181554/swf/正多边形内角和.swf"));

                    session.Enqueue(Encoding.UTF8.GetBytes(@"http://jx.taedu.gov.cn:83/Storages/周小广/2016/9/13/pdf/一年级英语上册-Unit1-School(2)课件-人教新起点_155027/pdf/一年级英语上册-Unit1-School(2)课件-人教新起点.pdf"));
                }

                session.Flush();
            }

            IWorker filePuller = new FilePuller(_fileStorePath,
                  _filePuller_Interval, _filePuller_DownloadTimeout, _filePuller_RetryTimes, queue);
            filePuller.Start();

            Thread.Sleep(100000);

        }

        [TestMethod]
        public void Enque_some_files_and_download()
        {
            var queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueueTest"));

            IWorker fileServer = new FileServer(_fileServer_Port, _fileStorePath, queue);
            fileServer.Start();

            using (var session = queue.OpenSession())
            {
                //clean queue
                while (session.Dequeue() != null) ;

                //20 images and 1 mp4
                foreach (var file in Directory.GetFiles(@"D:\Files\Source\", "*", SearchOption.AllDirectories))
                {
                    session.Enqueue(Encoding.UTF8.GetBytes(file.Replace(@"D:\Files\Source\", @"http://localhost:9001/file/Source/")));
                }

                session.Flush();
            }

            IWorker filePuller = new FilePuller(_fileStorePath,
                     _filePuller_Interval, _filePuller_DownloadTimeout, _filePuller_RetryTimes, queue);
            filePuller.Start();


            Thread.Sleep(100000);
        }

        [TestMethod]
        public void Enque_some_files_and_download_multiThread()
        {
            var queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueue"));

            IWorker fileServer = new FileServer(_fileServer_Port, _fileStorePath, queue);
            fileServer.Start();


            using (var session = queue.OpenSession())
            {
                //clean queue
                while (session.Dequeue() != null) ;

                //20 images and 1 mp4
                foreach (var file in Directory.GetFiles(@"D:\Files\Source", "*", SearchOption.AllDirectories))
                {
                    session.Enqueue(Encoding.UTF8.GetBytes(file.Replace(@"D:\Files\Source\", @"http://localhost:9001/file/Source/")));
                }

                session.Flush();
            }

            for (int i = 0; i < 5; i++)
            {
                new FilePuller(_fileStorePath,
                     _filePuller_Interval, _filePuller_DownloadTimeout, _filePuller_RetryTimes, queue).Start();
            }

            Thread.Sleep(100000);
        }

        [TestMethod]
        public void Enque_remote_files_and_download_multiThread()
        {
            var queue = new PersistentQueue(Path.Combine(_fileStorePath, "_FileQueue"));
            using (var session = queue.OpenSession())
            {
                //加入多个重复文件
                //当文件开始下载时就已经创建了,其他线程下载同样文件时,会检测到已下载(比较UpdateTime)而跳过
                for (int i = 0; i < 10; i++)
                {
                    //10m左右
                    session.Enqueue(Encoding.UTF8.GetBytes(@"http://jx.taedu.gov.cn:83/Resource.Portal.Web/SubjectResource/DownloadById/7923654c-189c-454f-bc0f-58dac38d2f78"));
                }

                session.Flush();
            }

            for (int i = 0; i < 5; i++)
            {
                new FilePuller(_fileStorePath,
                     _filePuller_Interval, _filePuller_DownloadTimeout, _filePuller_RetryTimes, queue).Start();
            }

            Thread.Sleep(100000);
        }

    }
}
