using System.Net.Http.Headers;

namespace ParserCore.Loaders
{
    public static class HttpHelper
    {
        public static HttpClient Client { get; set; } = null!;

        public static void InitializeClient(string? mediaType = null)
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Clear();
            if (mediaType == null)
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            else Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        }
        public static void InitializeClientWithDefaultHeaders()
        {
            Client = new HttpClient();
            //Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru-RU"));
        }
        public static async Task<Stream?> GetStreamAsync(string url, CancellationToken token)
        {
            var response = await Client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync(token);
            return stream;
        }
        public static async Task<string?> GetStringAsync(string url, CancellationToken token)
        {
            var response = await Client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync(token);
            return result;
        }
        public static async Task<HttpContent?> GetContentAsync(string url, CancellationToken token)
        {
            var response = await Client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();
            var content = response.Content;
            return content;
        }
        public static async Task GetFileAsync(string url, Stream destination, CancellationToken token, IProgress<double>? progress = null)
        {
            var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();
            var total = response.Content.Headers.ContentLength ?? -1L;
            using var source = await response.Content.ReadAsStreamAsync(token);

            await CopyStreamWithProgressAsync(source, destination, total, token);
        }
        private static async Task CopyStreamWithProgressAsync(Stream input, Stream output, long total, CancellationToken token, IProgress<double>? progress = null)
        {
            const int IO_BUFFER_SIZE = 8 * 1024; // Optimal size depends on your scenario

            // Expected size of input stream may be known from an HTTP header when reading from HTTP. Other streams may have their
            // own protocol for pre-reporting expected size.

            var canReportProgress = total != -1 && progress != null;
            var totalRead = 0L;
            byte[] buffer = new byte[IO_BUFFER_SIZE];
            int read;

            while ((read = await input.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                //token.ThrowIfCancellationRequested();
                await output.WriteAsync(buffer, 0, read);
                totalRead += read;
                if (canReportProgress)
                    progress?.Report(totalRead * 1d / (total * 1d) * 100);
            }
        }
    }
}
