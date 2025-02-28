using Mirage.Api.Infrastructure.Services.Endpoint.Dto;

public class MyRoute
{
    public string Route { get; }
    public IEnumerable<string> HttpMethods { get; }
    public Type ReturnType { get; }
    public string ReturnTypeName { get; }
    public List<ParameterDetail> Parameters { get; }

    public MyRoute(string route, IEnumerable<string> httpMethods, Type returnType, string returnTypeName, List<ParameterDetail> parameters)
    {
        Route = route;
        HttpMethods = httpMethods;
        ReturnType = returnType;
        Parameters = parameters;
        ReturnTypeName = returnTypeName;
    }
}