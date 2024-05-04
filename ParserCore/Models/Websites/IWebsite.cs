using ParserCore.Models.Options;
using ParserCore.Models.Parsers;

namespace ParserCore.Models.Websites;

public interface IWebsite
{
    IParser Parser { get; }
    string Name { get; }
    string Url { get; }
}