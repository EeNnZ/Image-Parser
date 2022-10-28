using AngleSharp.Html.Dom;
using ParserCore.Interfaces;

namespace ParserCore.Parsers.Wallhaven
{
    public class WallhavenParser : ImageParser
    {
        //TODO: Fix wallhaven
        public WallhavenParser(IParserOptions options)
            : base(options) { }
        protected override IEnumerable<string> GetLinksToPagesWithSingleImage(IHtmlDocument doc)
        {
            //<a class="preview" href="https://wallhaven.cc/w/4d3kog" target="_blank"></a>
            var elements = doc.QuerySelectorAll("a")
                .Where(x => x != null && x.ClassName == "preview"
                                      && x.HasAttribute("href")
                                      && x.HasAttribute("target"));
            var links = from element in elements
                        from attr in element.Attributes.Where(attr => attr.Name == "href")
                        select attr.Value;
            return links;
        }

        protected override IEnumerable<string> GetImageSources(IHtmlDocument doc)
        {
            //<img id="wallpaper" 
            //    src="https://w.wallhaven.cc/full/k9/wallhaven-k92o97.jpg"
            //    class="fill-horizontal">
            //data-wallpaper-width="1920"
            //
            //TODO: Add display resolution support
            var links = doc.QuerySelectorAll("img")
                .Where(x => x != null
                      && x.Id == "wallpaper"
                      && x.HasAttribute("src"))
                .Select(x => x.GetAttribute("src") ?? "");
            return links;
        }

        protected override string MakeUrl(int id)
        {
            string query = Options.SearchQuery.Replace(" ", "%20").Trim().ToLowerInvariant();
            Options.SearchQuery = query;
            return $"{Options.BaseUrl}{Options.PagePrefix}{id}";
        }
    }
}
