namespace Mirage.Api.Infrastructure.Services.Endpoint.Dto;

public record MyRoute(string Route, IEnumerable<string> HttpMethods, string ReturnType, List<ParameterDetail> Parameters);