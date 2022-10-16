using AngleSharp.Html.Dom;
using ParserCore.Interfaces;
using System.Runtime.Versioning;

namespace ParserCore.Parsers.WallpapersWide
{
    [SupportedOSPlatform("windows")]
    public class WallpapersWideParser : ImageParser
    {
        public WallpapersWideParser(IParserOptions options)
            : base(options) { }
        protected override string MakeUrl(int pageId)
        {
            string url;
            if (SearchMode)
            {
                string searchQuery = Options.SearchQuery.Replace(" ", "%20").Trim().ToLowerInvariant();
                url = $"{Options.BaseUrl}{Options.SearchPrefix}/{pageId}?q={searchQuery}";
            }
            else { url = $"{Options.BaseUrl}{Options.PagePrefix}/{pageId}"; }
            return url;
        }

        protected override IEnumerable<string> GetImageListPageLinks(IHtmlDocument document)
        {
            var elements = document.QuerySelectorAll("div")
                .Where(x => x != null
                && x.ClassName == "mini-hud"
                && x.OuterHtml.Contains("prevframe_show"));
            var links = from element in elements
                        from attr in element.Attributes.Where(attr => attr.Name == "onclick")
                        let link = attr.Value.Split("'").Where(x => x.Contains("http")).FirstOrDefault()
                        select link ?? "";
            return links;
        }
        protected override IEnumerable<string> GetImageSources(IHtmlDocument document)
        {
            //<a target = "_self"
            //   href = "/download/wildlife_5-wallpaper-1920x1080.jpg" 
            //   title = "HD 16:9 1920 x 1080 wallpaper for FHD 1080p High Definition or Full HD displays" > 1920x1080
            //</a>
            //http://wallpaperswide.com/download/wildlife_5-wallpaper-1920x1080.jpg
            return from tag in document.QuerySelectorAll("a")
                   let baseUrl = Options.BaseUrl
                   let href = tag.GetAttribute("href")
                   where tag != null && href != null
                                     && href.Contains("/download/")
                                     && href.Contains(Options.Resolution ?? Display.Resolution)
                   select string.Concat(baseUrl, href[1..]);
        }
    }
}
