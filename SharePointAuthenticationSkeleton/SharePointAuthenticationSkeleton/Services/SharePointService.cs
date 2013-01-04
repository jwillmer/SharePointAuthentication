using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SharePointAuthenticationSkeleton.Helpers;

namespace SharePointAuthenticationSkeleton.DataService
{
    public static class SharePointService
    {
        public static async Task<string> GetStringFromSharePointUrl(Uri location)
        {
            if (SharePointAuthentication.Current != null)
            {
                var clientHandler = new HttpClientHandler();
                clientHandler.CookieContainer = new CookieContainer();
                CookieContainer cookieContainer = await SharePointAuthentication.Current.GetCookieContainer();

                foreach (Cookie cookie in cookieContainer.GetCookies(location))
                    clientHandler.CookieContainer.Add(location, cookie);

                var bytes = await HttpUtility.SendHttpRequest(
                    location,
                    HttpMethod.Get,
                    null,
                    null,
                    clientHandler);

                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }

            return string.Empty;
        }

        public static async Task<byte[]> GetDataFromSharePointUrl(Uri location)
        {
            if (SharePointAuthentication.Current != null)
            {
                var clientHandler = new HttpClientHandler();
                clientHandler.CookieContainer = new CookieContainer();
                CookieContainer cookieContainer = await SharePointAuthentication.Current.GetCookieContainer();

                foreach (Cookie cookie in cookieContainer.GetCookies(location))
                    clientHandler.CookieContainer.Add(location, cookie);

                var bytes = await HttpUtility.SendHttpRequest(
                    location,
                    HttpMethod.Get,
                    null,
                    null,
                    clientHandler);

                return bytes;
            }

            return new byte[0];
        }
    }
}
