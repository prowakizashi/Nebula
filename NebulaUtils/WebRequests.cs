using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NebulaUtils
{
    public static class WebRequests
    {
        public enum EDownloadResponse
        {
            SUCCESS,
            DIRECTORY_ERROR,
            WEB_ERROR,
            INVALID_PARAMETERS,
            NOT_SUPPORTED
        }

        private static async Task DownloadFileTask(string Url, string LocalPath, bool IsMainThread, Action Callback = null)
        {
            if (Url == null || Url.Length == 0 || LocalPath == null || LocalPath.Length == 0)
                return;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(Url).ConfigureAwait(!IsMainThread); ;
                    response.EnsureSuccessStatusCode();

                    string dir = Path.GetDirectoryName(LocalPath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    Stream contentStream = await response.Content.ReadAsStreamAsync();
                    using (FileStream fileStream = File.Create(LocalPath))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }
                catch (Exception) { }
            }

            Callback?.Invoke();
        }

        private static async Task DownloadStringTask(string Url, bool IsMainThread, Action<string> Callback = null)
        {
            string responseBody = "";
            if (Url != null && Url.Length > 0)
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(5.0);
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(Url).ConfigureAwait(!IsMainThread);
                        if (response.IsSuccessStatusCode)
                            responseBody = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception) {  }
                }
            }
            Callback?.Invoke(responseBody);
        }

        private static async Task DownloadFileWithResponseTask(string Url, string LocalPath, bool IsMainThread, Action<EDownloadResponse> Callback = null)
        {
            if (Url == null || Url.Length == 0 || LocalPath == null || LocalPath.Length == 0)
            {
                Callback?.Invoke(EDownloadResponse.INVALID_PARAMETERS);
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(Url).ConfigureAwait(!IsMainThread);
                    if (!response.IsSuccessStatusCode)
                    {
                        Callback?.Invoke(EDownloadResponse.WEB_ERROR);
                        return;
                    }

                    string dir = Path.GetDirectoryName(LocalPath);
                    try
                    {
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }
                    catch (Exception)
                    {
                        Callback?.Invoke(EDownloadResponse.DIRECTORY_ERROR);
                        return;
                    }

                    Stream contentStream = await response.Content.ReadAsStreamAsync();
                    using (FileStream fileStream = File.Create(LocalPath))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }

                    Callback?.Invoke(EDownloadResponse.SUCCESS);
                }
                catch (HttpRequestException) { Callback?.Invoke(EDownloadResponse.WEB_ERROR); }
                catch (Exception) { Callback?.Invoke(EDownloadResponse.NOT_SUPPORTED); }
            }
        }

        private static async Task DownloadStringWithResponseTask(string Url, bool IsMainThread, Action<string, EDownloadResponse> Callback = null)
        {
            if (Url == null || Url.Length == 0)
            {
                Callback?.Invoke("", EDownloadResponse.INVALID_PARAMETERS);
                return;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(Url).ConfigureAwait(!IsMainThread);
                    if (!response.IsSuccessStatusCode)
                    {
                        Callback?.Invoke("", EDownloadResponse.WEB_ERROR);
                        return;
                    }
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Callback?.Invoke(responseBody, EDownloadResponse.SUCCESS);
                }
                catch (HttpRequestException) { Callback?.Invoke("", EDownloadResponse.WEB_ERROR); }
                catch (Exception) { Callback?.Invoke("", EDownloadResponse.NOT_SUPPORTED); }
            }
        }

        public static void DownloadFileSync(string Url, string LocalPath)
        {
            var task = DownloadFileTask(Url, LocalPath, true);
            task.Wait();
        }

        public static async void DownloadFileAsync(string Url, string LocalPath, Action Callback = null)
        {
            await DownloadFileTask(Url, LocalPath, false, Callback);
        }

        public static string DownloadStringSync(string Url)
        {
            string str = "";

            var task = DownloadStringTask(Url, true, (result) =>
            {
                str = result;
            });
            task.Wait();

            return str;
        }

        public static async void DownloadStringAsync(string Url, Action<string> Callback = null)
        {
            await DownloadStringTask(Url, false, Callback);
        }

        public static EDownloadResponse DownloadFileWithResponseSync(string Url, string LocalPath)
        {
            EDownloadResponse res = EDownloadResponse.NOT_SUPPORTED;

            var task = DownloadFileWithResponseTask(Url, LocalPath, true, (result) =>
            {
                res = result;
            });
            task.Wait();

            return res;
        }

        public static async void DownloadFileWithResponseAsync(string Url, string LocalPath, Action<EDownloadResponse> Callback = null)
        {
            await DownloadFileWithResponseTask(Url, LocalPath, false, Callback);
        }

        public static string DownloadStringWithResponseSync(string Url, out EDownloadResponse Result)
        {
            string str = "";
            EDownloadResponse res = EDownloadResponse.NOT_SUPPORTED;

            var task = DownloadStringWithResponseTask(Url, true, (message, result) =>
            {
                str = message;
                res = result;
            });
            task.Wait();

            Result = res;
            return str;
        }

        public static async void DownloadStringWithResponseAsync(string Url, Action<string, EDownloadResponse> Callback = null)
        {
            await DownloadStringWithResponseTask(Url, false, Callback);
        }
    }
}
