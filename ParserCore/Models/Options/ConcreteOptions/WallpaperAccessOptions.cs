namespace ParserCore.Models.Options.ConcreteOptions;

internal class WallpaperAccessOptions : IParserOptions
{
    public string BaseUrl => "https://wallpaperaccess.com/";
    public string SearchPrefix => "search?q=";
    public string SearchQuery { get; set; } = "";
    public int ImageCount { get; set; }
    public string? Resolution { get; set; }
    #region Not used in case of this parser
    public string PagePrefix => "";
    public int StartPoint { get; set; }
    public int EndPoint { get; set; }
    #endregion
}