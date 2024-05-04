using AngleSharp.Html.Dom;
using ParserCore.Downloaders;
using ParserCore.Helpers;
using ParserCore.Models.Options;

namespace ParserCore.Models.Parsers.ConcreteParsers;
public class WallpaperAccessParser : IParser
{
    public IParserOptions Options { get; set; }

    public WallpaperAccessParser(IParserOptions options)
    {
        Options = options;
    }
    public IEnumerable<string> GetLinksToPagesThatContainsSingleImage(IHtmlDocument document)
    {
        throw new NotImplementedException();
    }
    public async Task<IEnumerable<string>> RetrieveSources(CancellationToken token,
        IProgress<ProgressChangedEventArgs> progress)
    {
        string mainPageUrl = ConstructUrl();

        var mainPageDocument = await DocumentParser.DownloadDocumentAsync(mainPageUrl, progress, token);
        if (mainPageDocument == null)
            throw new NullReferenceException(nameof(mainPageDocument));

        var linksToImageGroups = GetLinksToImageGroups(mainPageDocument);
        var imageGroups = await DocumentParser.GetHtmlDocumentsAsync(linksToImageGroups, progress, token);
#if DEBUG
        var sources = new List<string>();
        foreach (var imageGroupDocument in imageGroups)
        {
            var collection = GetDirectImageLinks(imageGroupDocument);
            sources.AddRange(collection);
        }
#else
            var sources = imageGroups.SelectMany(doc => GetDirectImageLinks(doc));
#endif
        if (Options.ImageCount > 0)
            return sources.Take(Options.ImageCount);

        return sources;
    }
    private IEnumerable<string> GetLinksToImageGroups(IHtmlDocument doc)
    {
        var links = from element in doc.QuerySelectorAll("a")
                    let hrefVal = element.GetAttribute("href")
                    where element != null && element.HasAttribute("href")
                                          && element.ClassName == "ui fluid image"
                                          && hrefVal != null
                                          && hrefVal.StartsWith("/")
                    select Options.BaseUrl[..^1] + element.GetAttribute("href");
        return links;
    }

    public IEnumerable<string> GetDirectImageLinks(IHtmlDocument doc)
    {
        var links = from element in doc.QuerySelectorAll("a")
                    let hrefVal = element.GetAttribute("href")
                    where element != null && element.HasAttribute("href")
                                          && hrefVal != null
                                          && hrefVal.StartsWith("/download")
                    select Options.BaseUrl[..^1] + element.GetAttribute("href");
        return links;
    }
    public string ConstructUrl(int id = 0)
    {
        string query = Options.SearchQuery.Replace(" ", "+").Trim().ToLowerInvariant();
        Options.SearchQuery = query;
        var url = $"{Options.BaseUrl}{Options.SearchPrefix}{query}";
        return url;
    }
}