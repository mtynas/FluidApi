namespace PushSharp.Core
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public static class PushHttpClient
    {
        static PushHttpClient()
        {
#if NET45
            ServicePointManager.DefaultConnectionLimit = 100;
#endif
        }

        public static async Task<PushHttpResponse> RequestAsync(PushHttpRequest pushRequest)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
#if NETSTANDARD16
                httpClientHandler.MaxConnectionsPerServer = 100;
#endif
                httpClientHandler.UseProxy = false;

                using (var client = new HttpClient(httpClientHandler))
                {
                    client.DefaultRequestHeaders.ExpectContinue = false;

                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri(pushRequest.Url),
                        Method = pushRequest.HttpMethod,
                    };

                    if (!string.IsNullOrEmpty(pushRequest.Body))
                    {
                        request.Content = new ByteArrayContent(pushRequest.Encoding.GetBytes(pushRequest.Body));
                    }

                    foreach (var headerKey in pushRequest.Headers.AllKeys)
                    {
                        request.Headers.Add(headerKey, pushRequest.Headers[headerKey]);
                    }

                    var response = await client.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var responseEncoding = Encoding.ASCII;
                    try
                    {
                        responseEncoding = Encoding.GetEncoding(response.Content.Headers.ContentEncoding.FirstOrDefault());
                    }
                    catch
                    {
                    }

                    var responseHeaders = new WebHeaderCollection();
                    foreach (var header in response.Headers)
                    {
                        responseHeaders[header.Key] = header.Value.FirstOrDefault();
                    }

                    return new PushHttpResponse
                    {
                        Body = responseBody,
                        Headers = responseHeaders,
                        Uri = response.RequestMessage.RequestUri,
                        Encoding = responseEncoding,
                        LastModified = response.Content.Headers.LastModified.GetValueOrDefault().DateTime,
                        StatusCode = response.StatusCode
                    };
                }
            }
        }
    }

    public class PushHttpRequest
    {
        public PushHttpRequest()
        {
            Encoding = Encoding.ASCII;
            Headers = new WebHeaderCollection();
            Method = "GET";
            Body = string.Empty;
        }

        public string Url { get; set; }

        public string Method { get; set; }

        public string Body { get; set; }

        public WebHeaderCollection Headers { get; set; }

        public Encoding Encoding { get; set; }

        public HttpMethod HttpMethod
        {
            get
            {
                switch (this.Method.ToUpperInvariant())
                {
                    case "delete":
                        return HttpMethod.Delete;
                    case "get":
                        return HttpMethod.Get;
                    case "head":
                        return HttpMethod.Head;
                    case "options":
                        return HttpMethod.Options;
                    case "post":
                        return HttpMethod.Post;
                    case "put":
                        return HttpMethod.Put;
                    case "trace":
                        return HttpMethod.Trace;
                    default:
                        throw new NotSupportedException("Invalid HTTP method.");
                }
            }
        }
    }

    public class PushHttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Body { get; set; }

        public WebHeaderCollection Headers { get; set; }

        public Uri Uri { get; set; }

        public Encoding Encoding { get; set; }

        public DateTime LastModified { get; set; }
    }
}
