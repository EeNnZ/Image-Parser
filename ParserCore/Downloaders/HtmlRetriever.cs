using ParserCore.Helpers;
namespace ParserCore.Downloaders;

internal static class HtmlRetriever
{
    private const int REQ_TIMEOUT = 2000;
    private const int RETRIES_COUNT = 5;

    internal static async Task<IEnumerable<string>> RetrieveManyAsync(IEnumerable<string> urlCollection,
                                                                      IProgress<ProgressChangedEventArgs> progress,
                                                                      CancellationToken token)
    {
        string[] urls = urlCollection as string[] ?? urlCollection.ToArray();

        var pages = new List<string>();
        var pInfo = new ProgressChangedEventArgs { TextStatus = "Download started in sequentially mode" };
        progress.Report(pInfo);

        foreach (string url in urls)
        {
            token.ThrowIfCancellationRequested();
            string html = await TryRetrieveSingleHtmlAsync(url, token, progress, pInfo);

            if (string.IsNullOrEmpty(html)) continue;

            ReportSuccess(progress, url, pInfo, pages, urls.Length);
            pages.Add(html);
        }

        if (pages.Count == 0)
            throw new Exception("Collection is empty" + nameof(pages));

        return pages;
    }

    private static async Task<string> TryRetrieveSingleHtmlAsync(string url, CancellationToken token,
                                                      IProgress<ProgressChangedEventArgs> progress,
                                                      ProgressChangedEventArgs pInfo)
    {
        for (var i = 0; i < RETRIES_COUNT; i++)
        {
            try
            {
                string? page = await HttpHelper.GetStringAsync(url, token);
                if (page != null)
                    return page;

                token.ThrowIfCancellationRequested();
                ReportPageFail(progress, url, pInfo);
            }
            catch (HttpRequestException)
            {
                ReportRetry(progress, pInfo);
                Thread.Sleep(REQ_TIMEOUT);
            }
        }

        return string.Empty;
    }

    private static void ReportRetry(IProgress<ProgressChangedEventArgs> progress, ProgressChangedEventArgs pInfo)
    {
        pInfo.TextStatus = "Too many requests, waiting for 2 seconds...";
        progress.Report(pInfo);
        pInfo.TextStatus = "Rertying";
        progress.Report(pInfo);
    }

    private static void ReportSuccess(IProgress<ProgressChangedEventArgs> progress, string link, ProgressChangedEventArgs pInfo, ICollection<string> pages,
                                      int urlsCount)
    {
        pInfo.ItemsProcessed.Add(link);
        pInfo.Percentage = pages.Count * 100 / urlsCount;
        progress.Report(pInfo);
    }

    private static void ReportPageFail(IProgress<ProgressChangedEventArgs> progress, string link, ProgressChangedEventArgs pInfo)
    {
        pInfo.ItemsFailed.Add(link);
        progress.Report(pInfo);
    }
}