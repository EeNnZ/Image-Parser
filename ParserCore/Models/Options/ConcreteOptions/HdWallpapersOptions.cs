namespace ParserCore.Models.Options.ConcreteOptions;
internal class HdWallpapersOptions : IParserOptions
{
    public string BaseUrl => "https://www.hdwallpapers.in/";
    public string PagePrefix => $"{SearchPrefix}page/";
    public string SearchPrefix => "search/";
    public int StartPoint { get; set; }
    public int EndPoint { get; set; }
    public int ImageCount { get; set; }
    public string SearchQuery { get; set; } = "";
    public string? Resolution { get; set; }
}