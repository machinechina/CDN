﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Services.Description;
using CDN.Infrastructure;
using DiskQueue;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using static CDN.Infrastructure.UrlHelper;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
namespace CDN.Workers
{
    internal class Startup
    {
        public static string _fileStorePath { get; set; }

        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

            //redirect server
            app.Map("/redirect", subApp => subApp.Use<RedirectMiddleware>());

            //file server
            var physicalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _fileStorePath);
            Directory.CreateDirectory(physicalPath);
            app.UseStaticFiles(new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
                RequestPath = new PathString("/file"),
                FileSystem = new PhysicalFileSystem(physicalPath),
                OnPrepareResponse = (context) =>
                {
                    context.OwinContext.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,HEAD,OPTIONS";
                    context.OwinContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
                }
            });

            //api
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{action}",
                defaults: new { controller = "FileServerApi" }
            );
            config.EnableCors(new EnableCorsAttribute("*", "*", "GET, POST, OPTIONS, PUT, DELETE"));

            app.UseWebApi(config);
        }

    }

    internal class RedirectMiddleware
    {
        public static string _fileStorePath { get; set; }
        public static IPersistentQueue _queue { get; set; }

        private readonly AppFunc next;

        public RedirectMiddleware(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            IOwinContext context = new OwinContext(env);

            var originalUrl = context.Request.Query["originalUrl"];

            try
            {
                context.Response.Redirect(GetNewUrlFromOriginalUrl(originalUrl, _fileStorePath));
            }
            catch (Exception)
            {
                context.Response.Redirect(Uri.EscapeUriString(originalUrl));

                new Task(() =>
                {
                    //Append to download queue
                    using (var session = _queue.OpenSession())
                    {
                        session.Enqueue(Encoding.UTF8.GetBytes(originalUrl));
                        session.Flush();
                    }
                }).Start();
            }

            //4.6 only
            //await Task.CompletedTask;
            await Task.FromResult(false);
        }
    }

    public class FileServerApiController : ApiController
    {
        public static string _fileStorePath { get; set; }
        public static int _port { get; set; }

        [HttpGet]
        public object GetAllFiles()
        {
            var apiResult = new
            {
                res_code = 0,
                Result = Directory.GetFiles($@"{_fileStorePath}\", "*", SearchOption.AllDirectories)
                .Select(file => file.Replace(_fileStorePath, $@"http://{Environment.MachineName}:{_port}/CDN/file").Replace(@"\", @"/"))
            };

            return Json(apiResult);
        }

        [HttpGet]
        public object GetConfig(string configKey = "")
        {
            var apiResult = new
            {
                res_code = 0,
                Result = ApplicationHelper.GetConfigFromDeployThenAppConfig<string>(configKey)
            };

            return Json(apiResult);
        }

    }
}