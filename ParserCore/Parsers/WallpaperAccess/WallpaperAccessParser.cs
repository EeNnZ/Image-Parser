﻿using AngleSharp.Html.Dom;
using ParserCore.Helpers;
using ParserCore.Interfaces;
using ParserCore.Parsers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Parsers.WallpaperAccess
{
    public class WallpaperAccessParser : ImageParser
    {
        public WallpaperAccessParser(IParserOptions options) : base(options) { }
        public override async Task<IEnumerable<string>> Parse(IProgress<ProgressChangedEventArgs> progress, CancellationToken token, [CallerMemberName] string callerName = "")
        {
            //TODO: Logging
            string mainPageUrl = MakeUrl();

            var mainPageDocument = await Doc.GetDocumentAsync(mainPageUrl, progress, token);


            var linksToImageGroups = GetLinksToImageGroups(mainPageDocument);


            var imageGroups = await Doc.GetHtmlDocumentsAsync(linksToImageGroups, progress, token);

#if DEBUG
            var sources = new List<string>();
            Log.Debug("Iterating through documents with single image started");
            foreach (var document in imageGroups)
            {
                Log.Debug("Processig {uri}", document.BaseUri);
                var collection = GetImageSources(document);
                Log.Debug("{sourcesCount} sources parsed from {uri}", collection.Count(), document.BaseUri);
                sources.AddRange(collection);
            }
#else
            Log.Information("Getting images urls");
            var sources = imageGroups.SelectMany(doc => GetImageSources(doc));
            Log.Information("Got {urlsCount} images urls", sources.Count());
#endif

            return Options.ImageCount > 0 ? sources.Take(Options.ImageCount) : sources;
        }
        private IEnumerable<string> GetLinksToImageGroups(IHtmlDocument doc)
        {
            var links = from element in doc.QuerySelectorAll("a")
                        let hrefVal = element.GetAttribute("href")
                        where element != null && element.HasAttribute("href")
                                              && element.ClassName == "ui fluid image"
                                              && hrefVal != null
                                              && hrefVal.StartsWith("/")
                        select Options.BaseUrl[..^1] + element.GetAttribute("href");
            return links;
        }
        protected override IEnumerable<string> GetImageSources(IHtmlDocument doc)
        {
            var links = from element in doc.QuerySelectorAll("a")
                        let hrefVal = element.GetAttribute("href")
                        where element != null && element.HasAttribute("href") 
                                              && hrefVal != null
                                              && hrefVal.StartsWith("/download")
                        select Options.BaseUrl[..^1] + element.GetAttribute("href");

            return links;
        }

        protected override IEnumerable<string> GetLinksToPagesWithSingleImage(IHtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        protected override string MakeUrl(int id = 0)
        {
            string query = Options.SearchQuery.Replace(" ", "+").Trim().ToLowerInvariant();
            Options.SearchQuery = query;
            return $"{Options.BaseUrl}{Options.SearchPrefix}{query}";
        }
    }
}
