using ParserCore;
using ParserCore.Helpers;

namespace ParserGui
{
    public partial class MainForm : Form
    {
        //TODO: Test and fix cancellation
        private CancellationTokenSource _cts = new();
        private readonly string[] _websites = new[] { "wallhaven.cc", "wallpaperswide.com" };
        private Progress<ProgressInfo> _mainProgress, _wallhavenProgress, _wallpapersWideProgress;
        public MainForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            websitesListBox.Items.AddRange(_websites);
            InitializeProgresses();
            HttpHelper.InitializeClientWithDefaultHeaders();
            
            //HttpHelper.InitializeClient("text/html");
        }
        private async void GoButtonCLick(object sender, EventArgs e)
        {
#if DEBUG
            var tasks = new List<Task<IEnumerable<string>>>()
            {
                ParserFactory.GetParser(_websites[0], (1, 3), "night city").Parse(_wallhavenProgress, _cts.Token),
                ParserFactory.GetParser(_websites[1], (1, 3), "night city").Parse(_wallpapersWideProgress, _cts.Token),
            };
            try
            {
                await RunTasksAsync(tasks);
            }
            catch (OperationCanceledException ex)
            {
                progressLabel.Text = ex.Message;
                textBoxStatus.Text = ex.Message;
                MessageBox.Show($"{ex.Message}", Text);
                return;
            }
            finally
            {
                foreach (var task in tasks) task.Dispose();
                _cts = new();
            }

#else
            try
            {
                var tasks = GetParseTasks();
                await RunTasksAsync(tasks);
            }
            catch (OperationCanceledException ex) { throw; }
            catch (Exception ex)
            {
                MessageBox.Show(Text, ex.Message);
                return;
            }
#endif
        }

        private List<Task<IEnumerable<string>>> GetParseTasks()
        {
            int[] range = rangeTextBox.Text.Replace(" ", "").Split('-').Select(s => int.Parse(s)).ToArray();
            (int sp, int ep) points = (range[0], range[1]);
            string searchQuery = searchTextBox.Text;

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
        private void InitializeProgresses()
        {
            _mainProgress = new(MainProgressChanged);
            _wallhavenProgress = new(WallhavenProgressChanged);
            _wallpapersWideProgress = new(WallpapersWideProgressChanged);
        }
        private void WallhavenProgressChanged(ProgressInfo e)
        {
            progressBar1.Value = e.Percentage;
            label4.Text = $"{e.Percentage} %";
            textBoxStatus.Text += $"\r\nWallhaven: {e.TextStatus}";
        }
        private void WallpapersWideProgressChanged(ProgressInfo e)
        {
            progressBar2.Value = e.Percentage;
            label5.Text = $"{e.Percentage} %";
            textBoxStatus.Text += $"\r\nWide: {e.TextStatus}";
        }

        private void TextBoxStatusTextChanged(object sender, EventArgs e)
        {
            textBoxStatus.SelectionStart = textBoxStatus.TextLength;
            textBoxStatus.ScrollToCaret();
        }

        private void MainProgressChanged(ProgressInfo e)
        {
            progressBar.Value = e.Percentage;
            progressLabel.Text = e.TextStatus;
        }

        private void CancelButtonOnClick(object sender, EventArgs e)
        {
            _cts.Cancel();
        }
    }
}