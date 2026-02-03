# OddDotNet

OddDotNet stands for Observability Driven Development (ODD) in .NET.

## Description
OddDotNet is a test harness for ODD. It includes an OTLP receiver that supports grpc (with http/protobuf and http/json on the roadmap).

This project is in active development. Please continue to check back often. Spans/Traces, Metrics, and LogRecords are all supported. Profiles are being actively developed.

## Supported Telemetry Formats

### OpenTelemetry (OTLP)
OddDotNet accepts OpenTelemetry telemetry via gRPC:
- **Traces/Spans** - Full span querying with filtering
- **Metrics** - Metric data point querying
- **Logs** - Log record querying

### Microsoft Application Insights
OddDotNet also accepts Application Insights telemetry via the `/v2/track` HTTP endpoint, enabling validation of App Insights to OpenTelemetry migrations.

**Supported telemetry types:**
- **Request** - HTTP request telemetry
- **Dependency** - External dependency calls (HTTP, SQL, etc.)
- **Exception** - Exception/error telemetry
- **Trace** - Log/trace messages
- **Event** - Custom events
- **Metric** - Custom metrics
- **PageView** - Page view telemetry
- **Availability** - Availability/health check results

**Ingestion endpoint:** `POST /v2/track`

Accepts single JSON objects, JSON arrays, or newline-delimited JSON (NDJSON).

**Query services:** Each telemetry type has a corresponding gRPC query service (e.g., `RequestQueryService`, `DependencyQueryService`) that supports filtering by:
- Telemetry-specific properties (id, name, success, responseCode, etc.)
- Common context fields (operation, cloud, user, session, device, location)
- Custom properties and measurements

## Tools and Setup
### Git Submodules
This repository makes use of `git submodule`s to clone down the proto files from GitHub, located
[here](https://github.com/open-telemetry/opentelemetry-proto).

When cloning down the repo, you'll need to `git clone --recurse-submodules` to pull in the proto
file git repo.

### .NET Aspire
This project makes use of .NET Aspire for testing. Follow the instructions located [here](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling?tabs=linux&pivots=dotnet-cli)
