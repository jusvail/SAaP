using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace SAaP.Core.Helpers;

public static class Http
{
    private static readonly HttpClient Client = CreateHttpClientWithUserAgent();

    private static HttpClient CreateHttpClientWithUserAgent()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("UserAgent", "android-async-http/2.0 (http://loopj.com/android-async-http)");
        client.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
        return client;
        //android-async-http/2.0 (http://loopj.com/android-async-http)
        //"Mozilla/5.0 (Linux; Android 4.1.1; Nexus 7 Build/JRO03D) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.166  Safari/535.19"
    }

    public static void Dispose()
    {
        Client?.Dispose();
    }

    public static async Task<bool> CheckInternet()
    {
        var client = CreateHttpClientWithUserAgent();
        try
        {
            return (await client.GetAsync(new Uri("http://www.bing.com"))).IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            client.Dispose();
        }
    }

    public static async Task<string> GetStringAsync(string uri)
    {
        if (Client == null) CreateHttpClientWithUserAgent();

        try
        {
            return await Client?.GetStringAsync(new Uri(uri));
        }
        catch (Exception)
        {
            return string.Empty;

            //throw;
        }
    }

    public static async Task<IBuffer> GetBufferAsync(string uri)
    {
        try
        {
            return await Client.GetBufferAsync(new Uri(uri));
        }
        catch (Exception)
        {
            return null;

            //throw;
        }
    }

    public static async Task<string> GetStringAsync(HttpClient client, Uri uri)
    {
        return await client.GetStringAsync(uri);
    }
}