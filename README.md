# Vostok.ClusterClient.Transport

[![Build status](https://ci.appveyor.com/api/projects/status/github/vostok/clusterclient.transport?svg=true&branch=master)](https://ci.appveyor.com/project/vostok/clusterclient.transport/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Vostok.ClusterClient.Transport.svg)](https://www.nuget.org/packages/Vostok.ClusterClient.Transport)

Universal transport implementation targeting netstandard2.0 and intended to be used by library developers.

Delegates to following implementations depending on runtime:

- [Vostok.ClusterClient.Transport.Webrequest](https://github.com/vostok/clusterclient.transport.webrequest) on .NET Framework
- [Vostok.ClusterClient.Transport.Native](https://github.com/vostok/clusterclient.transport.native) on .NET Core 2.0
- [Vostok.ClusterClient.Transport.Sockets](https://github.com/vostok/clusterclient.transport.sockets) on .NET Core 2.1+
