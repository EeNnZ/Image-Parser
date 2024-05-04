using ParserCore.Models.Options;
using ParserCore.Models.Parsers;
using ParserCore.Models.Parsers.ConcreteParsers;

namespace ParserCore.Models.Websites.ConcreteWebsites;

public class Wallhaven : IWebsite
{
    public IParser Parser { get; }
    public string  Name   => nameof(Wallhaven);
    public string  Url    => "wallhaven.cc";

    public Wallhaven(IParserOptions options)
    {
        Parser = new WallhavenParser(options);
    }
}