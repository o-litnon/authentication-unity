using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Cdm.Authentication.Clients
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetAsync<T>(this HttpClient httpClient, string url,
                    AuthenticationHeaderValue authenticationHeader, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = authenticationHeader;

#if UNITY_EDITOR
            Debug.Log($"{request}");
#endif

            var response = await httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync();

#if UNITY_EDITOR
            Debug.Log(content);
#endif

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(content);
            }

            throw new Exception($"GET {nameof(T)} - {content}");
        }
    }
}