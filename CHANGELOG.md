## 0.1.9 (07.12.2018)

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