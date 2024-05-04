using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParserCore.Helpers;
using ParserCore.Models.Websites.ConcreteWebsites;
using ParserGuiWpf.Helpers;


namespace ParserGuiWpf.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly CancellationTokenSource _cts                 = new CancellationTokenSource();

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
        private IEnumerable<ISelectableWebsite>          SelectedWebsites => _selectableWebsites.Where(x => x.IsSelected);

        public MainViewModel()
        {
            HttpHelper.InitializeClientWithDefaultHeaders();

            _selectableWebsites = new ObservableCollection<ISelectableWebsite>
            {
                _wallhavenViewModel, _wallpapersWideViewModel, _hdWallpapersViewModel, _wallpaperAccesViewModel
            };
            SubscribeToSelectedProgressChanged();
        }

        private void SubscribeToSelectedProgressChanged()
        {
            foreach (var website in SelectedWebsites) 
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
                await DownloadImagesFromSelectedWebsites();
            }
            catch (OperationCanceledException)
            {
                await new CustomMessageBox().ShowCanceledDialog(_cts.Token);
            }
            catch (Exception ex)
            {
                await new CustomMessageBox($"{ex.Message}{Environment.NewLine}StackTrace:{ex.StackTrace}", "Uncaught exception")
                   .ShowDialogAsync(_cts.Token);
                Exit();
            }
        }

        private async Task DownloadImagesFromSelectedWebsites()
        {
            var tasks = GetSelectedWebsitesTasks();
            await Task.WhenAll(tasks);
        }

        private IEnumerable<Task> GetSelectedWebsitesTasks()
        {
            var tasks = SelectedWebsites
                       .Select(website => Task.Run(() => DownloadImages(website), _cts.Token))
                       .ToList();
            return tasks;
        }

        private Task DownloadImages(ISelectableWebsite website)
        {
            return website.DownloadImages(_startPointValue, _endPointValue, _searchTermValue,
                                          _cts.Token);
        }

        [RelayCommand]
        private void Cancel() => _cts.Cancel();

        [RelayCommand]
        private void OpenResults()
        {
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