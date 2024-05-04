namespace ParserGuiWpf.ViewModels;

public interface ISelectableWebsite
{
    /// <inheritdoc cref="SelectableWebsiteViewModel{TWebsite}._isSelected"/>
    bool IsSelected { get;         set; }
    double       Percentage { get; set; }
    Task         DownloadImages(int start, int end, string query, CancellationToken token);
    event Action ProgressValueChanged;
}