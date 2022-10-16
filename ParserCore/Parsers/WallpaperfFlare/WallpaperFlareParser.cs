using AngleSharp.Html.Dom;
using ParserCore.Helpers;
using ParserCore.Interfaces;

namespace ParserCore.Parsers.WallpaperfFlare
{
    public class WallpaperFlareParser : ImageParser
    {
        public WallpaperFlareParser(IParserOptions options)
            : base(options) { }
        public override async Task<IEnumerable<string>> Parse(IProgress<ProgressInfo> progress, CancellationToken token)
        {
            var pagesWithEncodedImages = await base.Parse(progress, token);
            var docsWithEncodedImages = await Doc.GetHtmlDocumentsAsync(pagesWithEncodedImages, progress, token);
            var base64Strings = docsWithEncodedImages.SelectMany(x => GetBase64StringsAsync(docsWithEncodedImages));
            return base64Strings;
        }
        protected override IEnumerable<string> GetImageListPageLinks(IHtmlDocument doc)
        {
            //< a itemprop = "url" 
            //    href = "https://www.wallpaperflare.com/brown-wooden-bench-night-city-lights-cityscape-anime-lantern-wallpaper-mwcit"
            //    target = "_blank" >
            var elements = doc.QuerySelectorAll("a")
                .Where(x => x != null && x.HasAttribute("href")
                                      && x.HasAttribute("itemprop")
                                      && x.HasAttribute("target"));

            var links = from element in elements
                        from attr in element.Attributes.Where(attr => attr.Name == "href")
                        select attr.Value;
            return links;

        }

        protected override IEnumerable<string> GetImageSources(IHtmlDocument doc)
        {
            //< a class="l" 
            //href="https://www.wallpaperflare.com/brown-wooden-bench-night-city-lights-cityscape-anime-lantern-wallpaper-mwcit/download/1920x1080">
            //1920x1080</a>
            var links = doc.QuerySelectorAll("a")
                .Where(x => x != null && x.ClassName == "l"
                                      && x.HasAttribute("href")
                                      && x.InnerHtml.Contains(Options.Resolution ?? Display.Resolution))
                .Select(x => x.GetAttribute("href") ?? "");
            //IEnumerable<string> base64Strings = GetBase64StringsAsync(links);
            return links;
        }
        private IEnumerable<string> GetBase64StringsAsync(IEnumerable<IHtmlDocument> docs)
        {
            var base64Strings = new List<string>();
            Parallel.ForEach(docs, doc =>
            {
                var str = GetBase64StringAsync(doc);
                base64Strings.Add(str);
            });
            return base64Strings;
        }

        private string GetBase64StringAsync(IHtmlDocument doc)
        {
            string? str = doc.QuerySelectorAll("img")
                .Where(x => x != null && x.HasAttribute("alt")
                                      && x.HasAttribute("itemprop")
                                      && x.HasAttribute("src"))
                .Select(x => x.GetAttribute("src"))
                .FirstOrDefault();
            return str ?? "";
        }

        protected override string MakeUrl(int id)
        {
            string query = Options.SearchQuery.Replace(" ", "+").Trim().ToLowerInvariant();
            Options.SearchQuery = query;
            return $"{Options.BaseUrl}{Options.PagePrefix}{id}";
        }
    }
}
