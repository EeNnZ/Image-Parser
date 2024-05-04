using AngleSharp.Html.Dom;
using ParserCore.Downloaders;
using ParserCore.Helpers;
using ParserCore.Models.Options;
using ParserCore.Models.Parsers.ConcreteParsers;

namespace ParserCore.Models.Parsers;

public class ParserExecutor
{
    private readonly IParser _concreteParser;

    public ParserExecutor(IParser concreteParser)
    {
        ThrowIfOptionsInvalid(concreteParser.Options);
        _concreteParser = concreteParser;
    }
    public async Task<IEnumerable<string>> RetrieveDirectLinksToImages(CancellationToken token,
                                                                       IProgress<ProgressChangedEventArgs> progress)
    {
        if (_concreteParser is WallpaperAccessParser wap)
            return await wap.RetrieveSources(token, progress);

        var sources = await RetrieveSources(token, progress);

        if (_concreteParser.Options.ImageCount > 0)
            return sources.Take(_concreteParser.Options.ImageCount);

        return sources;
    }

    private async Task<List<string>> RetrieveSources(CancellationToken token, IProgress<ProgressChangedEventArgs> progress)
    {
        var sources = new List<string>();
        foreach (var document in await GetDocumentsWithSingleImage(token, progress))
        {
            var collection = _concreteParser.GetDirectImageLinks(document);
            sources.AddRange(collection);
        }

        return sources;
    }
    private async Task<IEnumerable<IHtmlDocument>> GetDocumentsWithSingleImage(CancellationToken token,
                                                                               IProgress<ProgressChangedEventArgs> progress)
    {
        var urls = GetUrls();
        var documentsWithImageList = await DocumentParser.GetHtmlDocumentsAsync(urls, progress, token);

        var linksToPagesWithSingleImage = documentsWithImageList.SelectMany(_concreteParser.GetLinksToPagesThatContainsSingleImage);
        var documentsWithSingleImage = await DocumentParser.GetHtmlDocumentsAsync(linksToPagesWithSingleImage, progress, token);

        return documentsWithSingleImage;
    }
    private IEnumerable<string> GetUrls()
    {
        int pageCount = _concreteParser.Options.EndPoint - _concreteParser.Options.StartPoint;
        var urls = new List<string>();
        for (int id = _concreteParser.Options.StartPoint; id <= pageCount; id++) urls.Add(_concreteParser.ConstructUrl(id));
        return urls;
    }

    private static void ThrowIfOptionsInvalid(IParserOptions value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (value.SearchQuery == "")
            throw new ArgumentOutOfRangeException("Query cannot be empty", nameof(value.SearchQuery));

        if (value.EndPoint <= value.StartPoint)
            throw new ArgumentOutOfRangeException($"{nameof(value.StartPoint)} and {value.EndPoint} cannot be equal");
    }
}