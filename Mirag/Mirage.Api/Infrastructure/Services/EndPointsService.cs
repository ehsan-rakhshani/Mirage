using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Mirage.Api.Infrastructure.Services;

public class EndPointsService
{
    private readonly IEnumerable<EndpointDataSource> _endpointSources;
    private readonly ILogger<EndPointsService> _logger;

    public EndPointsService(IEnumerable<EndpointDataSource> endpointSources, ILogger<EndPointsService> logger)
    {
        _endpointSources = endpointSources;
        _logger = logger;
    }

    public async Task GetList()
    {
        await Task.Delay(2000);
        var result = new List<MyRoute>();

        foreach (var item in _endpointSources)
        {
            _logger.LogInformation($"Processing EndpointDataSource: {item.GetType().Name}");

            foreach (var endpoint in item.Endpoints)
            {
                if (endpoint is not RouteEndpoint routeEndpoint)
                {
                    _logger.LogWarning("Skipping non-RouteEndpoint.");
                    continue;
                }

                var httpVerb = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
                var httpMethods = httpVerb?.HttpMethods ?? new List<string> { "UNKNOWN" };
                var route = routeEndpoint.RoutePattern.RawText ?? "UNKNOWN";

                _logger.LogInformation($"Found Route: {route} - Methods: {string.Join(", ", httpMethods)}");

                // دریافت متد کنترلر مربوطه
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();

                if (controllerActionDescriptor == null)
                {
                    _logger.LogWarning($"ControllerActionDescriptor not found for route: {route}");
                    continue;
                }

                var methodInfo = controllerActionDescriptor.MethodInfo;
                var returnType = GetReturnType(methodInfo.ReturnType); // استخراج نوع خروجی

                var parameters = methodInfo.GetParameters()
                    .Select(param => new ParameterDetail
                    {
                        Name = param.Name ?? "UNKNOWN",
                        Type = param.ParameterType.Name,
                        BindingSource = GetBindingSource(param)
                    })
                    .ToList();

                result.Add(new MyRoute(route, httpMethods, returnType, parameters));
            }
        }

        // نمایش خروجی در کنسول
        foreach (var route in result)
        {
            Console.WriteLine($"Route: {route.Route}");
            Console.WriteLine($"Methods: {string.Join(", ", route.HttpMethods)}");
            Console.WriteLine($"Return Type: {route.ReturnType}");
            Console.WriteLine("Parameters:");
            foreach (var param in route.Parameters)
            {
                Console.WriteLine($"  - {param.Name}: {param.Type} (Binding: {param.BindingSource})");
            }
            Console.WriteLine(new string('-', 50));
        }
    }

    private string GetReturnType(Type returnType)
    {
        // بررسی اینکه آیا نوع خروجی `Task<T>` است
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return $"Task<{returnType.GetGenericArguments()[0].Name}>";
        }

        // بررسی اینکه آیا نوع خروجی `ActionResult<T>` است
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            return $"ActionResult<{returnType.GetGenericArguments()[0].Name}>";
        }

        return returnType.Name;
    }

    private string GetBindingSource(ParameterInfo parameter)
    {
        var attributes = parameter.GetCustomAttributes();
        if (attributes.OfType<FromQueryAttribute>().Any()) return "Query";
        if (attributes.OfType<FromBodyAttribute>().Any()) return "Body";
        if (attributes.OfType<FromRouteAttribute>().Any()) return "Route";
        if (attributes.OfType<FromHeaderAttribute>().Any()) return "Header";
        if (attributes.OfType<FromFormAttribute>().Any()) return "Form";
        return "Default";
    }
}

public record MyRoute(string Route, IEnumerable<string> HttpMethods, string ReturnType, List<ParameterDetail> Parameters);

public class ParameterDetail
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string BindingSource { get; set; } = "";
}
