namespace ParserCore.Interfaces
{
    //TODO: Use abstract options instead?
    public interface IParserOptions
    {
        string BaseUrl { get; }
        string PagePrefix { get; }
        string SearchPrefix { get; }
        int StartPoint { get; set; }
        int EndPoint { get; set; }
        string SearchQuery { get; set; }
        string? Resolution { get; set; }
    }
}
