using ParserCore.Parsers.Wallhaven;
using ParserCore.Parsers.WallpapersWide;
using Parsers.HdWallpapers;
using System.Runtime.Versioning;

namespace ParserCore.Parsers
{
    [SupportedOSPlatform("windows")]
    public static class ParserFactory
    {
        public static ImageParser GetParser(string website, (int sp, int ep) points, string searchQuery) => website switch
        {
            "wallhaven.cc" => new WallhavenParser(new WallhavenOptions
            {
                SearchQuery = searchQuery,
                StartPoint = points.sp,
                EndPoint = points.ep
            }),
            "wallpaperswide.com" => new WallpapersWideParser(new WallpapersWideOptions
            {
                SearchQuery = searchQuery,
                StartPoint = points.sp,
                EndPoint = points.ep
            }),
            "hdwallpapers.in" => new HdWallpapersParser(new HdWallpapersOptions
            {
                SearchQuery = searchQuery,
                StartPoint = points.sp,
                EndPoint = points.ep
            }),
            _ => throw new Exception("There is a problem occured while getting parser")
        };
    }
}
