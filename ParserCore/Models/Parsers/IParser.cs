using AngleSharp.Html.Dom;
using ParserCore.Models.Options;

namespace ParserCore.Models.Parsers;

public interface IParser
{
    IParserOptions Options { get; set; }
    IEnumerable<string> GetDirectImageLinks(IHtmlDocument document);
    IEnumerable<string> GetLinksToPagesThatContainsSingleImage(IHtmlDocument document);
    string ConstructUrl(int id);
}