## 0.1.17 (25.12.2020):

Connection error detector is now a bit more conservative about returning ConnectionFailure code to avoid cases where the calling code is led to conclude that the request has not been actually sent over the wire. ReceiveFailure may now be returned in such circumstances. 

## 0.1.16 (23.12.2020):

Added support for UTF-8 header values on .NET 5+

## 0.1.15 (10.12.2020):

Fixed NRE in ssl options configuration.

## 0.1.14 (30.10.2020):

Fixed lost headers on SocketTuningContent creation.

## 0.1.13 (29.04.2020):

Fixed SocketsTransport not being able to read response bodies larger than 2 GB when response does not use chunked transfer encoding. 

## 0.1.12 (03.03.2020):

* Gathered all transport implementations in this module + retargeted everything to netstandard2.0.
* UniversalTransport no longer breaks down after ILRepack.

## 0.1.11 (04.02.2020):

* Improved detection of connection errors in SocketTransport.
* Implemented https://github.com/vostok/clusterclient.transport/issues/1

## 0.1.10 (18.01.2020)

Incorporated latest changes in SocketTransport (0.1.7) and WebRequestTransport (0.1.7).

## 0.1.9 (07.12.2019)

Incorporated changes from release 0.1.6 of SocketTransport.

## 0.1.8 (04.12.2019)

Explicitly disabled ARP cache warmup for WebRequestTransport and SocketTransport implementations.

## 0.1.7 (15-08-2019):

Fixed reception of Content-Length header without body in response to a HEAD request.

## 0.1.6 (14-08-2019):

Fixed a bug where a network error while reading content could cause the transport to return a response with headers or partial body.

## 0.1.5 (20-03-2019): 

UniversalTransportSettings now expose a configurable response buffer factory function.

## 0.1.3 (03-03-2019): 

UniversalTransport now supports composite request bodies.

## 0.1.2 (06-02-2019): 

Added SetupUniversalTransport() extension for IClusterClientConfiguration.

## 0.1.1 (04-02-2019): 

Fixed NuGet package for .NET Core broken due to a mix of Cement and NuGet references.

## 0.1.0 (04-02-2019): 

Initial prerelease.