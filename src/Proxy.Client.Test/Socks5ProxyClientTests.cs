using NUnit.Framework;
using Proxy.Client.Contracts;
using Proxy.Client.Exceptions;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Proxy.Client.Test
{
    /// <summary>
    /// Test Fixture for the SOCKS5 Proxy Client.
    /// A proxy server supporting SOCKS5 must be running on the local host for the unit tests to work.
    /// </summary>
    [TestFixture]
    internal sealed class Socks5ProxyClientTests
    {
        private Socks5ProxyClient _socks5ProxyClient;

        [SetUp]
        public void SetUp()
        {
            _socks5ProxyClient = new Socks5ProxyClient("localhost", 1083, string.Empty, string.Empty);
        }

        #region Get Method
        [Test]
        public void Get_Successful()
        {
            var response = _socks5ProxyClient.Get("http://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_Successful()
        {
            var response = await _socks5ProxyClient.GetAsync("http://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithSsl_Successful()
        {
            var response = _socks5ProxyClient.Get("https://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_WithSsl_Successful()
        {
            var response = await _socks5ProxyClient.GetAsync("https://www.example.com/");

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

            var response = _socks5ProxyClient.Get("http://www.example.com/", headers: headers);

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

            var response = await _socks5ProxyClient.GetAsync("http://www.example.com/", headers: headers);

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

            var response = _socks5ProxyClient.Get("http://www.example.com/", cookies: cookies);

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

            var response = await _socks5ProxyClient.GetAsync("http://www.example.com/", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithoutKeepAlive_Successful()
        {
            var response = _socks5ProxyClient.Get("http://www.example.com/", isKeepAlive: false);
            var response2 = _socks5ProxyClient.Get("http://www.example.com/");

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
            var response = await _socks5ProxyClient.GetAsync("http://www.example.com/", isKeepAlive: false);
            var response2 = await _socks5ProxyClient.GetAsync("http://www.example.com/");

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
            var response = _socks5ProxyClient.Get("http://www.example.com/", isKeepAlive: true);
            var response2 = _socks5ProxyClient.Get("http://www.example.com/");

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
            var response = await _socks5ProxyClient.GetAsync("http://www.example.com/", isKeepAlive: true);
            var response2 = await _socks5ProxyClient.GetAsync("http://www.example.com/");

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
            Assert.Throws<ProxyException>(() => _socks5ProxyClient.Get("http:wrongurl"));
        }

        [Test]
        public void GetAsync_InvalidUrl_Failed()
        {
            Assert.ThrowsAsync<ProxyException>(async () => await _socks5ProxyClient.GetAsync("http:wrongurl"));
        }
        #endregion

        #region Post Method
        [Test]
        public void Post_Successful()
        {
            var response = _socks5ProxyClient.Post("http://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PostAsync_Successful()
        {
            var response = await _socks5ProxyClient.PostAsync("http://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Post_WithSsl_Successful()
        {
            var response = _socks5ProxyClient.Post("https://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PostAsync_WithSsl_Successful()
        {
            var response = await _socks5ProxyClient.PostAsync("https://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Post_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = _socks5ProxyClient.Post("http://www.example.com/", "testContent", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PostAsync_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = await _socks5ProxyClient.PostAsync("http://www.example.com/", "testContent", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Post_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = _socks5ProxyClient.Post("http://www.example.com/", "testContent", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PostAsync_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = await _socks5ProxyClient.PostAsync("http://www.example.com/", "testContent", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Post_WithoutKeepAlive_Successful()
        {
            var response = _socks5ProxyClient.Post("http://www.example.com/", "testContent", isKeepAlive: false);
            var response2 = _socks5ProxyClient.Post("http://www.example.com/", "testContent");

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
        public async Task PostAsync_WithoutKeepAlive_Successful()
        {
            var response = await _socks5ProxyClient.PostAsync("http://www.example.com/", "testContent", isKeepAlive: false);
            var response2 = await _socks5ProxyClient.PostAsync("http://www.example.com/", "testContent");

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
        public void Post_WithKeepAlive_Successful()
        {
            var response = _socks5ProxyClient.Post("http://www.example.com/", "testContent", isKeepAlive: true);
            var response2 = _socks5ProxyClient.Post("http://www.example.com/", "testContent");

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
        public async Task PostAsync_WithKeepAlive_Successful()
        {
            var response = await _socks5ProxyClient.PostAsync("http://www.example.com/", "testContent", isKeepAlive: true);
            var response2 = await _socks5ProxyClient.PostAsync("http://www.example.com/", "testContent");

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
        public void Post_InvalidUrl_Failed()
        {
            Assert.Throws<ProxyException>(() => _socks5ProxyClient.Post("http:wrongurl", "testContent"));
        }

        [Test]
        public void PostAsync_InvalidUrl_Failed()
        {
            Assert.ThrowsAsync<ProxyException>(async () => await _socks5ProxyClient.PostAsync("http:wrongurl", "testContent"));
        }
        #endregion

        #region Put Method
        [Test]
        public void Put_Successful()
        {
            var response = _socks5ProxyClient.Put("http://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PutAsync_Successful()
        {
            var response = await _socks5ProxyClient.PutAsync("http://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Put_WithSsl_Successful()
        {
            var response = _socks5ProxyClient.Put("https://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PutAsync_WithSsl_Successful()
        {
            var response = await _socks5ProxyClient.PutAsync("https://www.example.com/", "testContent");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Put_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = _socks5ProxyClient.Put("http://www.example.com/", "testContent", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PutAsync_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = await _socks5ProxyClient.PutAsync("http://www.example.com/", "testContent", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Put_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = _socks5ProxyClient.Put("http://www.example.com/", "testContent", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task PutAsync_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = await _socks5ProxyClient.PutAsync("http://www.example.com/", "testContent", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Put_WithoutKeepAlive_Successful()
        {
            var response = _socks5ProxyClient.Put("http://www.example.com/", "testContent", isKeepAlive: false);
            var response2 = _socks5ProxyClient.Put("http://www.example.com/", "testContent");

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
        public async Task PutAsync_WithoutKeepAlive_Successful()
        {
            var response = await _socks5ProxyClient.PutAsync("http://www.example.com/", "testContent", isKeepAlive: false);
            var response2 = await _socks5ProxyClient.PutAsync("http://www.example.com/", "testContent");

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
        public void Put_WithKeepAlive_Successful()
        {
            var response = _socks5ProxyClient.Put("http://www.example.com/", "testContent", isKeepAlive: true);
            var response2 = _socks5ProxyClient.Put("http://www.example.com/", "testContent");

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
        public async Task PutAsync_WithKeepAlive_Successful()
        {
            var response = await _socks5ProxyClient.PutAsync("http://www.example.com/", "testContent", isKeepAlive: true);
            var response2 = await _socks5ProxyClient.PutAsync("http://www.example.com/", "testContent");

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
        public void Put_InvalidUrl_Failed()
        {
            Assert.Throws<ProxyException>(() => _socks5ProxyClient.Put("http:wrongurl", "testContent"));
        }

        [Test]
        public void PutAsync_InvalidUrl_Failed()
        {
            Assert.ThrowsAsync<ProxyException>(async () => await _socks5ProxyClient.PutAsync("http:wrongurl", "testContent"));
        }
        #endregion

        #region Delete Method
        [Test]
        public void Delete_Successful()
        {
            var response = _socks5ProxyClient.Delete("http://www.example.com/customers/123");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task DeleteAsync_Successful()
        {
            var response = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Delete_WithSsl_Successful()
        {
            var response = _socks5ProxyClient.Delete("http://www.example.com/customers/123");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task DeleteAsync_WithSsl_Successful()
        {
            var response = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Delete_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = _socks5ProxyClient.Delete("http://www.example.com/customers/123", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task DeleteAsync_WithHeaders_Successful()
        {
            var headers = new List<ProxyHeader>
            {
                ProxyHeader.Create("HeaderName1", "HeaderValue1"),
                ProxyHeader.Create("HeaderName2", "HeaderValue2")
            };

            var response = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123", headers: headers);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Delete_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = _socks5ProxyClient.Delete("http://www.example.com/customers/123", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task DeleteAsync_WithCookies_Successful()
        {
            var cookies = new List<Cookie>
            {
                new Cookie("CookieName1", "CookieValue1"),
                new Cookie("CookieName2", "CookieValue2")
            };

            var response = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Delete_WithoutKeepAlive_Successful()
        {
            var response = _socks5ProxyClient.Delete("http://www.example.com/customers/123", isKeepAlive: false);
            var response2 = _socks5ProxyClient.Delete("http://www.example.com/customers/123");

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
        public async Task DeleteAsync_WithoutKeepAlive_Successful()
        {
            var response = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123", isKeepAlive: false);
            var response2 = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123");

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
        public void Delete_WithKeepAlive_Successful()
        {
            var response = _socks5ProxyClient.Delete("http://www.example.com/customers/123", isKeepAlive: true);
            var response2 = _socks5ProxyClient.Delete("http://www.example.com/customers/123");

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
        public async Task DeleteAsync_WithKeepAlive_Successful()
        {
            var response = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123", isKeepAlive: true);
            var response2 = await _socks5ProxyClient.DeleteAsync("http://www.example.com/customers/123");

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
        public void Delete_InvalidUrl_Failed()
        {
            Assert.Throws<ProxyException>(() => _socks5ProxyClient.Delete("http:wrongurl"));
        }

        [Test]
        public void DeleteAsync_InvalidUrl_Failed()
        {
            Assert.ThrowsAsync<ProxyException>(async () => await _socks5ProxyClient.DeleteAsync("http:wrongurl"));
        }
        #endregion

        [TearDown]
        public void TearDown()
        {
            _socks5ProxyClient.Dispose();
        }
    }
}
