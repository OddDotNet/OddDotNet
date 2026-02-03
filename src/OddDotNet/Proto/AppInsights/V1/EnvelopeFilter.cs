using OddDotNet.Filters;

namespace OddDotNet.Proto.AppInsights.V1;

public sealed partial class EnvelopeFilter
{
    public bool Matches(TelemetryEnvelope envelope) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.InstrumentationKey => StringFilter.Matches(envelope.InstrumentationKey, InstrumentationKey),
        ValueOneofCase.Time => StringFilter.Matches(envelope.Time, Time),
        ValueOneofCase.Context => Context.Matches(envelope.Context),
        _ => false
    };
}

public sealed partial class ContextFilter
{
    public bool Matches(TelemetryContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Operation => Operation.Matches(context.Operation),
        ValueOneofCase.Cloud => Cloud.Matches(context.Cloud),
        ValueOneofCase.Device => Device.Matches(context.Device),
        ValueOneofCase.User => User.Matches(context.User),
        ValueOneofCase.Session => Session.Matches(context.Session),
        ValueOneofCase.Location => Location.Matches(context.Location),
        ValueOneofCase.Application => Application.Matches(context.Application),
        ValueOneofCase.Internal => Internal.Matches(context.Internal),
        _ => false
    };
}

public sealed partial class OperationContextFilter
{
    public bool Matches(OperationContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(context.Id, Id),
        ValueOneofCase.ParentId => StringFilter.Matches(context.ParentId, ParentId),
        ValueOneofCase.Name => StringFilter.Matches(context.Name, Name),
        ValueOneofCase.SyntheticSource => StringFilter.Matches(context.SyntheticSource, SyntheticSource),
        ValueOneofCase.CorrelationVector => StringFilter.Matches(context.CorrelationVector, CorrelationVector),
        _ => false
    };
}

public sealed partial class CloudContextFilter
{
    public bool Matches(CloudContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.RoleName => StringFilter.Matches(context.RoleName, RoleName),
        ValueOneofCase.RoleInstance => StringFilter.Matches(context.RoleInstance, RoleInstance),
        _ => false
    };
}

public sealed partial class DeviceContextFilter
{
    public bool Matches(DeviceContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(context.Id, Id),
        ValueOneofCase.Type => StringFilter.Matches(context.Type, Type),
        ValueOneofCase.OsVersion => StringFilter.Matches(context.OsVersion, OsVersion),
        _ => false
    };
}

public sealed partial class UserContextFilter
{
    public bool Matches(UserContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(context.Id, Id),
        ValueOneofCase.AuthenticatedId => StringFilter.Matches(context.AuthenticatedId, AuthenticatedId),
        ValueOneofCase.AccountId => StringFilter.Matches(context.AccountId, AccountId),
        _ => false
    };
}

public sealed partial class SessionContextFilter
{
    public bool Matches(SessionContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(context.Id, Id),
        ValueOneofCase.IsFirst => BoolFilter.Matches(context.IsFirst, IsFirst),
        _ => false
    };
}

public sealed partial class LocationContextFilter
{
    public bool Matches(LocationContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Ip => StringFilter.Matches(context.Ip, Ip),
        ValueOneofCase.Country => StringFilter.Matches(context.Country, Country),
        ValueOneofCase.Province => StringFilter.Matches(context.Province, Province),
        ValueOneofCase.City => StringFilter.Matches(context.City, City),
        _ => false
    };
}

public sealed partial class ApplicationContextFilter
{
    public bool Matches(ApplicationContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Version => StringFilter.Matches(context.Version, Version),
        _ => false
    };
}

public sealed partial class InternalContextFilter
{
    public bool Matches(InternalContext context) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.SdkVersion => StringFilter.Matches(context.SdkVersion, SdkVersion),
        ValueOneofCase.AgentVersion => StringFilter.Matches(context.AgentVersion, AgentVersion),
        ValueOneofCase.NodeName => StringFilter.Matches(context.NodeName, NodeName),
        _ => false
    };
}
