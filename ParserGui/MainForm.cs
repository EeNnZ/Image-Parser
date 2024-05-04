using MaterialSkin.Controls;
using ParserCore.Downloaders;
using ParserCore.Helpers;
using System.Diagnostics;
using ParserCore.Factory;

namespace ParserGui;

public partial class MainForm : MaterialForm
{
    private CancellationTokenSource _cts = new();

    //TODO: Add new parser (https://wallpaperstock.net/)
    private readonly string[] _websites =
        { "wallpaperaccess.com", "wallhaven.cc", "wallpaperswide.com", "hdwallpapers.in" };

    private readonly Dictionary<string, Progress<ProgressChangedEventArgs>> _websiteProgressPairs;

    private readonly Progress<ProgressChangedEventArgs> _mainProgress;
    private readonly Progress<ProgressChangedEventArgs> _wallhavenProgress;
    private readonly Progress<ProgressChangedEventArgs> _wallpapersWideProgress;
    private readonly Progress<ProgressChangedEventArgs> _hdwallprogress;
    private readonly Progress<ProgressChangedEventArgs> _wallpaperAccessProgress;

    public MainForm()
    {
        InitializeComponent();


        _mainProgress            = new Progress<ProgressChangedEventArgs>(MainProgressChanged);
        _wallhavenProgress       = new Progress<ProgressChangedEventArgs>(WallhavenProgressChanged);
        _wallpapersWideProgress  = new Progress<ProgressChangedEventArgs>(WallpapersWideProgressChanged);
        _hdwallprogress          = new Progress<ProgressChangedEventArgs>(HdWallpapersProgressChanged);
        _wallpaperAccessProgress = new Progress<ProgressChangedEventArgs>(WallpaperAccessProgressChanged);

        _websiteProgressPairs = FillWebsiteProgressPairs();

        HttpHelper.InitializeClientWithDefaultHeaders();
#if DEBUG
        statusTextBox.Text = Environment.CurrentDirectory;
#endif
    }

    #region Methods

    private async Task DoWork()
    {
        if (await CheckConnection()) return;

        var parseTasks   = GetParseTasks();
        var results = await Task.WhenAll(parseTasks);

        string[] imageSources  = GetImageSources(results);
        var      downloadTasks = GetImageDownloadTasks(imageSources);

        await Task.WhenAll(downloadTasks);
    }
    private List<Task<IEnumerable<string>>> GetParseTasks()
    {
#if DEBUG
        int[]            range       = { 1, 7 };
        (int sp, int ep) points      = (range[0], range[1]);
        var              searchQuery = "forest";

        string[] checkedWebsites =
            websitesListBox.Items.Where(item => item.Checked).Select(item => item.Text).ToArray();

        var list = new List<Task<IEnumerable<string>>>();
        foreach (string? website in checkedWebsites)
        {
            var parsableWebsite   = new WebsiteFactory<>().CreateWebsite(website, points, searchQuery);
            var progress = _websiteProgressPairs[website];
            var task     = parsableWebsite.RetrieveDirectLinksToImages(_cts.Token, progress);
            list.Add(task);
        }
#else
            int[] range = rangeTextBox.Text.Replace(" ", "").Split('-').Select(s => int.Parse(s)).ToArray();
            (int sp, int ep) points = (range[0], range[1]);
            string searchQuery = searchTextBox.Text;
#endif
        return list;
    }
    private string[] GetImageSources(IEnumerable<string>[] results)
    {
        string[]? sources = results.SelectMany(x => x).ToArray();

        if (sources == null || !sources.Any())
            throw new NullReferenceException(nameof(sources));

        return sources;
    }

    private async Task<bool> CheckConnection()
    {
        if (!await ConnectionChecker.CheckIfConnected())
        {
            var dr = MessageBox.Show("You're not connected to the internet", Text, MessageBoxButtons.OK,
                                     MessageBoxIcon.Error);
            if (dr == DialogResult.OK) return true;
        }

        return false;
    }

    private List<Task> GetImageDownloadTasks(string[] sources)
    {
        var grouping = sources.GroupBy(source => source.Split("/")[2]);
        var tasks    = new List<Task>();

        foreach (var group in grouping)
            tasks.Add(ImageDownloader.DownloadImagesAsync(group.Skip(1), _mainProgress, _cts.Token));

        return tasks;
    }

    private Dictionary<string, Progress<ProgressChangedEventArgs>> FillWebsiteProgressPairs()
    {
        Dictionary<string, Progress<ProgressChangedEventArgs>>? result = new();

        foreach (string website in _websites) 
            ResolveWebsite(website, result);

        return result;
    }

    private void ResolveWebsite(string website, Dictionary<string, Progress<ProgressChangedEventArgs>> result)
    {
        if (website == "wallpaperaccess.com")
            result.Add(website, _wallpaperAccessProgress);

        else if (website == "wallhaven.cc")
            result.Add(website, _wallhavenProgress);

        else if (website == "wallpaperswide.com")
            result.Add(website, _wallpapersWideProgress);

        else if (website == "hdwallpapers.in")
            result.Add(website, _hdwallprogress);
    }

    #endregion

    #region Progress event handlers

    private void WallhavenProgressChanged(ProgressChangedEventArgs e)
    {
        if (e.Percentage > 100) e.Percentage = 100;
        progressBar3.Value  = e.Percentage;
        label6.Text         = $"{e.Percentage} %";
        statusTextBox.Lines = e.ItemsProcessed.ToArray();
        progressLabel.Text  = e.TextStatus;
    }

    private void WallpapersWideProgressChanged(ProgressChangedEventArgs e)
    {
        if (e.Percentage > 100) e.Percentage = 100;
        progressBar2.Value  = e.Percentage;
        label5.Text         = $"{e.Percentage} %";
        statusTextBox.Lines = e.ItemsProcessed.ToArray();
        progressLabel.Text  = e.TextStatus;
    }

    private void HdWallpapersProgressChanged(ProgressChangedEventArgs e)
    {
        if (e.Percentage > 100) e.Percentage = 100;
        progressBar1.Value  = e.Percentage;
        label4.Text         = $"{e.Percentage} %";
        statusTextBox.Lines = e.ItemsProcessed.ToArray();
        progressLabel.Text  = e.TextStatus;
    }

    private void MainProgressChanged(ProgressChangedEventArgs e)
    {
        if (e.Percentage > 100) e.Percentage = 100;
        progressBar.Visible = true;
        progressBar.Value   = e.Percentage;
        progressLabel.Text  = e.TextStatus;
        statusTextBox.Lines = e.ItemsProcessed.ToArray();
        progressLabel.Text  = e.TextStatus;
    }

    private void WallpaperAccessProgressChanged(ProgressChangedEventArgs e)
    {
        if (e.Percentage > 100)
            e.Percentage = 100;
        progressBar4.Value  = e.Percentage;
        label7.Text         = e.TextStatus;
        statusTextBox.Lines = e.ItemsProcessed.ToArray();
        progressLabel.Text  = e.TextStatus;
    }

    private void StatusTextBoxTextChanged(object sender, EventArgs e)
    {
        statusTextBox.SelectionStart = statusTextBox.Text.Length;
        statusTextBox.ScrollToCaret();
    }

    #endregion

    #region Button clicks

    private async void GoButtonClick(object sender, EventArgs e)
    {
        try
        {
            await DoWork();
        }
        catch (OperationCanceledException ex)
        {
            progressLabel.Text = ex.Message;
            statusTextBox.Text = ex.Message;
        }
        finally
        {
            _cts = new CancellationTokenSource();
        }
    }

    private void ExitButtonClick(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void OpenResFolderButtonClick(object sender, EventArgs e)
    {
        if (!Directory.Exists(ImageDownloader.WorkingDirectory))
        {
            MessageBox.Show("Folder is not created yet, download images first", "Error", MessageBoxButtons.OK);
            return;
        }

        Process.Start(new ProcessStartInfo()
        {
            FileName        = ImageDownloader.WorkingDirectory,
            UseShellExecute = true,
            Verb            = "open"
        });
    }

    private void CancelButtonClick(object sender, EventArgs e)
    {
        if (_cts.Token.CanBeCanceled)
        {
            _cts.Cancel();
        }
        else
        {
            var dialogResult = MessageBox.Show("Execution cannot ba canceled right now", Text,
                                               MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            if (dialogResult == DialogResult.Retry)
                CancelButtonClick(sender, e);
            else return;
        }
    }

    #endregion

    #region Overrides

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        foreach (string website in _websites) websitesListBox.Items.Add(website);
    }

    #endregion
}