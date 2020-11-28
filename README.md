![Proxy.Client](https://i.imgur.com/5HM1SLu.png)

# Proxy.Client [![Build](https://github.com/bokklu/proxy-client/workflows/Build/badge.svg)](https://github.com/bokklu/proxy-client/actions?query=workflow%3ABuild) [![NuGet](https://img.shields.io/nuget/v/Proxy.Client?style=flat-square)](https://www.nuget.org/packages/Proxy.Client/) [![GitHub](https://img.shields.io/github/license/bokklu/proxy-client?style=flat-square)](https://github.com/bokklu/proxy-client/blob/master/LICENSE) [![Target Framework](https://img.shields.io/static/v1?label=Target%20Framework&message=.NET%205.0&color=blue&style=flat-square)](https://dotnet.microsoft.com/download/dotnet/5.0) [![Downloads](https://img.shields.io/nuget/dt/Proxy.Client?color=success&style=flat-square)](https://www.nuget.org/packages/Proxy.Client/)

The Proxy Client library allows the user to send and receive requests over Http, Socks4/4a/5 proxies.

## Installation

Install as [Nuget Package](https://www.nuget.org/packages/Proxy.Client/)

```powershell
Install-Package Proxy.Client
```

.NET CLI:

```shell
dotnet add package Proxy.Client
```
## Basic Usage
```C#
using(var socks4ProxyClient = new Socks4ProxyClient("212.86.75.9", 4153))
{
    var response = await socks4ProxyClient.GetAsync("https://www.example.com/");
}
```
### Adding Headers
```C#
using(var socks4ProxyClient = new Socks4ProxyClient("212.86.75.9", 4153))
{
    var proxyHeaders = var headers = new List<ProxyHeader>
    {
        ProxyHeader.Create("User-Agent", "My-UserAgent"),
        ProxyHeader.Create("Cache-Control", "no-cache")
    };
    
    var response = await socks4ProxyClient.GetAsync("https://www.example.com/", headers: proxyHeaders);
}
```
### Adding Cookies
```C#
using(var socks4ProxyClient = new Socks4ProxyClient("212.86.75.9", 4153))
{
    var proxyCookies = new List<Cookie>
    {
        new Cookie("Id", "a3fWa"),
        new Cookie("Expires", "Thu, 31 Oct 2021 07:28:00 GMT")
    };
    
    var response = await socks4ProxyClient.GetAsync("https://www.example.com/", cookies: proxyCookies);
}
```
### Adding Timeouts
Below is an example of a request with a 10 second total timeout and 1 second read/write timeout:
```C#
using(var socks4ProxyClient = new Socks4ProxyClient("212.86.75.9", 4153))
{
    var response = await socks4ProxyClient.GetAsync("https://www.example.com/", totalTimeout: 10000, readTimeout: 1000, writeTimeout: 1000);
}
```
### Keep-Alive Support
By default, all connections are persistent.
To force the connection/socket closure, set the isKeepAlive flag to False:
```C#
using(var socks4ProxyClient = new Socks4ProxyClient("212.86.75.9", 4153))
{
    var response = await socks4ProxyClient.GetAsync("https://www.example.com/", isKeepAlive: False);
}
```
## Contribution
Feel free to [open an issue](https://github.com/bokklu/proxy-client/issues) or submit a [pull request](https://github.com/bokklu/Proxy.Client/pulls).
