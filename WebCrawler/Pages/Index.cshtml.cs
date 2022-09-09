using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

namespace WebCrawler.Pages
{
    public class IndexModel : PageModel
    { 
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public List<Result> getResults()
        {
            var connectionstring = Work.Connection;

            var optionsBuilder = new DbContextOptionsBuilder<CrawleContext>();
            optionsBuilder.UseSqlServer(connectionstring);


            CrawleContext DBContext = new CrawleContext(optionsBuilder.Options);
            var list = DBContext.Results.ToList();
            list.Reverse();
            return list;
        }
    }
}