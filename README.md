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

## Unified Query API (`/query/v1/*`)

Starting in 0.5.0, every signal type — OTLP and App Insights — exposes the same HTTP query shape. Existing gRPC query services and the `/appinsights/*` endpoints continue to work and are preserved as aliases.

**Paths (one per signal type):**

| OTLP                   | App Insights                      |
| ---------------------- | --------------------------------- |
| `/query/v1/spans`      | `/query/v1/appinsights/requests`       |
| `/query/v1/metrics`    | `/query/v1/appinsights/dependencies`   |
| `/query/v1/logs`       | `/query/v1/appinsights/exceptions`     |
|                        | `/query/v1/appinsights/traces`         |
|                        | `/query/v1/appinsights/events`         |
|                        | `/query/v1/appinsights/metrics`        |
|                        | `/query/v1/appinsights/pageviews`      |
|                        | `/query/v1/appinsights/availability`   |

**`POST /query/v1/{signal}`** — body is the corresponding gRPC `XxxQueryRequest` message serialized as JSON (via Google.Protobuf `JsonFormatter`), reusing the existing filter proto. `Content-Type: application/json` is required. The `duration` field long-polls up to N ms for matches to arrive (same semantics as gRPC).

Response:
```json
{
  "items": [ /* FlatXxx JSON */ ],
  "count": 3,
  "truncated": false
}
```

`truncated` is `true` when the configured `take` cap was reached.

**`DELETE /query/v1/{signal}`** — clears that signal's store. Returns `204 No Content`.

**`DELETE /query/v1/all`** — clears every signal type in one call. Returns `204 No Content`.

**`GET /query/v1/{signal}?...`** (added in 0.5.1) — equality-only shorthand for the 90% case. Maps query-string params onto the same filter proto that POST uses. Response shape is identical to POST.

Example curls:
```
curl 'http://localhost:PORT/query/v1/spans?name=checkout&take=all&wait_ms=500'
curl 'http://localhost:PORT/query/v1/spans?attr.service.name=svc-a&take=1'
curl 'http://localhost:PORT/query/v1/appinsights/requests?id=req-123&take=all'
curl 'http://localhost:PORT/query/v1/logs?trace_id=aabbccddeeff00112233445566778899'
```

Reserved params:

| Param     | Meaning                                                                 |
| --------- | ----------------------------------------------------------------------- |
| `take`    | `N` (int, 0 returns empty), `all`, or `first`. Default `first`.         |
| `wait_ms` | Long-poll window in ms. `0` = snapshot. Clamped to `[0, 60000]`; out of range → `400`. |

Filter params:

- `{field}=value` — equality filter on a top-level proto field (e.g. `name`, `trace_id`, `id`). Field names are snake_case, matching the `.proto` definitions.
- `attr.{k}=v` — equality filter against attributes. The **first** dot after `attr` is the separator; everything after it becomes the attribute key, so `attr.service.name=svc-a` filters on key `service.name`.
- Multiple filter params AND together.
- Binary fields (`trace_id`, `span_id`, etc.) are hex-encoded in the query string.
- Attribute filtering is not supported on `/query/v1/metrics` in phase 2 (metric attributes live on data points, not the metric top-level).

Errors:

| Condition                                    | Status |
| -------------------------------------------- | ------ |
| Unknown signal path                          | 404    |
| Unknown field for a known signal             | 400    |
| Malformed `take` or `wait_ms`                | 400    |
| `wait_ms` out of `[0, 60000]`                | 400    |
| Malformed hex in `trace_id` / `span_id`      | 400    |

**Shared errors (all verbs):** `404` for unknown signal paths; `415` for `POST` without `application/json`; `400` for malformed JSON or invalid filters.

Phase-3 streaming (`/query/v1/{signal}/stream` via SSE / NDJSON) is a planned follow-up; see `rest_query_surface.md`.

## Tools and Setup
### Git Submodules
This repository makes use of `git submodule`s to clone down the proto files from GitHub, located
[here](https://github.com/open-telemetry/opentelemetry-proto).

When cloning down the repo, you'll need to `git clone --recurse-submodules` to pull in the proto
file git repo.

### .NET Aspire
This project makes use of .NET Aspire for testing. Follow the instructions located [here](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling?tabs=linux&pivots=dotnet-cli)
