using System.Drawing;
using System.Runtime.Versioning;

namespace ParserCore.Helpers
{
    [SupportedOSPlatform("windows")]
    public static class ImageDownloader
    {
        private const int REQ_TIMEOUT = 2000;
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
        private static async Task<bool> DownloadImageAsync(string imageUrl, CancellationToken token)
        {
            string imgName = imageUrl.Split("/").Last();
            string imgPath = Path.Combine(WorkingDirectory, imgName);

            using var input = new FileStream(imgPath, FileMode.Create);
            await HttpHelper.GetFileAsync(imageUrl, input, token);
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
        public static async Task DownloadAsync(IEnumerable<string> sources, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            var pInfo = new ProgressInfo() { TextStatus = "Downloading images" };
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
                                            _ = DownloadImageAsync(source, token).Result; downloadedConut++;
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
                await Task.Run(() => DownloadImages(sources, pInfo, progress, token), token);
            }
            pInfo.TextStatus = "Done";
            pInfo.Percentage = 100;
            progress.Report(pInfo);
        }

        private static void DownloadImages(IEnumerable<string> sources, ProgressInfo pInfo, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            int downloadedCount = 0;
            pInfo.ItemsProcessed.Clear();
            _ = ClearWorkingDirectory(WorkingDirectory);
            pInfo.TextStatus = "Download will be restarted synchronously";
            progress.Report(pInfo);
            int urlsCount = sources.Count();

            foreach (string source in sources)
            {
                token.ThrowIfCancellationRequested();
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        DownloadImageAsync(source, token).RunSynchronously(); downloadedCount++;
                    }
                    catch (AggregateException ex) when (ex.InnerException is HttpRequestException)
                    {
                        Thread.Sleep(REQ_TIMEOUT); continue;
                    }
                }

                pInfo.ItemsProcessed.Add(source);
                pInfo.TextStatus = "Downloading images..";
                pInfo.Percentage = downloadedCount * 100 / urlsCount;
                progress.Report(pInfo);
            }
        }
        private static bool ClearWorkingDirectory(string workingDirectory)
        {
            Directory.EnumerateFiles(workingDirectory).ToList().ForEach(file => File.Delete(file));
            return !Directory.EnumerateFiles(workingDirectory).Any();
        }
    }
}
