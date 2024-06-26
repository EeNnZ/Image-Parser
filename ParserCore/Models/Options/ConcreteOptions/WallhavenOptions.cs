﻿namespace ParserCore.Models.Options.ConcreteOptions;

internal class WallhavenOptions : IParserOptions
{
    public string BaseUrl => "https://wallhaven.cc/";
    public string PagePrefix => $"{SearchPrefix}{SearchQuery}&page=";
    public string SearchPrefix => "search?q=";
    public int StartPoint { get; set; }
    public int EndPoint { get; set; }
    public int ImageCount { get; set; }
    public string SearchQuery { get; set; } = "";
    public string? Resolution { get; set; }
}