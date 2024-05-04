using System.Runtime.Versioning;
using AngleSharp.Html.Dom;
using ParserCore.Helpers;
using ParserCore.Models.Options;

namespace ParserCore.Models.Parsers.ConcreteParsers;
[SupportedOSPlatform("windows")]
public class WallpapersWideParser : IParser
{
    public IParserOptions Options { get; set; }

    public WallpapersWideParser(IParserOptions options)
    {
        Options = options;
    }
    public string ConstructUrl(int pageId)
    {
        string searchQuery = Options.SearchQuery.Replace(" ", "%20").Trim().ToLowerInvariant();
        var url = $"{Options.BaseUrl}{Options.SearchPrefix}/{pageId}?q={searchQuery}";
        return url;
    }
    public IEnumerable<string> GetLinksToPagesThatContainsSingleImage(IHtmlDocument document)
    {
        var elements = document.QuerySelectorAll("div")
                               .Where(x => x.ClassName == "mini-hud"
                                           && x.OuterHtml.Contains("prevframe_show"));
        var links = from element in elements
                    from attr in element.Attributes.Where(attr => attr.Name == "onclick")
                    let link = attr.Value.Split("'").FirstOrDefault(x => x.Contains("http"))
                    select link ?? "";
        return links;
    }



    public IEnumerable<string> GetDirectImageLinks(IHtmlDocument document)
    {
        //<a target = "_self"
        //   href = "/download/wildlife_5-wallpaper-1920x1080.jpg" 
        //   title = "HD 16:9 1920 x 1080 wallpaper for FHD 1080p High Definition or Full HD displays" > 1920x1080
        //</a>
        //http://wallpaperswide.com/download/wildlife_5-wallpaper-1920x1080.jpg
        return from tag in document.QuerySelectorAll("a")
               let baseUrl = Options.BaseUrl
               let href = tag.GetAttribute("href")
               where tag != null && href != null
                                 && href.Contains("/download/")
                                 && href.Contains(Options.Resolution ?? DisplayResolution.Resolution)
               select string.Concat(baseUrl, href[1..]);
    }
}