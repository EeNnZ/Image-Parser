using MaterialSkin;
using MaterialSkin.Controls;
using ParserCore;
using ParserCore.Helpers;
using ParserCore.Parsers;
using System.Runtime.CompilerServices;
using Timer = System.Windows.Forms.Timer;

namespace ParserGui
{
    public partial class MainForm : MaterialForm
    {
        #region Fields
        private bool _connected = false;
        private CancellationTokenSource _cts = new();

        //TODO: Add new parsers (https://wallpaperaccess.com/, https://www.hdwallpapers.in/, https://wallpaperstock.net/)
        private readonly string[] _websites = new[] { "wallhaven.cc", "wallpaperswide.com", "hdwallpapers.in" };
        private readonly MaterialSkinManager _sm;
        private readonly Progress<ProgressChangedEventArgs> _mainProgress, _wallhavenProgress, _wallpapersWideProgress, _hdwallprogress;
        private readonly ConnectionChecker _connectionChecker = new(2000);
        private readonly Timer _timer; 
        #endregion
        public MainForm()
        {
            InitializeComponent();
            _sm = MaterialSkinManager.Instance;
            _sm.AddFormToManage(this);
            _sm.ThemeChanged += SkinManagerThemeChanged;
            SetTheme();
            _mainProgress = new(MainProgressChanged);
            _wallhavenProgress = new(WallhavenProgressChanged);
            _wallpapersWideProgress = new(WallpapersWideProgressChanged);
            _hdwallprogress = new(HdWallpapersProgressChanged);
            _timer = new() { Interval = _connectionChecker.CheckInterval };
            _timer.Tick += CheckConnection;
            _timer.Start();
            HttpHelper.InitializeClientWithDefaultHeaders();
        }
        #region Methods
        private async Task DoWork()
        {
            if (!_connected)
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
                ParserFactory.GetParser(_websites[2], points, searchQuery).Parse(_hdwallprogress, _cts.Token)
            };
        }
        private void CheckConnection(object? sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                bool connected = await _connectionChecker.CheckIfConnected();
                progressLabel.Invoke(() =>
                {
                    progressLabel.Text = connected ? "Connected" : "Looks like you're not connected to the internet";
                });
                _connected = connected;
            });
        } 
        #endregion

        #region Theme control
        private void SetTheme()
        {
            if (WinThemeDetector.ShouldUseDarkMode()) SetDarkTheme();
            else SetLightTheme();
        }

        private void SkinManagerThemeChanged(object sender)
        {
            themeSwitcher.CheckState = _sm.Theme == MaterialSkinManager.Themes.LIGHT ? CheckState.Unchecked : CheckState.Checked;
        }
        private void ThemeSwitcherStateChanged(object sender, EventArgs e)
        {
            if (themeSwitcher.CheckState == CheckState.Checked) SetDarkTheme();
            else SetLightTheme();
        }

        private void SetDarkTheme()
        {
            _sm.Theme = MaterialSkinManager.Themes.DARK;
            _sm.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue500, Accent.LightBlue200, TextShade.WHITE);
        }
        private void SetLightTheme()
        {
            _sm.Theme = MaterialSkinManager.Themes.LIGHT;
            _sm.ColorScheme = new ColorScheme(Primary.Grey300, Primary.DeepPurple500, Primary.DeepPurple900, Accent.DeepPurple700, TextShade.BLACK);
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

        private void StatusTextBoxTextChanged(object sender, EventArgs e)
        {
            statusTextBox.SelectionStart = statusTextBox.Text.Length;
            statusTextBox.ScrollToCaret();
        }
        #endregion

        #region Button clicks
        private async void GoButtonCLick(object sender, EventArgs e)
        {
            await DoWork();
        }
        private void ExitButtonClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void CancelButtonClick(object sender, EventArgs e)
        {
            if (_cts.Token.CanBeCanceled)
            {
                _cts.Cancel();
            }
            else
            {
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
            _timer.Dispose();
            base.OnFormClosing(e); 
            #endregion
        }
    }
}