using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using Knapcode.TorSharp;

#pragma warning disable 618

namespace GeometryParser.Web
{
    public class WebClient
    {
        private readonly TorSharpProxy _proxy;
        private readonly TorSharpSettings _settings;

        private CookieContainer _cookieContainer;
        private string _userAgent;

        #region Constructors

        public WebClient()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(currentDirectory)) throw new InvalidOperationException();

            _settings = new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(currentDirectory, "TorZipped"),
                ExtractedToolsDirectory = Path.Combine(currentDirectory, "TorExtracted"),
                PrivoxyPort = 1337,
                TorSocksPort = 1338,
                TorControlPort = 1339,
                TorControlPassword = "foobar"
            };

            Console.WriteLine("Download tools...");
            new TorSharpToolFetcher(_settings, new HttpClient()).FetchAsync().Wait();

            Console.WriteLine("Start proxy...");
            _proxy = new TorSharpProxy(_settings);
            _proxy.ConfigureAndStartAsync().Wait();

            UpdateCookie();
        }

        #endregion

        #region Members

        public string GetContent(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.CookieContainer = _cookieContainer;
            webRequest.UserAgent = _userAgent;
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,image/*,*/*;q=0.8";
            webRequest.Proxy = new WebProxy(new Uri("http://localhost:" + _settings.PrivoxyPort));

            var webResponse = (HttpWebResponse)webRequest.GetResponse();

            using (var webResponceStream = webResponse.GetResponseStream())
            using (var streamReader = new StreamReader(webResponceStream, Encoding.GetEncoding(1251)))
            {
                return HttpUtility.HtmlDecode(streamReader.ReadToEnd());
            }
        }

        public void UpdateCookie()
        {
            _cookieContainer = new CookieContainer();
        }

        public void UpdateProxy()
        {
            _proxy.GetNewIdentityAsync().Wait();
        }

        public void UpdateUserAgent()
        {
            _userAgent = UserAgents.GetRandomAgent();
        }

        #endregion
    }
}