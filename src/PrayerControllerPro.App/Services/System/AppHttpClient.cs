using System.Net.Http;

namespace PrayerControllerPro.App.Services.System;

public sealed class AppHttpClient
{
    private static readonly HttpClient SharedHttpClient = CreateHttpClient();

    public Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken = default)
    {
        return SharedHttpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    public Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        return SharedHttpClient.GetStringAsync(requestUri, cancellationToken);
    }

    public Task<HttpResponseMessage> PostAsync(
        Uri requestUri,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        return SharedHttpClient.PostAsync(requestUri, content, cancellationToken);
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"PrayerControllerPro/{AppIdentity.CurrentVersion}");
        return httpClient;
    }
}
