using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SharePointAuthenticationSkeleton.Helpers;
using Windows.Data.Json;

namespace SharePointAuthenticationSkeleton
{
    internal class HttpUtility
    {
        /// <summary>
        /// Sends a JSON OData request appending the SharePoint canary to the request header.
        /// Appending the canary to the request is necessary to perform write operations (e.g. create, update, delete list items)
        /// The canary is a security measure to prevent cross site scripting attacks
        /// </summary>
        /// <param name="uri">The request uri</param>
        /// <param name="method">The http method</param>
        /// <param name="requestContent">A stream containing the request content</param>
        /// <param name="clientHandler">The request client handler</param>
        /// <param name="authUtility">An instance of the auth helper to perform authenticated calls to SPO</param>
        /// <returns></returns>
        public static async Task<byte[]> SendODataJsonRequestWithCanary(Uri uri, HttpMethod method, Stream requestContent, HttpClientHandler clientHandler, SharePointAuthentication authUtility)
        {
            // Make a post request to {siteUri}/_api/contextinfo to get the canary
            var response = await HttpUtility.SendODataJsonRequest(
                new Uri(String.Format("{0}/_api/contextinfo", SharePointAuthentication.Current.SiteUrl)),
                HttpMethod.Post,
                null,
                clientHandler,
                SharePointAuthentication.Current);

            Dictionary<String, IJsonValue> dict = new Dictionary<string, IJsonValue>();
            HttpUtility.ParseJson(JsonObject.Parse(Encoding.UTF8.GetString(response, 0, response.Length)), dict); // parse the JSON response containing the canary

            string canary = dict["FormDigestValue"].GetString(); // the canary is contained in the FormDigestValue of the response body

            // Make the OData request passing the canary in the request headers
            return await HttpUtility.SendODataJsonRequest(
                uri,
                method,
                requestContent,
                clientHandler,
                SharePointAuthentication.Current, 
                new Dictionary<string, string> { 
                { "X-RequestDigest", canary  } 
                });
        }

        /// <summary>
        /// Sends a JSON OData request appending SPO auth cookies to the request header.
        /// </summary>
        /// <param name="uri">The request uri</param>
        /// <param name="method">The http method</param>
        /// <param name="requestContent">A stream containing the request content</param>
        /// <param name="clientHandler">The request client handler</param>
        /// <param name="authUtility">An instance of the auth helper to perform authenticated calls to SPO</param>
        /// <param name="headers">The http headers to append to the request</param>
        public static async Task<byte[]> SendODataJsonRequest(Uri uri, HttpMethod method, Stream requestContent, HttpClientHandler clientHandler, SharePointAuthentication authUtility, Dictionary<string, string> headers = null)
        {
            if (clientHandler.CookieContainer == null)
                clientHandler.CookieContainer = new CookieContainer();

            CookieContainer cookieContainer = await authUtility.GetCookieContainer(); // get the auth cookies from SPO after authenticating with Microsoft Online Services STS

            foreach (Cookie c in cookieContainer.GetCookies(uri))
            {
                clientHandler.CookieContainer.Add(uri, c); // apppend SPO auth cookies to the request
            }

            return await SendHttpRequest(
                uri, 
                method, 
                requestContent, 
                "application/json;odata=verbose;charset=utf-8", // the http content type for the JSON flavor of SP REST services 
                clientHandler, 
                headers);
        }

        /// <summary>
        /// Sends an http request to the specified uri and returns the response as a byte array 
        /// </summary>
        /// <param name="uri">The request uri</param>
        /// <param name="method">The http method</param>
        /// <param name="requestContent">A stream containing the request content</param>
        /// <param name="contentType">The content type of the http request</param>
        /// <param name="clientHandler">The request client handler</param>
        /// <param name="headers">The http headers to append to the request</param>
        public static async Task<byte[]> SendHttpRequest(Uri uri, HttpMethod method, Stream requestContent = null, string contentType = null, HttpClientHandler clientHandler = null, Dictionary<string, string> headers = null)
        {
            var req = clientHandler == null ? new HttpClient() : new HttpClient(clientHandler);
            var message = new HttpRequestMessage(method, uri);
            byte[] response;
            
            req.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
            message.Headers.Add("Accept", contentType); // set the content type of the request

                
            if (requestContent != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete))
            {
                message.Content = new StreamContent(requestContent); //set the body for the request

                if (!string.IsNullOrEmpty(contentType))
                {
                    message.Content.Headers.Add("Content-Type", contentType); // if the request has a body set the MIME type
                }
            }

            // append additional headers to the request
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (message.Headers.Contains(header.Key))
                    {
                        message.Headers.Remove(header.Key);
                    }

                    message.Headers.Add(header.Key, header.Value);
                }
            }

            // Send the request and read the response as an array of bytes
            using (var res = await req.SendAsync(message))
            {
                response = await res.Content.ReadAsByteArrayAsync();
            }

            return response;
        }

        /// <summary>
        /// Parses a JSON object recursively into a dictionary of name value pairs
        /// </summary>
        /// <param name="jObj">The root JSON object</param>
        /// <param name="result">The reference to the resulting dictionary</param>
        public static Dictionary<String, IJsonValue> ParseJson(JsonObject jObj, Dictionary<String, IJsonValue> result)
        {
            var keys = jObj.Keys.GetEnumerator();

            while (keys.MoveNext())
            {
                String key = keys.Current;
                if (jObj[key].ValueType == JsonValueType.Object)
                {
                    JsonObject value = jObj[key].GetObject();
                    ParseJson(value, result);
                }
                else if (jObj[key].ValueType != JsonValueType.Null)
                {
                    if (!result.ContainsKey(key))
                        result.Add(key, jObj[key]);
                }
            }

            return result;
        }
    }
}
