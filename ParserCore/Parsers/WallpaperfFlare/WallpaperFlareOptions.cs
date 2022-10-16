using ParserCore.Interfaces;

namespace ParserCore.Parsers.WallpaperfFlare
{
    public class WallpaperFlareOptions : IParserOptions
    {
        public string BaseUrl => "https://www.wallpaperflare.com/";

        public string PagePrefix => $"{SearchPrefix}{SearchQuery}&page=";

        public string SearchPrefix => "search?wallpaper=";

        public int StartPoint { get; set; }
        public int EndPoint { get; set; }
        public string SearchQuery { get; set; } = "";
        public string? Resolution { get; set; }
    }
}
