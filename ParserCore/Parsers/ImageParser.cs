using AngleSharp.Html.Dom;
using ParserCore.Helpers;
using ParserCore.Interfaces;
using Serilog;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ParserCore.Parsers
{
    public abstract class ImageParser
    {
        protected IParserOptions _options = null!;
        public IParserOptions Options
        {
            get { return _options; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                else if (value == _options) return;
                else if (value.SearchQuery == "") throw new ArgumentOutOfRangeException("Query cannot be empty", nameof(value.SearchQuery));
                else if (value.EndPoint <= value.StartPoint) throw new ArgumentOutOfRangeException($"{nameof(value.StartPoint)} and {value.EndPoint} cannot be equal");
                else _options = value;

            }
        }
        public ImageParser(IParserOptions options)
        {
            Options = options;
        }
        public virtual async Task<IEnumerable<string>> Parse(IProgress<ProgressChangedEventArgs> progress,
                                                             CancellationToken token,
                                                             [CallerMemberName] string callerName = "")
        {
            Log.Information("Thread: {ThreadId} with caller: {Caller} entered to {ClassName}->{MethodName}",
                             Environment.CurrentManagedThreadId,
                             callerName,
                             MethodBase.GetCurrentMethod()?.ReflectedType,
                             MethodBase.GetCurrentMethod()?.Name);

            Log.Information("Getting urls");
            IEnumerable<string> urls = GetUrls();
            Log.Information("Got {urlsCount} urls", urls.Count());

            Log.Information("Getting html documents with image list");
            var documentsWithImageList = await Doc.GetHtmlDocumentsAsync(urls, progress, token);
            Log.Information("Got {documentsCount} documents", documentsWithImageList.Count());

            Log.Information("Getting links to single image pages");
            var linksToPagesWithSingleImage = documentsWithImageList.SelectMany(doc => GetLinksToPagesWithSingleImage(doc));
            Log.Information("Got {linksCount}", linksToPagesWithSingleImage.Count());

            Log.Information("Getting html documents with image list");
            var documentsWithSingleImage = await Doc.GetHtmlDocumentsAsync(linksToPagesWithSingleImage, progress, token);
            Log.Information("Got {documentsCount} documents", documentsWithSingleImage.Count());

#if DEBUG
            var sources = new List<string>();
            Log.Debug("Iterating through documents with single image started");
            foreach (var document in documentsWithSingleImage)
            {
                Log.Debug("Processig {uri}", document.BaseUri);
                var collection = GetImageSources(document);
                Log.Debug("{sourcesCount} sources parsed from {uri}", collection.Count(), document.BaseUri);
                sources.AddRange(collection);
            }
#else
            Log.Information("Getting images urls");
            var sources = documentsWithSingleImage.SelectMany(doc => GetImageSources(doc));
            Log.Information("Got {urlsCount} images urls", sources.Count());
#endif
            return Options.ImageCount > 0 ? sources.Take(Options.ImageCount) : sources;

        }

        protected IEnumerable<string> GetUrls()
        {
            if (Options == null) throw new NullReferenceException(nameof(Options));
            int pageCount = Options.EndPoint - Options.StartPoint;
            var urls = new List<string>();
            for (int id = Options.StartPoint; id <= pageCount; id++)
            {
                urls.Add(MakeUrl(id));
            }
            return urls;
        }

        protected abstract IEnumerable<string> GetImageSources(IHtmlDocument doc);
        protected abstract IEnumerable<string> GetLinksToPagesWithSingleImage(IHtmlDocument doc);
        protected abstract string MakeUrl(int id);
    }
}
