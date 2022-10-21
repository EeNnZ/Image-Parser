using AngleSharp.Html.Dom;
using ParserCore;
using ParserCore.Interfaces;
using ParserCore.Parsers;
using System.Runtime.Versioning;
namespace Parsers.HdWallpapers
{
    [SupportedOSPlatform("windows")]
    internal class HdWallpapersParser : ImageParser
    {
        //TODO: Floating error - sometimes this parser returns zero results when tries to download any html
        public HdWallpapersParser(IParserOptions options) : base(options)
        {
        }

        protected override IEnumerable<string> GetLinksToPagesWithSingleImage(IHtmlDocument doc)
        {
            var elements = doc.QuerySelectorAll("a")
                .Where(x => x != null && x.InnerHtml.Contains("img src")
                                      && x.HasAttribute("href"));
            var links = from elem in elements
                        from attr in elem.Attributes.Where(attr => attr.Name == "href")
                        let baseUrl = Options.BaseUrl
                        select baseUrl + attr.Value[1..];
            //select string.Concat(baseUrl, attr.Value);
            return links;

        }

        protected override IEnumerable<string> GetImageSources(IHtmlDocument doc)
        {
            //< a href = "download/yellow_green_autumn_trees_forest_road_4k_hd_nature-1280x720.jpg" 
            //    title = "HD 1280 x 720 Wallpaper" > 1280 x 720 </ a >
            var elements = doc.QuerySelectorAll("a")
                .Where(x => x != null && x.HasAttribute("href"));
            var sources = from elem in elements
                          from attr in elem.Attributes.Where(attr => attr.Name == "href")
                          where attr.Value.Contains("download") && attr.Value.Contains(Options.Resolution ?? Display.Resolution)
                          select Options.BaseUrl + attr.Value;

            //var sources = from tag in doc.QuerySelectorAll("a")
            //              .Where(x => x != null && x.HasAttribute("href")
            //                                    && x.HasAttribute("title"))
            //              let baseUrl = Options.BaseUrl
            //              let href = tag.GetAttribute("href")
            //              where tag != null && href != null
            //                                && href.Contains("/download/")
            //                                //&& href.Contains(Options.Resolution ?? Display.Resolution)
            //              select string.Concat(baseUrl, href[1..]);
            return sources;
        }

        protected override string MakeUrl(int id)
        {
            string query = Options.SearchQuery.Replace(" ", "%20").Trim().ToLowerInvariant();
            Options.SearchQuery = query;
            return $"{Options.BaseUrl}{Options.SearchPrefix}{Options.PagePrefix}{id}?q={query}";
        }
    }
}
