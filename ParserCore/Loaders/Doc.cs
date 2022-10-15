﻿using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace ParserCore.Loaders
{
    public static class Doc
    {
        public static async Task<IEnumerable<IHtmlDocument>> GetHtmlDocumentsAsync(IEnumerable<string> urls,
                                                                                   IProgress<ProgressInfo> progress,
                                                                                   CancellationToken token)
        {
            IEnumerable<string>? htmlPages = null;
            try
            {
                htmlPages = await HtmlPageLoader.LoadPagesAsync(urls, progress, token);
            }
            catch (OperationCanceledException)
            { 
            }
            if (htmlPages == null) throw new NullReferenceException(nameof(htmlPages));
            return await GetDocumentsAsync(htmlPages, progress, token);
        }
        private static async Task<IEnumerable<IHtmlDocument>> GetDocumentsAsync(IEnumerable<string> htmlPages,
                                                                                IProgress<ProgressInfo> progress,
                                                                                CancellationToken token)
        {
            var parser = new HtmlParser();
            var documents = new List<IHtmlDocument>();

            var pInfo = new ProgressInfo() { TextStatus = "Preparing documents" };
            int pagesCount = htmlPages.Count();
            progress.Report(pInfo);

            try
            {
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
                                            pInfo.Percentage = (documents.Count * 100) / pagesCount;
                                            progress.Report(pInfo);
                                        });
                            }
                            catch (OperationCanceledException) { return; }
                        }, token);
            }
            catch (OperationCanceledException) { }
            return documents;
        }
    }
}
