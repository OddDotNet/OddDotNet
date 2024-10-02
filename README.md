# OddDotNet

OddDotNet stands for Observability Driven Development (ODD) in .NET.

## Description
OddDotNet is a test harness for ODD. It includes an OTLP receiver that supports grpc (with http/protobuf and http/json on the roadmap).

This project is in active development. Please continue to check back often. Spans/traces are currently being worked. Metrics and LogRecords
are on the roadmap. 

## Tools and Setup
### Git Submodules
This repository makes use of `git submodule`s to clone down the proto files from GitHub, located
[here](https://github.com/open-telemetry/opentelemetry-proto).

When cloning down the repo, you'll need to `git clone --recurse-submodules` to pull in the proto
file git repo.

### .NET Aspire
This project makes use of .NET Aspire. Follow the instructions located [here](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling?tabs=linux&pivots=dotnet-cli)
