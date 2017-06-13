using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SEOP.Framework.Infrastructure;
namespace CDN.Infrastructure
{
    public static class UrlHelper
    {
        public static void GetHeaderFromUrl(string url, out string fileName, out DateTime lastModified)
        {
            HttpWebResponse response = null;
            fileName = "";
            lastModified = DateTime.Now;
            try
            {
                var request = ( HttpWebRequest )WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 5000;
                //有些server对于没有UA的请求会500(多数是他们验证UA直接抛异常了)
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.3; Win64; x64) " +
                    "AppleWebKit/537.36 (KHTML, like Gecko) " +
                    "Chrome/56.0.2924.87 Safari/537.36";

                response = ( HttpWebResponse )request.GetResponse();
                try
                {
                    //服务方如果手动设置,应该转成GMT时间
                    //如dateTime.ToUniversalTime().ToString("R")
                    lastModified = response.LastModified;
                }
                catch { }
                try
                {
                    //从response header里面尝试寻找文件名
                    //一般有attachment和inline两种形式
                    fileName =
                        Uri.UnescapeDataString(response
                        .Headers["Content-Disposition"]
                        //.Replace("attachment; filename=", String.Empty)
                        //.Replace("inline; filename=", String.Empty)
                        .Split(new string[] { "filename=" },
                             StringSplitOptions.RemoveEmptyEntries)[1]
                        .Replace("\"", String.Empty));
                }
                catch
                {
                    //如果response header找不到就从链接尝试获取
                    fileName = Path.GetFileName(url);
                }
            }
            finally
            {
                response?.Dispose();
            }
        }

        public static string GetPhysicalPathByOriginalUrl(string originalUrl, string rootPath, string fileName = "")
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

        public static string GetNewUrlFromOriginalUrl(string originalUrl, string rootPath)
        {
            var physicalPath = GetPhysicalPathByOriginalUrl(originalUrl, rootPath);
            var physicalFilePath = Directory.GetFiles(physicalPath).First();
            // Directory.GetFiles 返回结果的大小写随physicalPath而定,不用特意转换
            return Path.Combine("file", Path.Combine(physicalFilePath.Replace(rootPath, "")
                .Split('\\').Select(Uri.EscapeUriString).ToArray()));
        }
    }
}
