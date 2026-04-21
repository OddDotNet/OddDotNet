using Microsoft.AspNetCore.Http;

namespace OddDotNet.Services.Query.Shorthand;

public interface IShorthandFilterBuilder<TRequest>
{
    TRequest Build(IQueryCollection query);
}
