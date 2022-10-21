using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ParserCore.Parsers;

namespace ParserCore.Helpers
{
    public static class Doc
    {
        public static async Task<IEnumerable<IHtmlDocument>> GetHtmlDocumentsAsync(IEnumerable<string> urls, IProgress<ProgressChangedEventArgs> progress, CancellationToken token)
        {
            var htmlPages = await HtmlDownloader.DownloadAsync(urls, progress, token);

            if (htmlPages == null) throw new NullReferenceException(nameof(htmlPages));
            return await GetDocumentsAsync(htmlPages, progress, token);
        }
        private static async Task<IEnumerable<IHtmlDocument>> GetDocumentsAsync(IEnumerable<string> htmlPages,
                                                                                IProgress<ProgressChangedEventArgs> progress,
                                                                                CancellationToken token)
        {
            var parser = new HtmlParser();
            var documents = new List<IHtmlDocument>();

            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Preparing documents" };
            int pagesCount = htmlPages.Count();
            progress.Report(pInfo);

            await Task.Run(() =>
                    {
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

            return documents;
        }
    }
}
