using ParserCore.Models.Options;
using ParserCore.Models.Parsers;
using ParserCore.Models.Parsers.ConcreteParsers;

namespace ParserCore.Models.Websites.ConcreteWebsites;

public class WallpaperAccess : IWebsite
{
    public IParser Parser { get; }
    public string  Name   => nameof(WallpaperAccess);
    public string  Url    => "wallpaperaccess.com";

    public WallpaperAccess(IParserOptions options)
    {
        Parser = new WallpaperAccessParser(options);
    }
}