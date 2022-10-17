using MaterialSkin;
using MaterialSkin.Controls;
using ParserCore;
using ParserCore.Helpers;

namespace ParserGui
{
    public partial class MainForm : MaterialForm
    {
        //TODO: Test and fix cancellation
        private CancellationTokenSource _cts = new();
        private readonly string[] _websites = new[] { "wallhaven.cc", "wallpaperswide.com" };
        private readonly Progress<ProgressInfo> _mainProgress, _wallhavenProgress, _wallpapersWideProgress;
        public MainForm()
        {
            InitializeComponent();
            SetDarkTheme();

            HttpHelper.InitializeClientWithDefaultHeaders();
            _mainProgress = new(MainProgressChanged);
            _wallhavenProgress = new(WallhavenProgressChanged);
            _wallpapersWideProgress = new(WallpapersWideProgressChanged);
        }

        private void SetDarkTheme()
        {
            var sm = MaterialSkinManager.Instance;
            sm.AddFormToManage(this);
            sm.Theme = MaterialSkinManager.Themes.DARK;
            sm.ColorScheme = new ColorScheme(Primary.Blue800, Primary.Blue900, Primary.Blue500, Accent.LightBlue200, TextShade.WHITE);
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
            string searchQuery = "red flower";
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
            var tResults = await Task.WhenAll(tasks);
            var sources = tResults.SelectMany(res => res).ToArray();
            await DownloadImagesAsync(sources);
        }

        private async Task DownloadImagesAsync(string[] sources)
        {
            if (sources == null) throw new NullReferenceException(nameof(sources));
            await ImageDownloader.DownloadAsync(sources, _mainProgress, _cts.Token);
        }
        private void WallhavenProgressChanged(ProgressInfo e)
        {
            progressBar1.Value = e.Percentage;
            label4.Text = $"{e.Percentage} %";
            statusTextBox.Text += $"\r\nWallhaven: {e.TextStatus}";
        }
        private void WallpapersWideProgressChanged(ProgressInfo e)
        {
            progressBar2.Value = e.Percentage;
            label5.Text = $"{e.Percentage} %";
            statusTextBox.Text += $"\r\nWide: {e.TextStatus}";
        }

        private void MainProgressChanged(ProgressInfo e)
        {
            progressBar.Value = e.Percentage;
            progressLabel.Text = e.TextStatus;
        }

        private void StatusTextBoxTextChanged(object sender, EventArgs e)
        {
            statusTextBox.SelectionStart = statusTextBox.Text.Length;
            statusTextBox.ScrollToCaret();
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