using ParserCore.Parsers;
using Serilog;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ParserCore.Helpers
{
    public static class HtmlDownloader
    {
        public const int REQ_TIMEOUT = 2000;
        public const int RETRIES_COUNT = 5;

        public static async Task<IEnumerable<string>> DownloadParallelAsync(IEnumerable<string> urls,
                                                                            IProgress<ProgressChangedEventArgs> progress,
                                                                            CancellationToken token,
                                                                            [CallerMemberName] string callerName = "")
        {
            Log.Debug("Thread: {ThreadId} with caller: {Caller} entered to DownloadParallelAsync",
                             Environment.CurrentManagedThreadId,
                             callerName);
            var pages = new List<string>();
            var links = new ConcurrentBag<string>(urls);

            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Downloading pages..." };
            progress.Report(pInfo);
            int urlsCount = urls.Count();
            ParallelLoopResult plr = default;
            Log.Debug("Parallel download task is about to run");
            await Task.Run(() =>
                {
                    Log.Debug("Task: {TaskId} started on thread: {ThreadId}", Task.CurrentId, Environment.CurrentManagedThreadId);
                    try
                    {
                        Log.Debug("ParallelForEach started");
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
                                catch (AggregateException aex) //when (ex.InnerException is HttpRequestException httpEx)
                                {
                                    Log.Error("ArrgregateException with inner {InnerException} caught inside parallel ForEach with message: {InnerExMessage}", aex.InnerException, aex.InnerException.Message);
                                    pInfo.TextStatus = "Parallel download interrupted";
                                    progress.Report(pInfo);
                                    pls.Break();
                                    Log.Information("Parallel loop break");
                                }
                            });
                    }
                    catch(Exception ex) { Log.Error("Exception {Exception} with message {ExMessage} caught in ParallelForEach task", ex, ex.Message); };
                }, token);
            if (!plr.IsCompleted && !token.IsCancellationRequested)
            {
                Log.Debug("ParallelForEach is not completed and cancellation is not requested");
                Log.Information("Starting sequential downloading");
                return await Task.Run(() => DownloadSequentially(urls, progress, token), token);
            }
            Log.Debug("Thread: {ThreadId} with caller: {Caller} is about to exit from {MethodName}",
                Environment.CurrentManagedThreadId, callerName, MethodBase.GetCurrentMethod()?.Name);
            return pages;
        }

        private static IEnumerable<string> DownloadSequentially(IEnumerable<string> urls,
                                                                IProgress<ProgressChangedEventArgs> progress,
                                                                CancellationToken token,
                                                                [CallerMemberName] string callerName = "")
        {
            Log.Debug("Thread: {ThreadId} with caller: {Caller} entered to DownloadParallelAsync",
                             Environment.CurrentManagedThreadId,
                             callerName);
            var pages = new List<string>();
            var links = new List<string>(urls);
            int urlsCount = urls.Count();

            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Download restarted in sequentially mode" };
            progress.Report(pInfo);

            try
            {
                Log.Information("Sequential html download started");
                foreach (string link in links)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < RETRIES_COUNT; i++)
                    {
                        try
                        {
                            string? page = HttpHelper.GetStringAsync(link, token).Result;
                            if (page == null)
                            {
                                pInfo.ItemsFailed.Add(link);
                                progress.Report(pInfo);
                                continue;
                            };
                            pages.Add(page);
                            pInfo.ItemsProcessed.Add(link);
                            pInfo.Percentage = pages.Count * 100 / urlsCount;
                            progress.Report(pInfo);
                            break;
                        }
                        catch (AggregateException aex) when (aex.InnerException is HttpRequestException)
                        {
                            Log.Error("ArrgregateException with inner {InnerException} caught inside foreach with message: {InnerExMessage}", aex.InnerException, aex.InnerException.Message);
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
            catch(Exception ex) { Log.Error("Exception {Exception} with message {ExMessage} caught in foreach task", ex, ex.Message); }
            Log.Error("Thread: {ThreadId} with caller: {Caller} is about to exit from {MethodName}",
                Environment.CurrentManagedThreadId, callerName, MethodBase.GetCurrentMethod()?.Name);
            if (pages.Count == 0)
            {
                Log.Error("Class: {ClassName}, Method: {MethodName} returned empty collection ", typeof(HtmlDownloader).Name, MethodBase.GetCurrentMethod()?.Name);
            }
            return pages;
        }
    }
}
