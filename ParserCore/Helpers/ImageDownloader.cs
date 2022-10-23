using Helpers;
using ParserCore.Parsers;
using Serilog;
using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        private static bool DownloadImageAsync(string imageUrl, CancellationToken token, [CallerMemberName]string callerName = "")
        {
            Log.Information("Thread: {ThreadId} with caller: {Caller} entered to {MethodName}",
                             Environment.CurrentManagedThreadId,
                             callerName,
                             MethodBase.GetCurrentMethod()?.Name);
            string[] urlParts = imageUrl.Split("/");
            string imgName = urlParts.Last();
            string website = urlParts[2];
            string localWorkingDirectory = Path.Combine(WorkingDirectory, website);

            if (!Directory.Exists(localWorkingDirectory))
            {
                Log.Information("Сreating directory {dir}", localWorkingDirectory);
                try
                {
                    Directory.CreateDirectory(localWorkingDirectory);
                }
                catch(Exception ex)
                {
                    Log.Error("Exception caught while creating directory {exception}", ex.Message);
                }
            }

            string imgPath = Path.Combine(localWorkingDirectory, imgName);
            bool fileLocked = CheckIfFileLockedByAnotherProcess(imgPath, out IEnumerable<Process> lockers);
            if (fileLocked)
            {
                Log.Information("Cannot download {imgFilePath} because of file is being locked by one or more processes ({firstLockerInCollection})"
                    , imgPath
                    , lockers.First());
                return false;
            }
            Log.Information("Downloading image {imageUrl} to {imagePath}", imgName, imgPath);
            for (int i = 0; i < RETRIES_COUNT; i++)
            {
                Log.Debug("{counter} try", i);
                try
                {
                    using var input = new FileStream(imgPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    HttpHelper.GetFileAsync(imageUrl, input, token).Wait(token);
                    Log.Debug("{counter} try successfull", i);
                    break;
                }
                catch (IOException ex)
                {
                    Log.Error("Exception caught {exception}", ex.Message);
                    continue;
                    /*if (!lockers.Any()) throw;
                    foreach (var locker in lockers)
                    {
                        Log.Information("Found {lockersCount} lockers");
                        bool closeMessageSent = locker.CloseMainWindow();
                        if (closeMessageSent)
                        {
                            Log.Information("Close request sent to {lockerProcess}", locker.ProcessName);
                            bool exited = locker.WaitForExit(30000);
                            if (exited)
                            {
                                Log.Information("{lockerProcess} terminated by user, retrying to save image", locker.ProcessName);
                                continue;
                            }
                            else
                            {
                                Log.Information("{lockerProcess} not terminated by user, killing", locker.ProcessName);
                                locker.Kill();
                                continue;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }*/
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
        public static async Task DownloadAsync(IEnumerable<string> sources,
                                               IProgress<ProgressChangedEventArgs> progress,
                                               CancellationToken token,
                                               [CallerMemberName]string callerName = "")
        {
            Log.Information("Thread: {ThreadId} with caller: {Caller} entered to {MethodName}",
                             Environment.CurrentManagedThreadId,
                             callerName,
                             MethodBase.GetCurrentMethod()?.Name);
            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Downloading images" };
            int linksCount = sources.Count();
            int downloadedConut = 0;
            progress.Report(pInfo);

            bool isBase64Encoded = sources.FirstOrDefault()?.Length > 1000;
            ParallelLoopResult plr = default;
            Log.Information("Downloading images in parallel");
            Log.Information("Parallel download task is about to run in {ClassName}->{MethodName}",
                            typeof(ImageDownloader).Name,
                            MethodBase.GetCurrentMethod()?.Name);
            await Task.Run(() =>
                {
                    Log.Information("Task: {TaskId} started on thread: {ThreadId}",
                                    Task.CurrentId,
                                    Environment.CurrentManagedThreadId);
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
                                    catch (AggregateException aex)
                                    {
                                        Log.Error("ArrgregateException with inner {InnerException} caught inside parallel ForEach with message: {InnerExMessage}",
                                                  aex.InnerException,
                                                  aex.InnerException?.Message);
                                        pls.Break();
                                        Log.Information("Parallel loop break");
                                    }

                                    pInfo.Percentage = downloadedConut * 100 / linksCount;
                                    //TODO: Fix pInfo to handle base64 cases
                                    pInfo.ItemsProcessed.Add(source.Split("/").Last());
                                    progress.Report(pInfo);
                                });
                    }
                    catch (Exception ex) { Log.Information("Exception {Exception} with message {ExMessage} caught in ParallelForEach task", ex, ex.Message); };
                }, token);
            if (!plr.IsCompleted && !token.IsCancellationRequested)
            {
                Log.Information("ParallelForEach is not completed and cancellation is not requested");
                await Task.Run(() => DownloadImages(sources, progress, token), token);
            }
            Log.Information("Thread: {ThreadId} with caller: {Caller} is about to exit from {MethodName}",
                Environment.CurrentManagedThreadId, callerName, MethodBase.GetCurrentMethod()?.Name);
            pInfo.TextStatus = "Done";
            pInfo.Percentage = 100;
            progress.Report(pInfo);
        }

        private static void DownloadImages(IEnumerable<string> sources,
                                           IProgress<ProgressChangedEventArgs> progress,
                                           CancellationToken token,
                                           [CallerMemberName]string callerName = "")
        {
            Log.Information("Thread: {ThreadId} with caller: {Caller} entered to {MethodName}",
                Environment.CurrentManagedThreadId, callerName, MethodBase.GetCurrentMethod()?.Name);
            int downloadedCount = 0;
            var pInfo = new ProgressChangedEventArgs() { TextStatus = "Download restarted sequentially" };
            //_ = ClearWorkingDirectory();
            progress.Report(pInfo);
            int urlsCount = sources.Count();

            try
            {
                Log.Information("Sequential images download started");
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
                        catch (AggregateException aex) when (aex.InnerException is HttpRequestException)
                        {
                            Log.Error("ArrgregateException with inner {InnerException} caught inside foreach loop with message: {InnerExMessage}",
                                      aex.InnerException,
                                      aex.InnerException.Message);
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
                Log.Error("Images download canceled");
            }
        }
        private static bool ClearWorkingDirectory()
        {
            Directory.EnumerateFiles(WorkingDirectory).ToList().ForEach(file => File.Delete(file));
            return !Directory.EnumerateFiles(WorkingDirectory).Any();
        }

        private static bool CheckIfFileLockedByAnotherProcess(string imgPath, out IEnumerable<Process> lockers , [CallerMemberName] string callerName = "")
        {
            Log.Debug("Thread: {ThreadId} with caller: {Caller} entered to {MethodName}",
                Environment.CurrentManagedThreadId, callerName, MethodBase.GetCurrentMethod()?.Name);

            lockers = FileUtil.WhoIsLocking(imgPath);
            bool locked = lockers.Any();
            return locked;
        }
    }
}
