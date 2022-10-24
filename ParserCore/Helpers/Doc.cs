using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ParserCore.Parsers;
using Serilog;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ParserCore.Helpers
{
    public static class Doc
    {
        public static async Task<IEnumerable<IHtmlDocument>> GetHtmlDocumentsAsync(IEnumerable<string> urls,
                                                                                   IProgress<ProgressChangedEventArgs> progress,
                                                                                   CancellationToken token,
                                                                                   [CallerMemberName]string callerName = "")
        {
            Log.Information("Thread: {ThreadId} with caller: {Caller} entered to {MethodName}",
                Environment.CurrentManagedThreadId, callerName, MethodBase.GetCurrentMethod()?.Name);
            var htmlPages = await HtmlDownloader.DownloadParallelAsync(urls, progress, token);
            Log.Information("Html pages downloaded, collection contains {itemsCount} elements", htmlPages.Count());
            if (htmlPages == null)
            {
                Log.Error("Html pages collection is null");
                throw new NullReferenceException(nameof(htmlPages));
            }
            else if (!htmlPages.Any())
            {
                Log.Error("Html pages collection contains zero elements");
                throw new Exception("collection is empty");
            }

            return await GetDocumentsParallelAsync(htmlPages, progress, token);
        }
        private static async Task<IEnumerable<IHtmlDocument>> GetDocumentsParallelAsync(IEnumerable<string> htmlPages,
                                                                                IProgress<ProgressChangedEventArgs> progress,
                                                                                CancellationToken token,
                                                                                [CallerMemberName]string callerName = "")
        {
            Log.Information("Thread: {ThreadId} with caller: {Caller} entered to {MethodName}",
                             Environment.CurrentManagedThreadId,
                             callerName,
                             MethodBase.GetCurrentMethod()?.Name);
            var parser = new HtmlParser();
            var documents = new List<IHtmlDocument>();

            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Preparing documents" };
            int pagesCount = htmlPages.Count();
            progress.Report(pInfo);
            Log.Information("Parallel parse task is about to run in {ClassName}->{MethodName}",
                      typeof(HtmlDownloader).Name,
                      MethodBase.GetCurrentMethod()?.Name);
            await Task.Run(() =>
                    {
                        Log.Information("Task: {TaskId} started on thread: {ThreadId}", Task.CurrentId, Environment.CurrentManagedThreadId);
                        try
                        {
                            Parallel.ForEach(htmlPages, new ParallelOptions
                            {
                                CancellationToken = token,
                                MaxDegreeOfParallelism = Environment.ProcessorCount
                            }, page =>
                                        {
                                            var document = parser.ParseDocument(page);
                                            documents.Add(document);
                                            pInfo.Percentage = documents.Count * 100 / pagesCount;
                                            if (pInfo.Percentage >= 100)
                                            {
                                                pInfo.Percentage = 100;
                                                pInfo.TextStatus = "Done";
                                            }
                                            progress.Report(pInfo);
                                        });
                        }
                        catch { }
                    }, token);
            Log.Information("Thread: {ThreadId} with caller: {Caller} is about to exit from {MethodName}",
                             Environment.CurrentManagedThreadId,
                             callerName,
                             MethodBase.GetCurrentMethod()?.Name);
            return documents;
        }
        public static async Task<IHtmlDocument> GetDocumentAsync(string url,
                                                                  IProgress<ProgressChangedEventArgs> progress,
                                                                  CancellationToken token,
                                                                  [CallerMemberName] string callerName = "")
        {
            Log.Information("Thread: {ThreadId} with caller: {Caller} entered to {MethodName}",
                Environment.CurrentManagedThreadId, callerName, MethodBase.GetCurrentMethod()?.Name);
            var html = await HttpHelper.GetStringAsync(url, token);
            if (html == null || string.IsNullOrWhiteSpace(html))
            {
                throw new NullReferenceException($"{nameof(html)} is null or empty");
            }
            Log.Information("Html page downloaded");
            return new HtmlParser().ParseDocument(html);
        }
    }
}
