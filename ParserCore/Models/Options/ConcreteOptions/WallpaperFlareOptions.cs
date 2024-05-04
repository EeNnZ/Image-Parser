﻿namespace ParserCore.Models.Options.ConcreteOptions;

internal class WallpaperFlareOptions : IParserOptions
{
    public string BaseUrl => "https://www.wallpaperflare.com/";
    public string PagePrefix => $"{SearchPrefix}{SearchQuery}&page=";
    public string SearchPrefix => "search?wallpaper=";
    public int StartPoint { get; set; }
    public int EndPoint { get; set; }
    public int ImageCount { get; set; }
    public string SearchQuery { get; set; } = "";
    public string? Resolution { get; set; }
}