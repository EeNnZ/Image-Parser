using MaterialSkin;
using MaterialSkin.Controls;
using ParserCore;
using ParserCore.Helpers;

namespace ParserGui
{
    public partial class MainForm : MaterialForm
    {
        MaterialSkinManager _sm;
        private CancellationTokenSource _cts = new();
        private readonly string[] _websites = new[] { "wallhaven.cc", "wallpaperswide.com" };
        private readonly Progress<ProgressInfo> _mainProgress, _wallhavenProgress, _wallpapersWideProgress;
        public MainForm()
        {
            InitializeComponent();
            _sm = MaterialSkinManager.Instance;
            _sm.AddFormToManage(this);
            //SetDarkTheme();
            SetLightTheme();
            HttpHelper.InitializeClientWithDefaultHeaders();
            _mainProgress = new(MainProgressChanged);
            _wallhavenProgress = new(WallhavenProgressChanged);
            _wallpapersWideProgress = new(WallpapersWideProgressChanged);
        }

        private void SetDarkTheme()
        {
            _sm.Theme = MaterialSkinManager.Themes.DARK;
            _sm.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue500, Accent.LightBlue200, TextShade.WHITE);
        }
        private void SetLightTheme()
        {
            _sm.Theme = MaterialSkinManager.Themes.LIGHT;
            _sm.ColorScheme = new ColorScheme(Primary.BlueGrey300, Primary.LightBlue900, Primary.Blue500, Accent.LightBlue200, TextShade.BLACK);
        }

        private async void GoButtonCLick(object sender, EventArgs e)
        {
            var tasks = GetParseTasks();
            try
            {
                await RunTasksAsync(tasks);
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

        private async Task RunTasksAsync(List<Task<IEnumerable<string>>> tasks)
        {
            var tResults = await Task.WhenAll(tasks)
                .ContinueWith(t => t.Result.SelectMany(res => res).ToArray())
                .ContinueWith(t => DownloadImagesAsync(t.Result));
            //var sources = tResults.SelectMany(res => res).ToArray();
        }

        private async Task DownloadImagesAsync(string[] sources)
        {
            if (sources == null) throw new NullReferenceException(nameof(sources));
            await ImageDownloader.DownloadAsync(sources, _mainProgress, _cts.Token);
        }

        private void WallhavenProgressChanged(ProgressInfo e)
        {
            //TODO: Fix progress reporting to statusTextBox
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar2.Value = e.Percentage;
            label5.Text = $"{e.Percentage} %";
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
        }

        private void WallpapersWideProgressChanged(ProgressInfo e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar1.Value = e.Percentage;
            label4.Text = $"{e.Percentage} %";
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
        }

        private void MainProgressChanged(ProgressInfo e)
        {
            if (e.Percentage > 100) e.Percentage = 100;
            progressBar.Visible = true;
            progressBar.Value = e.Percentage;
            progressLabel.Text = e.TextStatus;
            statusTextBox.Lines = e.ItemsProcessed.ToArray();
        }

        private void StatusTextBoxTextChanged(object sender, EventArgs e)
        {
            statusTextBox.SelectionStart = statusTextBox.Text.Length;
            statusTextBox.ScrollToCaret();
        }

        private void ThemeSwitcherCheckedChanged(object sender, EventArgs e)
        {
            if (themeSwitcher.Checked)
            {
                if (_sm.Theme == MaterialSkinManager.Themes.LIGHT)
                {
                    SetDarkTheme();
                    return;
                }
                else
                {
                    themeSwitcher.CheckState = CheckState.Unchecked;
                    return;
                }
            }
            else
            {
                if (_sm.Theme == MaterialSkinManager.Themes.DARK)
                {
                    SetLightTheme();
                    return;
                }
                else return;
            }
        }

        private void CancelButtonOnClick(object sender, EventArgs e)
        {
            _cts.Cancel();
        }

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