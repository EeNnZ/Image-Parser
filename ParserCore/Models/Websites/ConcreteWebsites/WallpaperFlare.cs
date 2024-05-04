using ParserCore.Models.Options;
using ParserCore.Models.Parsers;
using ParserCore.Models.Parsers.ConcreteParsers;

namespace ParserCore.Models.Websites.ConcreteWebsites;

public class WallpaperFlare : IWebsite
{
    public IParser Parser { get; }
    public string  Name   => nameof(WallpaperFlare);
    public string  Url    => "wallpaperflare.com";

    public WallpaperFlare(IParserOptions options)
    {
        Parser = new WallpaperFlareParser(options);
    }
}