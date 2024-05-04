using ParserCore.Models.Options;
using ParserCore.Models.Options.ConcreteOptions;
using ParserCore.Models.Websites;
using ParserCore.Models.Websites.ConcreteWebsites;

namespace ParserCore.Factory;

public class WebsiteFactory<TWebsite> where TWebsite : IWebsite
{
    private readonly Dictionary<Type, Type> _matchingDictionary = new()
    {
        { typeof(HdWallpapers), typeof(HdWallpapersOptions) },
        { typeof(Wallhaven), typeof(WallhavenOptions) },
        { typeof(WallpaperAccess), typeof(WallpaperAccessOptions) },
        { typeof(WallpaperFlare), typeof(WallpaperFlareOptions) },
        { typeof(WallpapersWide), typeof(WallpapersWideOptions) }
    };

    public IWebsite CreateWebsite((int startPoint, int endPoint) points, string searchQuery)
    {
        return CreateWebsite(CreateOptions(points, searchQuery)) ?? throw new InvalidOperationException();
    }

    private IWebsite? CreateWebsite(IParserOptions options)
    {
        return (IWebsite?)Activator.CreateInstance(typeof(TWebsite), options);
    }

    private IParserOptions CreateOptions((int startPoint, int endPoint) points, string searchQuery)
    {
        Type optionsToBeCreated = _matchingDictionary[typeof(TWebsite)];

        if (Activator.CreateInstance(optionsToBeCreated) is not IParserOptions options)
            throw new ArgumentNullException(nameof(options));

        options.StartPoint  = points.startPoint;
        options.EndPoint    = points.endPoint;
        options.SearchQuery = searchQuery;

        return options;
    }
}