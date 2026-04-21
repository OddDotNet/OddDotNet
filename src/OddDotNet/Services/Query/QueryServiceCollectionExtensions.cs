using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services.Query.Shorthand;

using AiFlatRequest = OddDotNet.Proto.AppInsights.V1.Request.FlatRequest;
using AiFlatDependency = OddDotNet.Proto.AppInsights.V1.Dependency.FlatDependency;
using AiFlatException = OddDotNet.Proto.AppInsights.V1.Exception.FlatException;
using AiFlatTrace = OddDotNet.Proto.AppInsights.V1.Trace.FlatTrace;
using AiFlatEvent = OddDotNet.Proto.AppInsights.V1.Event.FlatEvent;
using AiFlatMetric = OddDotNet.Proto.AppInsights.V1.Metric.FlatMetric;
using AiFlatPageView = OddDotNet.Proto.AppInsights.V1.PageView.FlatPageView;
using AiFlatAvailability = OddDotNet.Proto.AppInsights.V1.Availability.FlatAvailability;

using AiRequestQueryRequest = OddDotNet.Proto.AppInsights.V1.Request.RequestQueryRequest;
using AiDependencyQueryRequest = OddDotNet.Proto.AppInsights.V1.Dependency.DependencyQueryRequest;
using AiExceptionQueryRequest = OddDotNet.Proto.AppInsights.V1.Exception.ExceptionQueryRequest;
using AiTraceQueryRequest = OddDotNet.Proto.AppInsights.V1.Trace.TraceQueryRequest;
using AiEventQueryRequest = OddDotNet.Proto.AppInsights.V1.Event.EventQueryRequest;
using AiMetricQueryRequest = OddDotNet.Proto.AppInsights.V1.Metric.MetricQueryRequest;
using AiPageViewQueryRequest = OddDotNet.Proto.AppInsights.V1.PageView.PageViewQueryRequest;
using AiAvailabilityQueryRequest = OddDotNet.Proto.AppInsights.V1.Availability.AvailabilityQueryRequest;

namespace OddDotNet.Services.Query;

public static class QueryServiceCollectionExtensions
{
    public static IServiceCollection AddQuerySurface(this IServiceCollection services)
    {
        // OTLP
        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<SpanQueryRequest, FlatSpan>(
            "spans",
            sp.GetRequiredService<SignalList<FlatSpan>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new SpanShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<MetricQueryRequest, FlatMetric>(
            "metrics",
            sp.GetRequiredService<SignalList<FlatMetric>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new MetricShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<LogQueryRequest, FlatLog>(
            "logs",
            sp.GetRequiredService<SignalList<FlatLog>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new LogShorthandBuilder()));

        // App Insights
        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiRequestQueryRequest, AiFlatRequest>(
            "appinsights/requests",
            sp.GetRequiredService<SignalList<AiFlatRequest>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiRequestShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiDependencyQueryRequest, AiFlatDependency>(
            "appinsights/dependencies",
            sp.GetRequiredService<SignalList<AiFlatDependency>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiDependencyShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiExceptionQueryRequest, AiFlatException>(
            "appinsights/exceptions",
            sp.GetRequiredService<SignalList<AiFlatException>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiExceptionShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiTraceQueryRequest, AiFlatTrace>(
            "appinsights/traces",
            sp.GetRequiredService<SignalList<AiFlatTrace>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiTraceShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiEventQueryRequest, AiFlatEvent>(
            "appinsights/events",
            sp.GetRequiredService<SignalList<AiFlatEvent>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiEventShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiMetricQueryRequest, AiFlatMetric>(
            "appinsights/metrics",
            sp.GetRequiredService<SignalList<AiFlatMetric>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiMetricShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiPageViewQueryRequest, AiFlatPageView>(
            "appinsights/pageviews",
            sp.GetRequiredService<SignalList<AiFlatPageView>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiPageViewShorthandBuilder()));

        services.AddSingleton<ISignalQueryHandler>(sp => new SignalQueryHandler<AiAvailabilityQueryRequest, AiFlatAvailability>(
            "appinsights/availability",
            sp.GetRequiredService<SignalList<AiFlatAvailability>>(),
            r => r.Take, r => r.Duration, r => r.Filters,
            new AiAvailabilityShorthandBuilder()));

        services.AddSingleton<QueryDispatcher>();

        return services;
    }
}
