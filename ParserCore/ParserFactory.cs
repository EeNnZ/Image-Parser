using ParserCore.Wallhaven;
using ParserCore.WallpapersWide;
using System.Runtime.Versioning;
using Wallhaven;

namespace ParserCore
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
            _ => throw new Exception("There is a problem occured while getting parser")
        };
    }
}
