using Microsoft.EntityFrameworkCore;
using System.Web;
using WebCrawler.Models;
using WebCrawler.Pages;

namespace WebCrawler.API
{
    public static class Crawle
    {
        public struct StartCrawleReuslt
        {
            public int ID { get; set; }

            public string Status { get; set; }
            public string ErrorMessage { get; set; }
        }

        private static string ValidateUrl(HttpContext context)
        { 
            if (context.Request.Method.ToLower() == "get" || !context.Request.Form.ContainsKey("url"))
            {
                context.Response.Redirect("/");
                return null;
            }

            var url = context.Request.Form["url"];

            return HttpUtility.UrlDecode(url);
        }

        public static Task StartCrawle(HttpContext context)
        {
            var connectionstring = Work.Connection;

            var optionsBuilder = new DbContextOptionsBuilder<CrawleContext>();
            optionsBuilder.UseSqlServer(connectionstring);


            CrawleContext DBContext = new CrawleContext(optionsBuilder.Options);
            try
            {
                Result result = null;
                StartCrawleReuslt startCrawleReuslt = new StartCrawleReuslt();

                if (context.Request.Form.Files.Count > 0)
                {
                    var newFileName = Guid.NewGuid().ToString();
                    var newPath = $"Inputs\\{newFileName}.txt";

                    using (var fileStream = File.Create(newPath))
                    {
                        fileStream.Position = 0;
                        context.Request.Form.Files[0].OpenReadStream().CopyTo(fileStream);
                    }

                    result = new Result()
                    { Done = false, Input = newPath, IsInputFile = true, PathToResult = "", TotalInvalidExternals = 0, TotalInvalidPages = 0, TotalPages = 0 };
                    startCrawleReuslt.Status = "Ok";
                }
                else
                {
                    var url = ValidateUrl(context);
                    if (url == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return Task.CompletedTask;
                    }

                    Uri _;
                    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                    {
                        startCrawleReuslt.ID = 0;
                        startCrawleReuslt.Status = "Error";
                        startCrawleReuslt.ErrorMessage = "Invalid url";
                    }
                    else
                    {
                        result = new Result()
                        { Done = false, Input = url, IsInputFile = false, PathToResult = "", TotalInvalidExternals = 0, TotalInvalidPages = 0, TotalPages = 0 };
                        startCrawleReuslt.Status = "Ok";
                    }
                }

                startCrawleReuslt.ID = 0;
                if (startCrawleReuslt.Status == "Ok")
                {
                    DBContext.Results.Add(result);
                    DBContext.SaveChanges();
                    startCrawleReuslt.ID = result.ID;
                    new Task(() => { Work.StartWork(result.ID); }).Start();

                }

                return context.Response.WriteAsJsonAsync(startCrawleReuslt);
            }
            catch { }
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Task.CompletedTask;
        }
    }
}
