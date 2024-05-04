namespace ParserCore.Models.Options.ConcreteOptions;

internal class WallpapersWideOptions : IParserOptions
{
    //http://wallpaperswide.com/search/page/1?q=sunset
    public string BaseUrl => "http://wallpaperswide.com/";
    public string PagePrefix => "page";
    public string SearchPrefix => "search/page";
    public int StartPoint { get; set; }
    public int EndPoint { get; set; }
    public int ImageCount { get; set; }
    public string SearchQuery { get; set; } = "";
    public string? Resolution { get; set; }
}