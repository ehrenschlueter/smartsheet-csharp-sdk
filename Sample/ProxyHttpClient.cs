using Smartsheet.Api.Internal.Http;
using RestSharp;
using System.Net;

namespace sdk_csharp_sample
{
    class ProxyHttpClient : DefaultHttpClient
    {
        public ProxyHttpClient(string host, int port)
            : base(new RestClient(new RestClientOptions
            {
                RedirectOptions = new RedirectOptions { FollowRedirects = true },
                Proxy = new WebProxy(host, port)
            }), new Smartsheet.Api.Internal.Json.JsonNetSerializer())
        {
        }
    }
}
