using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CDN.Test.Standalone
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("File Url:");
            var url = Console.ReadLine();

            Console.WriteLine("How many times:");
            var times = int.Parse(Console.ReadLine());

            var timePerParse = Stopwatch.StartNew();
            Parallel.For(0, times, i =>
            {
                var path = Path.Combine("C:\\TestDownload", i.ToString());
                Directory.CreateDirectory(path);
                using (var webClient = new WebClient())
                    webClient.DownloadFile(url, Path.Combine(path, "FILE"));
            });
            timePerParse.Stop();
            Console.WriteLine("Finished,Total time:" + timePerParse.Elapsed.ToString());

            Console.ReadLine();
        }
    }
}
