using Leaf.xNet;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebCrawler.Models;

namespace WebCrawler.Pages
{
    public static class Work
    {
        public const string Connection = "Server=(localdb)\\mssqllocaldb;Database=WebCrawlerDB;Trusted_Connection=True;";
        private static HashSet<string> byPassExt;
        static Work()
        {
            byPassExt = new HashSet<string>()
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".css",
                ".js",
                ".txt",
                ".woff2",
                "tel",
                "mailto"
            };
        }
        public static void StartWork(int id)
        {
            var connectionstring = Work.Connection;

            var optionsBuilder = new DbContextOptionsBuilder<CrawleContext>();
            optionsBuilder.UseSqlServer(connectionstring);


            CrawleContext DBContext = new CrawleContext(optionsBuilder.Options);


            var inp = DBContext.Results.First(x => x.ID == id);
            List<string> url;
            if (inp.IsInputFile)
            {
                url = File.ReadAllLines(inp.Input).ToList();
            }
            else
            {
                url = new List<string>();
                url.Add(inp.Input);
            }

            var client = new HttpClient();

            List<string> result = new List<string>();
            result.Add("########################");

            int _totalPages = 0;
            int _totalExternal = 0;
            int _totalErrors = 0;

            foreach (var t in url)
            {
                Queue<string> queueUrls = new Queue<string>();
                HashSet<string> usedUrls = new HashSet<string>();
                List<string> errors = new List<string>();
                result.Add("Site: " + t);
                Uri mainUri;

                if (!Uri.TryCreate(t, UriKind.Absolute, out mainUri))
                {
                    result.Add("Invalid url");
                    continue;
                }
                queueUrls.Enqueue(t);

                int totalPages = 1;
                int totalExternal = 0;
                int totalErrors = 0;
                while (queueUrls.Count > 0)
                {
                    string item = "";
                    try
                    {
                        item = queueUrls.Dequeue();
                        if (usedUrls.Contains(item))
                        {
                            continue;
                        }
                        var response = client.GetAsync(item).Result;
                        var page = response.Content.ReadAsStringAsync().Result;

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var list = page.Substrings("href=\"", "\"");
                            var list2 = page.Substrings("href='", "'");
                            var urls = new List<string>();

                            if (list != null && list.Length > 0)
                            {
                                urls.AddRange(list);
                            }
                            if (list2 != null && list2.Length > 0)
                            {
                                urls.AddRange(list2);
                            }

                            urls = urls.Distinct<string>().ToList();

                            urls.ForEach(x =>
                            {
                                if (!x.StartsWith("#") && !byPassExt.Contains(x.Split(':')[0]) && !usedUrls.Contains(x) && !byPassExt.Contains("." + x.Split(".").Last()))
                                {
                                    if (x.StartsWith("/"))
                                    {
                                        x = "https://" + mainUri.Host + x;
                                    }
                                    x = WebUtility.UrlDecode(x);
                                    Uri newUri;
                                    if (Uri.TryCreate(x, UriKind.Absolute, out newUri))
                                    {
                                        ++totalPages;

                                        usedUrls.Add(x);
                                        if (newUri.Host.Replace("www.", "") != mainUri.Host.Replace("www.", ""))
                                        {
                                            ++totalExternal;
                                            result.Add($"External url: {x}");
                                        }
                                        else
                                        {
                                            queueUrls.Enqueue(x);
                                        }
                                    }

                                }
                            });
                        }
                        else
                        {
                            ++totalErrors;
                            errors.Add($"Error [not 200 OK]: {item}");
                        }


                    }
                    catch (Exception e)
                    {
                        ++totalErrors;
                        errors.Add($"Error [{e.Message}]: {item}");
                    }
                }

                result.Add("Total urls: " + totalPages);
                result.Add("Total error urls: " + totalErrors);
                result.Add("Total External urls: " + totalExternal);

                _totalExternal += totalExternal;
                _totalErrors += totalErrors;
                _totalPages += totalPages;

                if (errors.Count > 0)
                {
                    result.Add("Errors list:");
                    result.AddRange(errors);
                }
                result.Add("########################");
                result.Add("");
            }

            var path = $"Results\\{Guid.NewGuid().ToString()}.txt";
            File.WriteAllLines(path, result);
            var fnd = DBContext.Results.First(x => x.ID == id);

            fnd.PathToResult = path;
            fnd.Done = true;
            fnd.TotalPages = _totalPages;
            fnd.TotalInvalidExternals = _totalExternal;
            fnd.TotalInvalidPages = _totalErrors;
            DBContext.SaveChanges();
        }
    }
}
