using AngleSharp.Html.Dom;
using ParserCore.Interfaces;
using ParserCore.Loaders;
using System.Reflection.Metadata;

namespace ParserCore
{
    public abstract class ImageParser
    {
        public bool SearchMode => !string.IsNullOrEmpty(Options.SearchQuery);
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
        public virtual async Task<IEnumerable<string>> Parse(IProgress<ProgressInfo> progress, CancellationToken token)
        {
            IEnumerable<string> urls = GetUrls();

            var documentsWithImageList = await Doc.GetHtmlDocumentsAsync(urls, progress, token);
            var linksToPagesWithSingleImage = documentsWithImageList.SelectMany(doc => GetImageListPageLinks(doc));

            var documentsWithSingleImage = await Doc.GetHtmlDocumentsAsync(linksToPagesWithSingleImage, progress, token);
            var sources = documentsWithSingleImage.SelectMany(doc => GetImageSources(doc));

            return sources;

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
        protected abstract IEnumerable<string> GetImageListPageLinks(IHtmlDocument doc);
        protected abstract string MakeUrl(int id);
    }
}
