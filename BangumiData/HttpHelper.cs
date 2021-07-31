using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BangumiData
{
    public static class HttpHelper
    {
        private static HttpClient _client = null;

        private static void EnsureClient()
        {
            if (_client == null)
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                };
                _client = new HttpClient(handler);
                // 添加登录后默认的 Headers
                _client.DefaultRequestHeaders.Add("User-Agent", "Z19.BangumiData");
            }
        }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="HttpRequestException"/>
        public static async Task<string> GetStringAsync(string url)
        {
            EnsureClient();
            return await _client.GetStringAsync(url).ConfigureAwait(false);
        }


        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="HttpRequestException"/>
        public static async Task<JsonDocument?> GetJsonDocumentAsync(string url)
        {
            EnsureClient();
            var response = await _client.GetAsync(url).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await JsonSerializer.DeserializeAsync<JsonDocument>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return null;
                }
            }
            return null;
        }
    }
}
