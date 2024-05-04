using System.Net.Http.Headers;
namespace ParserCore.Helpers;
public static class HttpHelper
{
    public const   int        RETRIES_COUNT = 5;
    private static HttpClient Client { get; set; } = null!;

    public static bool IsInitialized { get; private set; } = false;
    public static void InitializeClient(string? mediaType = null)
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.Clear();
        var userAgentHeader = new KeyValuePair<string, string>("user-agent",
                                                               "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.13");
        _ = Client.DefaultRequestHeaders.TryAddWithoutValidation(userAgentHeader.Key, userAgentHeader.Value);
        if (mediaType == null)
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        else Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

        IsInitialized = true;
    }

    private static void EnsureInitialized()
    {
        if (!IsInitialized)
            throw new InvalidOperationException(typeof(HttpClient) + " " + nameof(Client) + " must be initialized");
    }
    public static void InitializeClientWithDefaultHeaders()
    {
        Client = new HttpClient();
        var userAgentHeader = new KeyValuePair<string, string>("user-agent",
                                                               "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.13");
        _ = Client.DefaultRequestHeaders.TryAddWithoutValidation(userAgentHeader.Key, userAgentHeader.Value);

        IsInitialized = true;
    }
    public static async Task<Stream?> GetStreamAsync(string url, CancellationToken token)
    {
        EnsureInitialized();

        var response = await Client.GetAsync(url, token);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(token);
        return stream;
    }
    public static async Task<string?> GetStringAsync(string url, CancellationToken token)
    {
        EnsureInitialized();

        var response = await Client.GetAsync(url, token);
        response.EnsureSuccessStatusCode();
        string result = await response.Content.ReadAsStringAsync(token);
        return result;
    }
    public static async Task<HttpContent?> GetContentAsync(string url, CancellationToken token)
    {
        EnsureInitialized();

        var response = await Client.GetAsync(url, token);
        response.EnsureSuccessStatusCode();
        var content = response.Content;
        return content;
    }
    public static async Task GetFileAsync(string url, Stream destination, CancellationToken token,
                                          IProgress<double>? progress = null)
    {
        EnsureInitialized();

        var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        long            total  = response.Content.Headers.ContentLength ?? -1L;
        await using var source = await response.Content.ReadAsStreamAsync(token);
        await CopyStreamWithProgressAsync(source, destination, total, token);
    }
    private static async Task CopyStreamWithProgressAsync(Stream input, Stream output, long total,
                                                          CancellationToken token, IProgress<double>? progress = null)
    {
        const int IO_BUFFER_SIZE = 8 * 1024; // Optimal size depends on your scenario
        // Expected size of input stream may be known from an HTTP header when reading from HTTP. Other streams may have their
        // own protocol for pre-reporting expected size.
        bool canReportProgress = total != -1 && progress != null;
        var totalRead = 0L;
        var buffer = new byte[IO_BUFFER_SIZE];
        int read;
        while ((read = await input.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
        {
            //token.ThrowIfCancellationRequested();
            await output.WriteAsync(buffer, 0, read, token);
            totalRead += read;
            if (canReportProgress)
                progress?.Report(totalRead * 1d / (total * 1d) * 100);
        }
    }
}