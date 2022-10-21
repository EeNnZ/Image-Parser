using System.Collections.Concurrent;
using ParserCore.Parsers;

namespace ParserCore.Helpers
{
    public static class HtmlDownloader
    {
        public const int REQ_TIMEOUT = 2000;
        public const int RETRIES_COUNT = 5;

        public static async Task<IEnumerable<string>> DownloadAsync(IEnumerable<string> urls, IProgress<ProgressChangedEventArgs> progress, CancellationToken token)
        {
            var pages = new List<string>();
            var links = new ConcurrentBag<string>(urls);

            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Downloading pages..." };
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
                            }, 
                            (string? link, ParallelLoopState pls) =>
                            {
                                try
                                {
                                    if (!links.TryTake(out link)) return;
                                    string? page = HttpHelper.GetStringAsync(link, token).Result;
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
                return await Task.Run(() => DownloadSequentially(urls, progress, token), token);
            }
            return pages;
        }

        private static IEnumerable<string> DownloadSequentially(IEnumerable<string> urls, IProgress<ProgressChangedEventArgs> progress, CancellationToken token)
        {
            var pages = new List<string>();
            var links = new List<string>(urls);
            int urlsCount = urls.Count();

            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Download restarted in sequentially mode" };
            progress.Report(pInfo);

            try
            {
                foreach (string link in links)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < RETRIES_COUNT; i++)
                    {
                        try
                        {
                            string? page = HttpHelper.GetStringAsync(link, token).Result;
                            if (page == null) continue;
                            pages.Add(page);
                            pInfo.ItemsProcessed.Add(link);
                            pInfo.Percentage = pages.Count * 100 / urlsCount;
                            progress.Report(pInfo);
                            break;
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
            return pages;
        }
    }
}
