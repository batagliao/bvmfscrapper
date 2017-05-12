using bvmfscrapper.helpers;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bvmfscrapper
{
    public static class HttpClientExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompanyExtensions));


        public static async Task<string> PostDataAsync(this HttpClient client, string url, Dictionary<string, string> data)
        {
            log.Info($"POST HTTP. URL = {url}");

            var content = new FormUrlEncodedContent(data);
            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            var exception = new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}");
            log.Error(exception);
            return await Task.FromException<string>(exception);
        }

        public static async Task<string> GetStringWithRetryAsync(this HttpClient client, string url)
        {
            log.Info($"GET HTTP. URL = {url}");

            return await Retry.Do(async () =>
            {
                var s = await client.GetStringAsync(url);
                return s;
            },
            retryInterval: TimeSpan.FromSeconds(1),
            retryCount: 3
            );
        }

        public static async Task<string> GetStringWithRetryAsync(this HttpClient client, string url, Encoding encoding)
        {
            log.Info($"GET HTTP. URL = {url}");

            return await Retry.Do(async () =>
            {
                var bytes = await client.GetByteArrayAsync(url);
                return encoding.GetString(bytes);
            },
            retryInterval: TimeSpan.FromSeconds(1),
            retryCount: 3
            );
        }
    }
}
