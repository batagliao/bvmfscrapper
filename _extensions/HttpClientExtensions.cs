﻿using bvmfscrapper.helpers;
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
        public static async Task<string> PostDataAsync(this HttpClient client, string url, Dictionary<string, string> data)
        {
            var content = new FormUrlEncodedContent(data);
            var response = await client.PostAsync(url, content);
            if(response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            var exception = new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}");
            return await Task.FromException<string>(exception);
        }

        public static async Task<string> GetStringWithRetryAsync(this HttpClient client, string url)
        {
            return await Retry.Do(async () =>
            {
                var s = await client.GetStringAsync(url);
                return s;
            },
            retryInterval: TimeSpan.FromSeconds(1),
            retryCount: 3
            );
        }

    }
}
