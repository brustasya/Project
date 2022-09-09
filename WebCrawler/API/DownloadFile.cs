using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;
using WebCrawler.Pages;

namespace WebCrawler.API
{
    public static class DownloadFile
    {
        private static Result IsOkRequest(HttpContext context, bool IsInput = false)
        {
            var connectionstring = Work.Connection;

            var optionsBuilder = new DbContextOptionsBuilder<CrawleContext>();
            optionsBuilder.UseSqlServer(connectionstring);


            CrawleContext DBContext = new CrawleContext(optionsBuilder.Options);
            try
            {
                if (context.Request.Method.ToLower() == "post" || !context.Request.QueryString.HasValue)
                {
                    context.Response.Redirect("/");
                    return null;
                }
                var id = context.Request.QueryString.Value.Split(new String[] { "id=" }, StringSplitOptions.None).Last().Split('&')[0];
                int idParsed;
                Result cr;
                if (!int.TryParse(id, out idParsed) || (cr = DBContext.Results.First(x => x.ID == idParsed)) == null || (IsInput && !cr.IsInputFile))
                {
                    context.Response.Redirect("/");
                    return null;
                }

                return cr;
            }
            catch { }
            return null;
        }

        public static Task CheckReady(HttpContext context)
        {
            try
            {
                var res = IsOkRequest(context);
                if (res == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return Task.CompletedTask;
                }
                return context.Response.WriteAsJsonAsync(res);
            }
            catch { }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Task.CompletedTask;
        }

        public static Task StartDownload(HttpContext context)
        {
            try
            {
                var res = IsOkRequest(context);
                if (res == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return Task.CompletedTask;
                }
                if (!File.Exists(res.PathToResult))
                {
                    throw new Exception();
                }
                return context.Response.SendFileAsync(res.PathToResult);
            }
            catch { }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Task.CompletedTask;
        }
        public static Task DownloadInputFile(HttpContext context)
        {
            try
            {
                var res = IsOkRequest(context, IsInput: true);
                if (res == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return Task.CompletedTask;
                }
                if (!File.Exists(res.PathToResult))
                {
                    throw new Exception();
                }
                return context.Response.SendFileAsync(res.Input);
            }
            catch { }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Task.CompletedTask;
        }
    }
}
