using System.Runtime.Versioning;
using AngleSharp.Html.Dom;
using ParserCore.Helpers;
using ParserCore.Models.Options;

namespace ParserCore.Models.Parsers.ConcreteParsers;
[SupportedOSPlatform("windows")]
public class HdWallpapersParser : IParser
{
    //TODO: Floating error - sometimes this parser returns zero results when tries to download any html
    public IParserOptions Options { get; set; }

    public HdWallpapersParser(IParserOptions options)
    {
        Options = options;
    }
    public IEnumerable<string> GetLinksToPagesThatContainsSingleImage(IHtmlDocument doc)
    {
        var elements = doc.QuerySelectorAll("a")
                          .Where(x => x.InnerHtml.Contains("img src")
                                           && x.HasAttribute("href"));
        var links = from elem in elements
                    from attr in elem.Attributes.Where(attr => attr.Name == "href")
                    let baseUrl = Options.BaseUrl
                    select baseUrl + attr.Value[1..];
        //select string.Concat(baseUrl, attr.Value);
        return links;
    }
    public IEnumerable<string> GetDirectImageLinks(IHtmlDocument doc)
    {
        //< a href = "download/yellow_green_autumn_trees_forest_road_4k_hd_nature-1280x720.jpg" 
        //    title = "HD 1280 x 720 Wallpaper" > 1280 x 720 </ a >
        var elements = doc.QuerySelectorAll("a")
                          .Where(x => x.HasAttribute("href"));
        var sources = from elem in elements
                      from attr in elem.Attributes.Where(attr => attr.Name == "href")
                      where attr.Value.Contains("download") &&
                            attr.Value.Contains(Options.Resolution ?? DisplayResolution.Resolution)
                      select Options.BaseUrl + attr.Value;

        return sources;
    }
    public string ConstructUrl(int id)
    {
        string query = Options.SearchQuery.Replace(" ", "%20").Trim().ToLowerInvariant();
        Options.SearchQuery = query;
        return $"{Options.BaseUrl}{Options.SearchPrefix}{Options.PagePrefix}{id}?q={query}";
    }
}