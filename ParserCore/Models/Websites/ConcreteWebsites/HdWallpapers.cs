using ParserCore.Models.Options;
using ParserCore.Models.Parsers;
using ParserCore.Models.Parsers.ConcreteParsers;

namespace ParserCore.Models.Websites.ConcreteWebsites;

public class HdWallpapers : IWebsite
{
    public IParser Parser { get; }

    public string Name => nameof(HdWallpapers);
    public string Url => "hdwallpapers.in";

    public HdWallpapers(IParserOptions options)
    {
        Parser = new HdWallpapersParser(options);
    }
}