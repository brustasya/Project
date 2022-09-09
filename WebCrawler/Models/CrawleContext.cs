using Microsoft.EntityFrameworkCore;

namespace WebCrawler.Models
{
    public class CrawleContext : DbContext
    {
        public DbSet<Result> Results { get; set; }
        public CrawleContext(DbContextOptions<CrawleContext> options)
            : base(options)
        {
            Database.EnsureCreated();

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Result>().ToTable("Results");
        }
    }
}
