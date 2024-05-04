using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using ParserCore.Helpers;
using ParserCore.Models.Parsers;
using ParserCore.Models.Websites;

namespace ParserCore.Downloaders;

[SupportedOSPlatform("windows")]
public static class ImageDownloader
{
    private const int REQUEST_TIMEOUT = 2000;
    private const int RETRIES_COUNT   = 5;

    public static string WorkingDirectory
    {
        get
        {
            string desktop           = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var    resultsFolderName = $"imgParser{DateTime.Now.ToShortDateString()}_{DateTime.Now.ToShortTimeString()}";
            string path              = Path.Combine(desktop, resultsFolderName);
            return path;
        }
    }
    public static async Task DownloadImagesFromWebsiteAsync(IWebsite                    website,
                                                     IProgress<ProgressChangedEventArgs> progress,
                                                     CancellationToken                   token)
    {
        var directLinks = await new ParserExecutor(website.Parser).RetrieveDirectLinksToImages(token, progress);
        await DownloadImagesAsync(directLinks, progress, token);
    }

    private static async Task DownloadImagesAsync(IEnumerable<string> sourcesCollection,
                                                  IProgress<ProgressChangedEventArgs> progress,
                                                  CancellationToken token)
    {
        string[] urls = sourcesCollection as string[] ?? sourcesCollection.ToArray();

        Directory.CreateDirectory(WorkingDirectory);

        var downloadedCount = 0;
        int urlsCount = urls.Length;

        var pInfo = new ProgressChangedEventArgs { TextStatus = "Download started sequentially" };
        progress.Report(pInfo);

        foreach (string source in urls)
        {
            token.ThrowIfCancellationRequested();
            for (var i = 0; i < RETRIES_COUNT; i++)
            {
                if (await DownloadImageAsync(source, token))
                {
                    downloadedCount++;
                    break;
                }

                Thread.Sleep(REQUEST_TIMEOUT);
                token.ThrowIfCancellationRequested();
            }

            ReportSuccess(progress, pInfo, source, downloadedCount, urlsCount);
        }
    }
    private static async Task<bool> DownloadImageAsync(string imageUrl, CancellationToken token)
    {
        string[] urlParts = imageUrl.Split("/");
        string imgName = urlParts.Last();

        if (!Path.HasExtension(imgName)) imgName += ".jpg";

        string website = urlParts[2];
        string localWorkingDirectory = Path.Combine(WorkingDirectory, website);

        Directory.CreateDirectory(localWorkingDirectory);

        string imgPath = Path.Combine(localWorkingDirectory, imgName);

        if (IsFileLockedByAnotherProcess(imgPath, out _))
            return false;

        for (var i = 0; i < RETRIES_COUNT; i++)
        {
            await using var input = new FileStream(imgPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                await HttpHelper.GetFileAsync(imageUrl, input, token);
                break;
            }
            catch (Exception) { /*ignored*/ }
        }
        return true;
    }
    private static bool IsFileLockedByAnotherProcess(string imgPath, out IEnumerable<Process> lockers)
    {
        lockers = FileUtil.WhoIsLocking(imgPath);
        bool locked = lockers.Any();
        return locked;
    }
    private static void ReportSuccess(IProgress<ProgressChangedEventArgs> progress, ProgressChangedEventArgs pInfo, string source,
                                      int downloadedCount, int urlsCount)
    {
        pInfo.ItemsProcessed.Add(source);
        pInfo.Percentage = downloadedCount * 100 / urlsCount;
        progress.Report(pInfo);
    }
    private static bool SaveImageFromBase64String(string source)
    {
        byte[] bytes = Convert.FromBase64String(source);
        using var stream = new MemoryStream(bytes);

        var decodedBitmap = new Bitmap(stream);

        string base64Dir = Path.Combine(WorkingDirectory, "base64_decoded");
        Directory.CreateDirectory(base64Dir);

        decodedBitmap.Save(Path.Combine(base64Dir, $"{new Guid()}.png"));
        return true;
    }
}