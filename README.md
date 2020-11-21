![Proxy.Client](https://i.imgur.com/5HM1SLu.png)

# Proxy.Client ![Build](https://github.com/bokklu/proxy-client/workflows/Build/badge.svg)

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

## Contribution
Feel free to [open an issue](https://github.com/bokklu/proxy-client/issues) or submit a [pull request](https://github.com/bokklu/Proxy.Client/pulls).
