using MaterialSkin;
using MaterialSkin.Controls;
using ParserCore;
using ParserCore.Helpers;
using ParserCore.Parsers;
using Serilog;
using System;
using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace ParserGui
{
    public partial class MainForm : MaterialForm
    {
        #region Fields
        private CancellationTokenSource _cts = new();
        private readonly string _logFilePath = Path.Combine(Environment.CurrentDirectory, "log.txt");

        //TODO: Add new parsers (https://wallpaperaccess.com/, https://wallpaperstock.net/)
        private readonly string[] _websites = new[] { "wallpaperaccess.com", "wallhaven.cc", "wallpaperswide.com", "hdwallpapers.in" };
        private readonly MaterialSkinManager _sm;
        private readonly Progress<ProgressChangedEventArgs> _mainProgress,
            _wallhavenProgress,
            _wallpapersWideProgress,
            _hdwallprogress,
            _wallpaperAccessProgress;
        #endregion
        public MainForm()
        {
            InitializeComponent();
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(_logFilePath,
                outputTemplate: "{Timestamp: HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            Log.Information("Initializing skin manager");
            _sm = MaterialSkinManager.Instance;
            _sm.AddFormToManage(this);
            _sm.ThemeChanged += SkinManagerThemeChanged;
            SetTheme();
            Log.Information("Skin manager initialized");

            Log.Information("Initializing progresses");
            _mainProgress = new(MainProgressChanged);
            _wallhavenProgress = new(WallhavenProgressChanged);
            _wallpapersWideProgress = new(WallpapersWideProgressChanged);
            _hdwallprogress = new(HdWallpapersProgressChanged);
            _wallpaperAccessProgress = new(WallpaperAccessProgressChanged);
            Log.Information("Progresses initialized");

            Log.Information("Initializing timer");
            Log.Information("Timer initialized and started");

            HttpHelper.InitializeClientWithDefaultHeaders();
#if DEBUG
            statusTextBox.Text = Environment.CurrentDirectory;
#endif
        }
        #region Methods
        private async Task DoWork()
        {
            if (await ConnectionChecker.CheckIfConnected())
            {
                var dr = MessageBox.Show("You're not connected to the internet", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (dr == DialogResult.OK) return;
            }
            var tasks = GetParseTasks();
            try
            {
                var results = await Task.WhenAll(tasks);
                var sources = results.SelectMany(x => x).ToArray();
                if (sources == null) throw new NullReferenceException(nameof(sources));

                await ImageDownloader.DownloadAsync(sources, _mainProgress, _cts.Token);
                //await RunTasksAsync(tasks);
            }
            catch (OperationCanceledException ex)
            {
                progressLabel.Text = ex.Message;
                statusTextBox.Text = ex.Message;
                //MaterialMessageBox.Show($"{ex.Message}", Text);

                return;
            }
            finally
            {
                foreach (var task in tasks) task.Dispose();
                _cts = new();
            }
        }

        private List<Task<IEnumerable<string>>> GetParseTasks()
        {
#if DEBUG
            int[] range = { 1, 3 };
            (int sp, int ep) points = (range[0], range[1]);
            string searchQuery = "forest road";
#else
            int[] range = rangeTextBox.Text.Replace(" ", "").Split('-').Select(s => int.Parse(s)).ToArray();
            (int sp, int ep) points = (range[0], range[1]);
            string searchQuery = searchTextBox.Text;
#endif
            return new List<Task<IEnumerable<string>>>()
            {
                //ParserFactory.GetParser(_websites[0], points, searchQuery).Parse(_wallhavenProgress, _cts.Token),
                //ParserFactory.GetParser(_websites[1], points, searchQuery).Parse(_wallpapersWideProgress, _cts.Token),
                //ParserFactory.GetParser(_websites[2], points, searchQuery).Parse(_hdwallprogress, _cts.Token)
                ParserFactory.GetParser(_websites[0], points, searchQuery, 50).Parse(_wallpaperAccessProgress, _cts.Token)
            };
        }
        #endregion

        #region Theme control
        private void SetTheme()
        {
            if (WinThemeDetector.ShouldUseDarkMode()) SetDarkTheme();
            else SetLightTheme();
            Log.Information("Theme is set up");
        }

        private void SkinManagerThemeChanged(object sender)
        {
            themeSwitcher.CheckState = _sm.Theme == MaterialSkinManager.Themes.LIGHT ? CheckState.Unchecked : CheckState.Checked;
        }
        private void ThemeSwitcherStateChanged(object sender, EventArgs e)
        {
            if (themeSwitcher.CheckState == CheckState.Checked)
            {
                SetDarkTheme();
                Log.Debug("Theme changed to dark");
            }
            else
            {
                SetLightTheme();
                Log.Debug("Theme changed to light");
            }
        }

        private void SetDarkTheme()
        {
            _sm.Theme = MaterialSkinManager.Themes.DARK;
            _sm.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue500, Accent.LightBlue200, TextShade.WHITE);
        }
        private void SetLightTheme()
        {
            _sm.Theme = MaterialSkinManager.Themes.LIGHT;
            _sm.ColorScheme = new ColorScheme(Primary.DeepPurple500, Primary.DeepPurple500, Primary.DeepPurple900, Accent.DeepPurple700, TextShade.WHITE);
            statusTextBox.BorderStyle = BorderStyle.FixedSingle;
        }
        #endregion

        #region Progress event handlers
        private void WallhavenProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar3.Value = e.Percentage;
            label6.Text = $"{e.Percentage} %";
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
            progressLabel.Text = e.TextStatus;
        }

        private void WallpapersWideProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar2.Value = e.Percentage;
            label5.Text = $"{e.Percentage} %";
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
            progressLabel.Text = e.TextStatus;
        }

        private void HdWallpapersProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar1.Value = e.Percentage;
            label4.Text = $"{e.Percentage} %";
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
            progressLabel.Text = e.TextStatus;
        }

        private void MainProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar.Visible = true;
            progressBar.Value = e.Percentage;
            progressLabel.Text = e.TextStatus;
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
            progressLabel.Text = e.TextStatus;
        }

        private void WallpaperAccessProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar4.Value = e.Percentage;
            label7.Text = e.TextStatus;
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
            progressLabel.Text = e.TextStatus;
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
            await DoWork();
        }
        private void ExitButtonClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenLogFileButtonClick(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(_logFilePath) { UseShellExecute = true });
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            Log.Information("Cancel button clicked");
            if (_cts.Token.CanBeCanceled)
            {
                Log.Debug("Token is able to be canceled, cancellation requested");
                Log.Information("Cancellation requested");
                _cts.Cancel();
            }
            else
            {
                Log.Error("Cancellation denied");
                var dialogResult = MessageBox.Show("Execution cannot ba canceled right now", Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dialogResult == DialogResult.Retry)
                {
                    CancelButtonClick(sender, e);
                }
                else return;
            }
        }
        #endregion

        #region Overrides
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            foreach (var website in _websites)
            {
                websitesListBox.Items.Add(website);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Log.Information("Closing application");
            Log.CloseAndFlush();
            base.OnFormClosing(e);
            #endregion
        }
    }
}