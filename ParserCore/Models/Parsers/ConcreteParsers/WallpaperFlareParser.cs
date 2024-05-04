using AngleSharp.Html.Dom;
using ParserCore.Helpers;
using ParserCore.Models.Options;

namespace ParserCore.Models.Parsers.ConcreteParsers;
public class WallpaperFlareParser : IParser
{
    public IParserOptions Options { get; set; }

    public WallpaperFlareParser(IParserOptions options)
    {
        Options = options;
    }
    public IEnumerable<string> GetLinksToPagesThatContainsSingleImage(IHtmlDocument doc)
    {
        //< a itemprop = "url" 
        //    href = "https://www.wallpaperflare.com/brown-wooden-bench-night-city-lights-cityscape-anime-lantern-wallpaper-mwcit"
        //    target = "_blank" >
        var elements = doc.QuerySelectorAll("a")
                          .Where(x => x.HasAttribute("href")
                                           && x.HasAttribute("itemprop")
                                           && x.HasAttribute("target"));
        var links = from element in elements
                    from attr in element.Attributes.Where(attr => attr.Name == "href")
                    select attr.Value;
        return links;
    }

    public IEnumerable<string> GetDirectImageLinks(IHtmlDocument doc)
    {
        //< a class="l" 
        //href="https://www.wallpaperflare.com/brown-wooden-bench-night-city-lights-cityscape-anime-lantern-wallpaper-mwcit/download/1920x1080">
        //1920x1080</a>
        var links = doc.QuerySelectorAll("a")
                       .Where(x => x.ClassName == "l"
                                        && x.HasAttribute("href")
                                        && x.InnerHtml.Contains(Options.Resolution ?? DisplayResolution.Resolution))
                       .Select(x => x.GetAttribute("href") ?? "");
        //IEnumerable<string> base64Strings = GetBase64StringsAsync(links);
        return links;
    }
    private IEnumerable<string> GetBase64StringsAsync(IEnumerable<IHtmlDocument> docs)
    {
        var base64Strings = new List<string>();
        Parallel.ForEach(docs, doc =>
        {
            string? str = GetBase64StringAsync(doc);
            base64Strings.Add(str);
        });
        return base64Strings;
    }
    private string GetBase64StringAsync(IHtmlDocument doc)
    {
        string? str = doc.QuerySelectorAll("img")
                         .Where(x => x.HasAttribute("alt")
                                          && x.HasAttribute("itemprop")
                                          && x.HasAttribute("src"))
                         .Select(x => x.GetAttribute("src"))
                         .FirstOrDefault();
        return str ?? "";
    }
    public string ConstructUrl(int id)
    {
        string query = Options.SearchQuery.Replace(" ", "+").Trim().ToLowerInvariant();
        Options.SearchQuery = query;
        return $"{Options.BaseUrl}{Options.PagePrefix}{id}";
    }
}