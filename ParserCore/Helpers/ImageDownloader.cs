using ParserCore.Parsers;
using System.Drawing;
using System.Runtime.Versioning;

namespace ParserCore.Helpers
{
    [SupportedOSPlatform("windows")]
    public static class ImageDownloader
    {
        public const int REQ_TIMEOUT = 2000;
        public const int RETRIES_COUNT = 5;

        public static string WorkingDirectory
        {
            get
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string resultsFolderName = $"imgParser{DateTime.Now.ToShortDateString()}";
                var dir = Directory.CreateDirectory(Path.Combine(desktop, resultsFolderName));
                return dir.FullName;
            }
        }
        private static bool DownloadImageAsync(string imageUrl, CancellationToken token)
        {
            string[] urlParts = imageUrl.Split("/");
            string imgName = urlParts.Last();
            string website = urlParts[2];
            string localWorkingDirectory = Path.Combine(WorkingDirectory, website);

            if (!Directory.Exists(localWorkingDirectory))
            {
                Directory.CreateDirectory(localWorkingDirectory);
            }

            string imgPath = Path.Combine(localWorkingDirectory, imgName);
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using var input = new FileStream(imgPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    HttpHelper.GetFileAsync(imageUrl, input, token).Wait(token);
                    break;
                }
                catch (IOException ex)
                {
                    //TODO: Handle case when file being used by another process
                    //TODO: Use win32 to find process? Any managed api there?
                }
            }
            return true;
        }
        private static bool GetImageFromBase64String(string source)
        {
            byte[] bytes = Convert.FromBase64String(source);
            using var stream = new MemoryStream(bytes);
            var decoded = new Bitmap(stream);
            //TODO: Save decoded to file
            return true;
        }
        public static async Task DownloadAsync(IEnumerable<string> sources, IProgress<ProgressChangedEventArgs> progress, CancellationToken token)
        {
            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Downloading images" };
            int linksCount = sources.Count();
            int downloadedConut = 0;
            progress.Report(pInfo);

            bool isBase64Encoded = sources.FirstOrDefault()?.Length > 1000;
            ParallelLoopResult plr = default;
            await Task.Run(() =>
                {
                    try
                    {
                        plr = Parallel.ForEach(sources, new ParallelOptions()
                        {
                            CancellationToken = token,
                            MaxDegreeOfParallelism = Environment.ProcessorCount
                        }, (source, pls) =>
                                {
                                    try
                                    {
                                        if (!isBase64Encoded)
                                        {
                                            _ = DownloadImageAsync(source, token); downloadedConut++;
                                        }
                                        else { _ = GetImageFromBase64String(source); }
                                    }
                                    catch (AggregateException)
                                    {
                                        pls.Break();
                                    }

                                    pInfo.Percentage = downloadedConut * 100 / linksCount;
                                    //TODO: Fix pInfo to handle base64 cases
                                    pInfo.ItemsProcessed.Add(source.Split("/").Last());
                                    progress.Report(pInfo);
                                });
                    }
                    catch { }
                }, token);
            if (!plr.IsCompleted && !token.IsCancellationRequested)
            {
                await Task.Run(() => DownloadImages(sources, progress, token), token);
            }
            pInfo.TextStatus = "Done";
            pInfo.Percentage = 100;
            progress.Report(pInfo);
        }

        private static void DownloadImages(IEnumerable<string> sources, IProgress<ProgressChangedEventArgs> progress, CancellationToken token)
        {
            int downloadedCount = 0;
            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Download restarted sequentially" };
            //_ = ClearWorkingDirectory();
            progress.Report(pInfo);
            int urlsCount = sources.Count();

            try
            {
                foreach (string source in sources)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < RETRIES_COUNT; i++)
                    {
                        try
                        {
                            _ = DownloadImageAsync(source, token);
                            downloadedCount++;
                        }
                        catch (AggregateException ex) when (ex.InnerException is HttpRequestException)
                        {
                            Thread.Sleep(REQ_TIMEOUT); continue;
                        }
                    }
                    pInfo.ItemsProcessed.Add(source);
                    pInfo.Percentage = downloadedCount * 100 / urlsCount;
                    progress.Report(pInfo);
                }
            }
            catch (OperationCanceledException)
            {

            }
        }
        private static bool ClearWorkingDirectory()
        {
            Directory.EnumerateFiles(WorkingDirectory).ToList().ForEach(file => File.Delete(file));
            return !Directory.EnumerateFiles(WorkingDirectory).Any();
        }
    }
}
