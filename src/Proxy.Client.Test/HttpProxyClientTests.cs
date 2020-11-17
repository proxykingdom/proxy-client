﻿using NUnit.Framework;
using Proxy.Client.Contracts;
using Proxy.Client.Exceptions;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Proxy.Client.Test
{
    /// <summary>
    /// Test Fixture for the HTTP Proxy Client.
    /// A proxy server supporting HTTP must be running on the local host for the unit tests to work.
    /// </summary>
    [TestFixture]
    internal sealed class HttpProxyClientTests
    {
        private HttpProxyClient _httpProxyClient;

        [SetUp]
        public void SetUp()
        {
            _httpProxyClient = new HttpProxyClient("localhost", 1080);
        }

        [Test]
        public void Get_Successful()
        {
            var response = _httpProxyClient.Get("http://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_Successful()
        {
            var response = await _httpProxyClient.GetAsync("http://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithSsl_Successful()
        {
            var response = _httpProxyClient.Get("https://www.example.com/");

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public async Task GetAsync_WithSsl_Successful()
        {
            var response = await _httpProxyClient.GetAsync("https://www.example.com/");

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

            var response = _httpProxyClient.Get("http://www.example.com/", headers: headers);

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

            var response = await _httpProxyClient.GetAsync("http://www.example.com/", headers: headers);

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

            var response = _httpProxyClient.Get("http://www.example.com/", cookies: cookies);

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

            var response = await _httpProxyClient.GetAsync("http://www.example.com/", cookies: cookies);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Get_WithoutKeepAlive_Successful()
        {
            var response = _httpProxyClient.Get("http://www.example.com/", isKeepAlive: false);
            var response2 = _httpProxyClient.Get("http://www.example.com/");

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
            var response = await _httpProxyClient.GetAsync("http://www.example.com/", isKeepAlive: false);
            var response2 = await _httpProxyClient.GetAsync("http://www.example.com/");

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
            var response = _httpProxyClient.Get("http://www.example.com/", isKeepAlive: true);
            var response2 = _httpProxyClient.Get("http://www.example.com/");

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
            var response = await _httpProxyClient.GetAsync("http://www.example.com/", isKeepAlive: true);
            var response2 = await _httpProxyClient.GetAsync("http://www.example.com/");

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
            Assert.Throws<ProxyException>(() => _httpProxyClient.Get("http:wrongurl"));
        }

        [Test]
        public void GetAsync_InvalidUrl_Failed()
        {
            Assert.ThrowsAsync<ProxyException>(async () => await _httpProxyClient.GetAsync("http:wrongurl"));
        }

        [TearDown]
        public void TearDown()
        {
            _httpProxyClient.Dispose();
        }
    }
}
