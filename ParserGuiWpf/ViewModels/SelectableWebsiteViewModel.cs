using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ParserCore.Downloaders;
using ParserCore.Factory;
using ParserCore.Helpers;
using ParserCore.Models.Websites;

namespace ParserGuiWpf.ViewModels;

public partial class SelectableWebsiteViewModel<TWebsite> : ObservableObject, ISelectableWebsite where TWebsite : IWebsite
{
    private readonly IWebsite _website;

    public IProgress<ProgressChangedEventArgs> Progress                 { get; private set; }
    public ObservableCollection<string>        ItemsProcessedCollection { get; set; }
    public ObservableCollection<string>        ItemsFailedCollection    { get; set; }

    public  string  Name => _website.Name;

    [ObservableProperty] private bool    _isSelected;
    [ObservableProperty] private string? _textStatus;
    [ObservableProperty] private double  _percentage;

    public event Action? ProgressValueChanged;

    public SelectableWebsiteViewModel()
    {
        _website = new WebsiteFactory<TWebsite>().CreateWebsite((0,1), "default");

        ItemsProcessedCollection = new ObservableCollection<string>();
        ItemsFailedCollection = new ObservableCollection<string>();

        Progress = new Progress<ProgressChangedEventArgs>(OnProgressReportedHandler);
    }

    partial void OnPercentageChanged(double value)
    {
        ProgressValueChanged?.Invoke();
    }

    public async Task DownloadImages(int start, int end, string query, CancellationToken token)
    {
        _website.Parser.Options.StartPoint = start;
        _website.Parser.Options.EndPoint = end;
        await ImageDownloader.DownloadImagesFromWebsiteAsync(_website, Progress, token);
    }

    private void OnProgressReportedHandler(ProgressChangedEventArgs args)
    {
        TextStatus               = args.TextStatus;
        Percentage               = args.Percentage;

        ItemsProcessedCollection = new ObservableCollection<string>(args.ItemsProcessed);
        ItemsFailedCollection = new ObservableCollection<string>(args.ItemsFailed);
    }
}