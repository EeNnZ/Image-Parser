using System.Drawing;
using System.Runtime.Versioning;

namespace ParserCore.Loaders
{
    [SupportedOSPlatform("windows")]
    public static class ImageLoader
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
        private static async Task DownloadImageAsync(string imageUrl, CancellationToken token)
        {
            //TODO: Cancellation
            string imgName = imageUrl.Split("/").Last();
            string imgPath = Path.Combine(WorkingDirectory, imgName);

            using var input = new FileStream(imgPath, FileMode.Create);
            await HttpHelper.GetFileAsync(imageUrl, input, token);
        }
        private static Bitmap GetImageFromBase64String(string source)
        {
            byte[] bytes = Convert.FromBase64String(source);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        public static async Task DownloadImagesAsync(IEnumerable<string> sources, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            var pInfo = new ProgressInfo() { TextStatus = "Downloading images" };
            int linksCount = sources.Count();
            int downloadedConut = 0;
            progress.Report(pInfo);

            bool isBase64Encoded = sources.FirstOrDefault()?.Length > 1000;
            ParallelLoopResult plr = default;
            try
            {
                await Task.Run(() =>
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
                                            try { DownloadImageAsync(source, token).RunSynchronously(); downloadedConut++; }
                                            catch (OperationCanceledException) { throw; }
                                        }
                                        else { GetImageFromBase64String(source); }
                                    }
                                    catch (AggregateException aex)
                                    {
                                        if (aex.InnerException is HttpRequestException httpEx)
                                        {
                                            pls.Break();
                                        }
                                    }

                                    pInfo.Percentage = (downloadedConut * 100) / linksCount;
                                    //TODO: Fix pInfo to handle base64 cases
                                    pInfo.ItemsProcessed.Add(source.Split("/").Last());
                                    progress.Report(pInfo);
                                });
                    }, token);
                if (!plr.IsCompleted) 
                {
                    try { await Task.Run(() => DownloadImages(sources, pInfo, progress, token), token); }
                    catch (OperationCanceledException) { throw; }
                }
            }
            catch (TaskCanceledException) { throw; }
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
            try
            {
                foreach (string source in sources)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < 5; i++)
                    {
                        try { DownloadImageAsync(source, token).RunSynchronously(); downloadedCount++; }
                        catch (OperationCanceledException) { throw; }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerException is HttpRequestException httpEx) { Thread.Sleep(REQ_TIMEOUT); continue; }
                            else throw;
                        }
                    }

                    pInfo.ItemsProcessed.Add(source);
                    pInfo.TextStatus = "Downloading images..";
                    pInfo.Percentage = (downloadedCount * 100) / urlsCount;
                    progress.Report(pInfo);
                }
            }
            catch (OperationCanceledException) { throw; }
        }

        private static bool ClearWorkingDirectory(string workingDirectory)
        {
            Directory.EnumerateFiles(workingDirectory).ToList().ForEach(file => File.Delete(file));
            return !Directory.EnumerateFiles(workingDirectory).Any();
        }
    }
}
