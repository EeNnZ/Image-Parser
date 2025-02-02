using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParserCore.Downloaders;
using ParserCore.Helpers;
using ParserCore.Models.Websites.ConcreteWebsites;
using ParserCore.Tools;
using ParserGuiWpf.Helpers;


namespace ParserGuiWpf.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private CancellationTokenSource Cts => new();

        [ObservableProperty] private string _logText         = string.Empty;
        [ObservableProperty] private string _searchTermValue = string.Empty;
        [ObservableProperty] private int    _imagesCountValue;
        [ObservableProperty] private int    _startPointValue;
        [ObservableProperty] private int    _endPointValue;
        [ObservableProperty] private double _commonProgress;

        [ObservableProperty] private SelectableWebsiteViewModel<Wallhaven>       _wallhavenViewModel      = new();
        [ObservableProperty] private SelectableWebsiteViewModel<WallpapersWide>  _wallpapersWideViewModel = new();
        [ObservableProperty] private SelectableWebsiteViewModel<HdWallpapers>    _hdWallpapersViewModel   = new();
        [ObservableProperty] private SelectableWebsiteViewModel<WallpaperAccess> _wallpaperAccesViewModel = new();

        private readonly ObservableCollection<ISelectableWebsite> _selectableWebsites;
        private IEnumerable<ISelectableWebsite> SelectedWebsites => _selectableWebsites.Where(x => x.IsSelected);

        public MainViewModel()
        {
            HttpHelper.InitializeClientWithDefaultHeaders();
            WrappedCall.SetHandler(async void (mes) =>
            {
                try
                {
                    await new CustomMessageBox(mes).ShowDialogAsync(Cts.Token);
                }
                catch (Exception e)
                {
                    Application.Current.Shutdown();
                }
            });

            _selectableWebsites = new ObservableCollection<ISelectableWebsite>
            {
                _wallhavenViewModel, _wallpapersWideViewModel, _hdWallpapersViewModel, _wallpaperAccesViewModel
            };
            SubscribeToSelectedProgressChanged();
        }

        private void SubscribeToSelectedProgressChanged()
        {
            foreach (ISelectableWebsite website in SelectedWebsites)
                website.ProgressValueChanged += UpdateCommonProgress;
        }

        private void UpdateCommonProgress()
        {
            CommonProgress = SelectedWebsites.Sum(w => w.Percentage) / SelectedWebsites.Count();
        }

        [RelayCommand]
        private async Task Go()
        {
            SearchTermValue = "city";
            StartPointValue = 1;
            EndPointValue   = 4;
            SubscribeToSelectedProgressChanged();

            try
            {
                var token = Cts.Token;
                await DownloadImagesFromSelectedWebsites(token);
            }
            catch (OperationCanceledException)
            {
                await new CustomMessageBox().ShowCanceledDialog(Cts.Token);
            }
            catch (Exception ex)
            {
                await new CustomMessageBox($"{ex.Message}{Environment.NewLine}StackTrace:{ex.StackTrace}",
                                           "Exception occured")
                   .ShowDialogAsync(Cts.Token);
                Exit();
            }
        }

        private async Task DownloadImagesFromSelectedWebsites(CancellationToken token)
        {
            var tasks = GetSelectedWebsitesTasks(token);
            await Task.WhenAll(tasks);
        }

        private IEnumerable<Task> GetSelectedWebsitesTasks(CancellationToken token)
        {
            var tasks = SelectedWebsites
                       .Select(website => Task.Run(() => DownloadImages(website, token), token))
                       .ToList();
            return tasks;
        }

        private Task DownloadImages(ISelectableWebsite website, CancellationToken token)
        {
            return website.DownloadImages(StartPointValue, EndPointValue, SearchTermValue,
                                          token);
        }

        [RelayCommand]
        private void Cancel() => Cts.Cancel();

        [RelayCommand]
        private void OpenResults()
        {
            WrappedCall.Action(() => Process.Start("explorer.exe", ImageDownloader.WorkingDirectory));
        }

        [RelayCommand]
        private void OpenLog()
        {
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}