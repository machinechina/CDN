using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SEOP.Framework.Infrastructure;
namespace CDN.Infrastructure
{
    public static class UrlHelper
    {
        public static void GetHeaderFromUrl(String url, out String fileName, out DateTime lastModified)
        {
            HttpWebResponse response = null;
            fileName = "";
            lastModified = DateTime.Now;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 5000;
                response = (HttpWebResponse)request.GetResponse();
                try
                {
                    //服务方如果手动设置,应该转成GMT时间
                    //如dateTime.ToUniversalTime().ToString("R")
                    lastModified = response.LastModified;
                }
                catch { }
                try
                {
                    fileName = Uri.UnescapeDataString(response.Headers["Content-Disposition"].Replace("attachment; filename=", String.Empty).Replace("\"", String.Empty));
                }
                catch
                {
                    fileName = Path.GetFileName(url);
                }
            }
            finally
            {
                response?.Dispose();
            }
        }

        public static String GetPhysicalPathByOriginalUrl(String originalUrl, String rootPath, String fileName = "")
        {
            var uri = new Uri(originalUrl);
            var filePath = Path.Combine(rootPath, uri.Host, Path.Combine(Uri.UnescapeDataString(uri.AbsolutePath).Split('/')));
            if (String.IsNullOrEmpty(fileName))
            {
                return filePath;
            }
            else
            {
                Directory.CreateDirectory(filePath);
                return Path.Combine(filePath, fileName);
            }

        }

        public static String GetNewUrlFromOriginalUrl(String originalUrl, String rootPath)
        {
            var physicalPath = GetPhysicalPathByOriginalUrl(originalUrl, rootPath);
            var physicalFilePath = Directory.GetFiles(physicalPath).First();
            // Directory.GetFiles 返回结果的大小写随physicalPath而定,不用特意转换
            return Path.Combine("file", Path.Combine(physicalFilePath.Replace(rootPath, "")
                .Split('\\').Select(Uri.EscapeUriString).ToArray()));
        }
    }
}
