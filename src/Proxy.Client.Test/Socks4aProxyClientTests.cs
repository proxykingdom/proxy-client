using NUnit.Framework;
using Proxy.Client.Contracts;
using Proxy.Client.Exceptions;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Proxy.Client.Test
{
    [TestFixture]
    internal sealed class Socks4aProxyClientTests
    {
        private Socks4aProxyClient _socks4aProxyClient;

        [SetUp]
        public void SetUp()
        {
            _socks4aProxyClient = new Socks4aProxyClient("localhost", 1082);
        }

        [Test]
        public void Get_Successful()
        {
            var response = _socks4aProxyClient.Get("http://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_Successful()
        {
            var response = await _socks4aProxyClient.GetAsync("http://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithSsl_Successful()
        {
            var response = _socks4aProxyClient.Get("https://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_WithSsl_Successful()
        {
            var response = await _socks4aProxyClient.GetAsync("https://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = _socks4aProxyClient.Get("http://www.example.com/", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = await _socks4aProxyClient.GetAsync("http://www.example.com/", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = _socks4aProxyClient.Get("http://www.example.com/", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = await _socks4aProxyClient.GetAsync("http://www.example.com/", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithoutKeepAlive_Successful()
        {
            var response = _socks4aProxyClient.Get("http://www.example.com/", isKeepAlive: false);
            var response2 = _socks4aProxyClient.Get("http://www.example.com/");

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(response);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                Assert.IsNotNull(response2);
                Assert.AreEqual(response2.StatusCode, HttpStatusCode.OK);
                Assert.IsTrue(response2.Timings.ConnectTime > 0);
            });
        }

        [Test]
        public async Task GetAsync_WithoutKeepAlive_Successful()
        {
            var response = await _socks4aProxyClient.GetAsync("http://www.example.com/", isKeepAlive: false);
            var response2 = await _socks4aProxyClient.GetAsync("http://www.example.com/");

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(response);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                Assert.IsNotNull(response2);
                Assert.AreEqual(response2.StatusCode, HttpStatusCode.OK);
                Assert.IsTrue(response2.Timings.ConnectTime > 0);
            });
        }

        [Test]
        public void Get_WithKeepAlive_Successful()
        {
            var response = _socks4aProxyClient.Get("http://www.example.com/", isKeepAlive: true);
            var response2 = _socks4aProxyClient.Get("http://www.example.com/");

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(response);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                Assert.IsNotNull(response2);
                Assert.AreEqual(response2.StatusCode, HttpStatusCode.OK);
                Assert.IsTrue(response2.Timings.ConnectTime == 0);
            });
        }

        [Test]
        public async Task GetAsync_WithKeepAlive_Successful()
        {
            var response = await _socks4aProxyClient.GetAsync("http://www.example.com/", isKeepAlive: true);
            var response2 = await _socks4aProxyClient.GetAsync("http://www.example.com/");

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(response);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                Assert.IsNotNull(response2);
                Assert.AreEqual(response2.StatusCode, HttpStatusCode.OK);
                Assert.IsTrue(response2.Timings.ConnectTime == 0);
            });
        }

        [Test]
        public void Get_InvalidUrl_Failed()
        {
            Assert.Throws<ProxyException>(() => _socks4aProxyClient.Get("http:wrongurl"));
        }

        [Test]
        public void GetAsync_InvalidUrl_Failed()
        {
            Assert.ThrowsAsync<ProxyException>(async () => await _socks4aProxyClient.GetAsync("http:wrongurl"));
        }

        [TearDown]
        public void TearDown()
        {
            _socks4aProxyClient.Dispose();
        }
    }
}
