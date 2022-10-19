using MaterialSkin;
using MaterialSkin.Controls;
using ParserCore;
using ParserCore.Helpers;

namespace ParserGui
{
    public partial class MainForm : MaterialForm
    {
        private CancellationTokenSource _cts = new();
        private readonly string[] _websites = new[] { "wallhaven.cc", "wallpaperswide.com" };
        private readonly MaterialSkinManager _sm;
        private readonly Progress<ProgressInfo> _mainProgress, _wallhavenProgress, _wallpapersWideProgress;
        public MainForm()
        {
            InitializeComponent();
            _sm = MaterialSkinManager.Instance;
            _sm.AddFormToManage(this);
            _sm.ThemeChanged += SkinManagerThemeChanged;
            SetTheme();
            HttpHelper.InitializeClientWithDefaultHeaders();
            _mainProgress = new(MainProgressChanged);
            _wallhavenProgress = new(WallhavenProgressChanged);
            _wallpapersWideProgress = new(WallpapersWideProgressChanged);
        }

        private async Task DoWork()
        {
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
            string searchQuery = "night city";
#else
            int[] range = rangeTextBox.Text.Replace(" ", "").Split('-').Select(s => int.Parse(s)).ToArray();
            (int sp, int ep) points = (range[0], range[1]);
            string searchQuery = searchTextBox.Text;
#endif
            return new List<Task<IEnumerable<string>>>()
            {
                ParserFactory.GetParser(_websites[0], points, searchQuery).Parse(_wallhavenProgress, _cts.Token),
                ParserFactory.GetParser(_websites[1], points, searchQuery).Parse(_wallpapersWideProgress, _cts.Token),
            };
        }

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
        private void WallhavenProgressChanged(ProgressInfo e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar2.Value = e.Percentage;
            label5.Text = $"{e.Percentage} %";
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
            progressLabel.Text = e.TextStatus;
        }

        private void WallpapersWideProgressChanged(ProgressInfo e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar1.Value = e.Percentage;
            label4.Text = $"{e.Percentage} %";
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
            progressLabel.Text = e.TextStatus;
        }

        private void MainProgressChanged(ProgressInfo e)
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            foreach (var website in _websites)
            {
                websitesListBox.Items.Add(website); 
            }
        }
    }
}