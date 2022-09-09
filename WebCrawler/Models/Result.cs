namespace WebCrawler.Models
{
    public class Result
    {
        public int ID { get; set; }
        public bool Done { get; set; }
        public bool IsInputFile { get; set; }
        public string? Input { get; set; }
        public int TotalPages { get; set; }
        public int TotalInvalidPages { get; set; }
        public int TotalInvalidExternals { get; set; }
        public string? PathToResult { get; set; }
    }
}
