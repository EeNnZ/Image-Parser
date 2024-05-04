using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ParserCore.Helpers;
using System.Runtime.CompilerServices;
namespace ParserCore.Downloaders;

internal static class DocumentParser
{
    internal static async Task<IEnumerable<IHtmlDocument>> GetHtmlDocumentsAsync(IEnumerable<string> urls,
                                                                                 IProgress<ProgressChangedEventArgs> progress,
                                                                                 CancellationToken token)
    {
        var htmlPages = await HtmlRetriever.RetrieveManyAsync(urls, progress, token);
        if (htmlPages == null)
            throw new NullReferenceException(nameof(htmlPages));

        var pages = htmlPages as string[] ?? htmlPages.ToArray();
        if (!pages.Any())
            throw new Exception("collection is empty" + nameof(htmlPages));

        return GetDocumentsParallelAsync(pages, progress, token);
    }

    private static IEnumerable<IHtmlDocument> GetDocumentsParallelAsync(
        IEnumerable<string> htmlPages,
        IProgress<ProgressChangedEventArgs> progress,
        CancellationToken token)
    {
        var pages = htmlPages as string[] ?? htmlPages.ToArray();

        var progressInfo = new ProgressChangedEventArgs { TextStatus = "Preparing documents" };
        progress.Report(progressInfo);

        var documents = GetDocumentsParallel(pages, progress, progressInfo, token);

        return documents;
    }

    private static IEnumerable<IHtmlDocument> GetDocumentsParallel(
        IReadOnlyCollection<string> pages,
        IProgress<ProgressChangedEventArgs> progress,
        ProgressChangedEventArgs progressInfo,
        CancellationToken token)
    {
        var parser = new HtmlParser();
        var documents = new List<IHtmlDocument>();
        int pagesCount = pages.Count;

        Parallel.ForEach(pages, new ParallelOptions
        {
            CancellationToken = token,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, page =>
        {
            ProcessSinglePage(progress, progressInfo, parser, page, documents, pagesCount);
        });

        return documents;
    }

    private static void ProcessSinglePage(IProgress<ProgressChangedEventArgs> progress,
                                          ProgressChangedEventArgs progressInfo,
                                          IHtmlParser parser,
                                          string page,
                                          ICollection<IHtmlDocument> documents,
                                          int pagesCount)
    {
        var document = parser.ParseDocument(page);
        documents.Add(document);

        progressInfo.Percentage = documents.Count * 100 / pagesCount;
        if (progressInfo.Percentage >= 100)
        {
            progressInfo.Percentage = 100;
            progressInfo.TextStatus = "Done";
        }

        progress.Report(progressInfo);
    }

    public static async Task<IHtmlDocument> DownloadDocumentAsync(string url,
                                                                  IProgress<ProgressChangedEventArgs> progress,
                                                                  CancellationToken token,
                                                                  [CallerMemberName] string callerName = "")
    {
        string? html = await HttpHelper.GetStringAsync(url, token);

        if (html == null || string.IsNullOrWhiteSpace(html))
            throw new NullReferenceException($"{nameof(html)} is null or empty");

        return await new HtmlParser().ParseDocumentAsync(html, token);
    }
}