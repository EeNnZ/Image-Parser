using System.Collections.Concurrent;

namespace ParserCore.Helpers
{
    public static class HtmlDownloader
    {
        private const int REQ_TIMEOUT = 2000;
        private const int RETRIES_COUNT = 5;

        public static async Task<IEnumerable<string>> DownloadAsync(IEnumerable<string> urls,
                                                                     IProgress<ProgressInfo> progress,
                                                                     CancellationToken token)
        {
            var pages = new List<string>();
            var links = new ConcurrentBag<string>(urls);

            var pInfo = new ProgressInfo() { TextStatus = "Downloading pages..." };
            progress.Report(pInfo);
            int urlsCount = urls.Count();
            ParallelLoopResult plr = default;
            await Task.Run(() =>
                {
                    try
                    {
                        plr = Parallel.ForEach(links,
                            new ParallelOptions()
                            {
                                CancellationToken = token,
                                MaxDegreeOfParallelism = Environment.ProcessorCount
                            }, (string? link, ParallelLoopState pls) =>
                            {
                                try
                                {
                                    if (!links.TryTake(out link)) return;
                                    var page = HttpHelper.GetStringAsync(link, token).Result;
                                    if (page is null) return;
                                    pages.Add(page);
                                    pInfo.ItemsProcessed.Add(link);
                                    pInfo.Percentage = pages.Count * 100 / urlsCount;
                                    progress.Report(pInfo);
                                }
                                catch (AggregateException) //when (ex.InnerException is HttpRequestException httpEx)
                                {
                                    pInfo.TextStatus = "Parallel download interrupted";
                                    progress.Report(pInfo);
                                    pls.Break();
                                }
                            });
                    }
                    catch { }
                }, token);
            if (!plr.IsCompleted && !token.IsCancellationRequested)
            {
                await Task.Run(() => LoadPagesSequentially(pages, urls, progress, pInfo, token), token);
            }
            return pages;
        }

        private static void LoadPagesSequentially(List<string> pages, IEnumerable<string> urls, IProgress<ProgressInfo> progress, ProgressInfo pInfo, CancellationToken token)
        {
            pages.Clear();
            pInfo.ItemsProcessed.Clear();
            pInfo.TextStatus = "Download restarted in sequentially mode";
            progress.Report(pInfo);
            int urlsCount = urls.Count();

            try
            {
                foreach (string url in urls)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < RETRIES_COUNT; i++)
                    {
                        try
                        {
                            string? page = HttpHelper.GetStringAsync(url, token).Result;
                            if (page == null) return;
                            pages.Add(page);
                            pInfo.ItemsProcessed.Add(url);
                            pInfo.TextStatus = "Downloading pages...";
                            pInfo.Percentage = pages.Count * 100 / urlsCount;
                            progress.Report(pInfo);
                        }
                        catch (AggregateException aex) when (aex.InnerException is HttpRequestException)
                        {
                            pInfo.TextStatus = "Too many requests, waiting for 2 seconds...";
                            progress.Report(pInfo);
                            Thread.Sleep(REQ_TIMEOUT);
                            pInfo.TextStatus = "Rertying";
                            progress.Report(pInfo);
                            continue;
                        }
                    }
                }
            }
            catch { }
        }
    }
}
