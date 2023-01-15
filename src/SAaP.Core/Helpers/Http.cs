using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace SAaP.Core.Helpers;

public static class Http
{
    private static HttpClient CreateHttpClientWithUserAgent()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:105.0) Gecko/20100101 Firefox/105.0");
        client.DefaultRequestHeaders.Add("ContentType", "application/json");
        return client;
        //android-async-http/2.0 (http://loopj.com/android-async-http)
        //"Mozilla/5.0 (Linux; Android 4.1.1; Nexus 7 Build/JRO03D) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.166  Safari/535.19"
    }

    public static async Task<bool> CheckInternet()
    {
        using var client = CreateHttpClientWithUserAgent();

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

        using var client = CreateHttpClientWithUserAgent();

        try
        {
            return await client.GetStringAsync(new Uri(uri));
        }
        catch (Exception)
        {
            return string.Empty;

            //throw;
        }
        finally
        {
            client.Dispose();
        }
    }


    public static async Task<string> GetStringWithoutUserAgentAsync(string uri)
    {
        using var client = new HttpClient();

        try
        {
            return await client.GetStringAsync(new Uri(uri));
        }
        catch (Exception)
        {
            return string.Empty;

            //throw;
        }
    }

    public static async Task<IBuffer> GetBufferAsync(string uri)
    {
        using var client = CreateHttpClientWithUserAgent();

        try
        {
            return await client.GetBufferAsync(new Uri(uri));
        }
        catch (Exception)
        {
            return null;

            //throw;
        }
        finally
        {
            client.Dispose();
        }
    }


    public static async Task<string> PostStringAsync(string uri, string postArgs)
    {
        using var client = CreateHttpClientWithUserAgent();

        try
        {
            var content = new HttpStringContent(postArgs);
            // necessary
            content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");

            // post async
            var result = await client.PostAsync(new Uri(uri), content);

            // return result
            return result.IsSuccessStatusCode ? result.Content.ToString() : string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;

            //throw;
        }
        finally
        {
            client.Dispose();
        }
    }
}