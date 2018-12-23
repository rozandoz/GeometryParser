using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using Knapcode.TorSharp;

#pragma warning disable 618

namespace GeometryParser.Web
{
    public class WebClient : IDisposable
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
                TorControlPassword = "foobar",
                ToolRunnerType = ToolRunnerType.VirtualDesktop
            };

            Console.WriteLine("Download tools...");
            new TorSharpToolFetcher(_settings, new HttpClient()).FetchAsync().Wait();

            Console.WriteLine("Start proxy...");
            _proxy = new TorSharpProxy(_settings);
            _proxy.ConfigureAndStartAsync().Wait();

            UpdateCookie();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _proxy?.Stop();
        }

        #endregion

        #region Members

        public string GetContent(string url, int timeout = Timeout.Infinite)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.CookieContainer = _cookieContainer;
            webRequest.UserAgent = _userAgent;
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,image/*,*/*;q=0.8";
            webRequest.Proxy = new WebProxy(new Uri("http://localhost:" + _settings.PrivoxyPort));
            webRequest.Timeout = timeout;
            webRequest.KeepAlive = false;

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            using (var webResponceStream = webResponse.GetResponseStream())
            using (var streamReader = new StreamReader(webResponceStream, Encoding.GetEncoding(1251)))
            {
                return HttpUtility.HtmlDecode(streamReader.ReadToEnd());
            }
        }

        public string GetIP()
        {
            return GetContent("http://api.ipify.org");
        }

        public void UpdateAll()
        {
            UpdateCookie();
            UpdateUserAgent();
            UpdateProxy();
        }

        public void UpdateCookie()
        {
            _cookieContainer = new CookieContainer();
        }

        public void UpdateProxy()
        {
            var start = DateTime.Now;

            var oldAddress = GetIP();
            var newAddress = string.Empty;

            do
            {
                if (DateTime.Now - start >= TimeSpan.FromSeconds(15))
                    throw new TimeoutException("Unable to change proxy address");

                _proxy.GetNewIdentityAsync().Wait();
                newAddress = GetIP();

                if (!oldAddress.Equals(newAddress, StringComparison.InvariantCultureIgnoreCase))
                    break;

                Thread.Sleep(5000);
            } while (true);

            Console.WriteLine($"Address has been changed {oldAddress} => {newAddress}");
        }

        public void UpdateUserAgent()
        {
            _userAgent = UserAgents.GetRandomAgent();
        }

        #endregion
    }
}