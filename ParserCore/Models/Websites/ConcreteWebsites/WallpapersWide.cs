using ParserCore.Models.Parsers;
using ParserCore.Models.Parsers.ConcreteParsers;
using ParserCore.Models.Options;

namespace ParserCore.Models.Websites.ConcreteWebsites;

public class WallpapersWide : IWebsite
{
    public IParser Parser { get; }
    public string Name => nameof(WallpapersWide);
    public string Url => "wallpaperswide.com";

    public WallpapersWide(IParserOptions options)
    {
        Parser = new WallpapersWideParser(options);
    }
}