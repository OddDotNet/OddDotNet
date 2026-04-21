# OddDotNet

OddDotNet stands for Observability Driven Development (ODD) in .NET.

## Description
OddDotNet is a test harness for ODD. It includes an OTLP receiver that supports gRPC, OTLP/HTTP with binary protobuf (`application/x-protobuf`), and OTLP/HTTP with JSON-encoded protobuf (`application/json`). HTTP endpoints: `POST /v1/traces`, `POST /v1/metrics`, `POST /v1/logs`. Gzip and deflate request bodies are accepted.

This project is in active development. Please continue to check back often. Spans/Traces, Metrics, and LogRecords are all supported. Profiles are being actively developed.

## Supported Telemetry Formats

### OpenTelemetry (OTLP)
OddDotNet accepts OpenTelemetry telemetry via gRPC and OTLP/HTTP (`/v1/traces`, `/v1/metrics`, `/v1/logs`, both `application/x-protobuf` and `application/json`, with optional gzip/deflate):
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

**Ingestion endpoint:** `POST /v2/track` (also `/v2.1/track`)

Accepts single JSON objects, JSON arrays, newline-delimited JSON (NDJSON), and gzip-compressed payloads.

**REST Query Endpoints:**
```
GET /appinsights              # Summary with counts of all telemetry types
GET /appinsights/requests     # All request telemetry
GET /appinsights/dependencies # All dependency telemetry
GET /appinsights/exceptions   # All exception telemetry
GET /appinsights/traces       # All trace/log telemetry
GET /appinsights/events       # All custom event telemetry
GET /appinsights/metrics      # All metric telemetry
GET /appinsights/pageviews    # All page view telemetry
GET /appinsights/availability # All availability telemetry
```

**gRPC Query Services:** Each telemetry type also has a corresponding gRPC query service (e.g., `RequestQueryService`, `DependencyQueryService`) that supports filtering by:
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
