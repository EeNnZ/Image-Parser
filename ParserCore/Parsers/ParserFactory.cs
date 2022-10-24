using ParserCore.Parsers.Wallhaven;
using ParserCore.Parsers.WallpapersWide;
using Parsers.HdWallpapers;
using Parsers.WallpaperAccess;
using System.Runtime.Versioning;

namespace ParserCore.Parsers
{
    [SupportedOSPlatform("windows")]
    public static class ParserFactory
    {
        public static ImageParser GetParser(string website, (int sp, int ep) points, string searchQuery, int imageCount = 0) => website switch
        {
            "wallhaven.cc" => new WallhavenParser(new WallhavenOptions
            {
                SearchQuery = searchQuery,
                StartPoint = points.sp,
                EndPoint = points.ep,
                ImageCount = imageCount
            }),
            "wallpaperswide.com" => new WallpapersWideParser(new WallpapersWideOptions
            {
                SearchQuery = searchQuery,
                StartPoint = points.sp,
                EndPoint = points.ep,
                ImageCount = imageCount
            }),
            "hdwallpapers.in" => new HdWallpapersParser(new HdWallpapersOptions
            {
                SearchQuery = searchQuery,
                StartPoint = points.sp,
                EndPoint = points.ep,
                ImageCount = imageCount
            }),
            "wallpaperaccess.com" => new WallpaperAccessParser(new WallpaperAccessOptions
            {
                SearchQuery= searchQuery,
                ImageCount = imageCount
            }),
            _ => throw new Exception("There is a problem occured while getting parser")
        };
    }
}
