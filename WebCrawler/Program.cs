using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;
using WebCrawler.Pages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<CrawleContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<CrawleContext>();
    context.Database.EnsureCreated();
    context.Results.ForEachAsync(x => x.Done = true).Wait();
    context.SaveChanges();
    //DbInitializer.Initialize(context);
}

var sc = app.Services.CreateScope();
var se = sc.ServiceProvider;

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


Directory.CreateDirectory("Inputs");
Directory.CreateDirectory("Results");

app.UseEndpoints(endpoints =>
{
    // Configure another endpoint, no authorization requirements.
    endpoints.MapPost("/api/crawle", context =>
    {
        return WebCrawler.API.Crawle.StartCrawle(context);
    });

    endpoints.MapGet("/api/downloadFile", context =>
    {
        return WebCrawler.API.DownloadFile.StartDownload(context);
    });

    endpoints.MapGet("/api/downloadInputFile", context =>
    {
        return WebCrawler.API.DownloadFile.DownloadInputFile(context);
    });

    endpoints.MapGet("/api/checkReady", context =>
    {
        return WebCrawler.API.DownloadFile.CheckReady(context);
    });
});

app.MapRazorPages();

app.Run();
