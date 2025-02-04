using Microsoft.AspNetCore.Mvc;
using Mirage.Api.Common;
using Mirage.Api.Infrastructure.Services.Endpoint.Dto;
using System.Reflection;

namespace Mirage.Api.Infrastructure.Services.Endpoint;

public class EndPointsService
{
    private readonly IEnumerable<EndpointDataSource> _endpointSources;
    private readonly ILogger<EndPointsService> _logger;

    public EndPointsService(IEnumerable<EndpointDataSource> endpointSources, ILogger<EndPointsService> logger)
    {
        _endpointSources = endpointSources;
        _logger = logger;
    }

    public async Task<IEnumerable<MyRoute>> GetList()
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

                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();

                if (controllerActionDescriptor == null)
                {
                    _logger.LogWarning($"ControllerActionDescriptor not found for route: {route}");
                    continue;
                }

                var methodInfo = controllerActionDescriptor.MethodInfo;
                var returnType = GetReturnType(methodInfo.ReturnType);
                var parameters = methodInfo.GetParameters()
                    .Select(param => new ParameterDetail
                    {
                        Name = param.Name ?? "UNKNOWN",
                        Type = param.ParameterType,
                        ModelBinding = GetBindingSource(param)
                    })
                    .ToList();

                result.Add(new MyRoute(route, httpMethods, returnType, parameters));
            }
        }

        return result;
    }

    private string GetReturnType(Type returnType)
    {
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return $"Task<{returnType.GetGenericArguments()[0].Name}>";
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            return $"ActionResult<{returnType.GetGenericArguments()[0].Name}>";
        }

        return returnType.Name;
    }

    private ModelBindingType GetBindingSource(ParameterInfo parameter)
    {
        var attributes = parameter.GetCustomAttributes();
        if (attributes.OfType<FromQueryAttribute>().Any()) return ModelBindingType.Query;
        if (attributes.OfType<FromBodyAttribute>().Any()) return ModelBindingType.Body;
        if (attributes.OfType<FromRouteAttribute>().Any()) return ModelBindingType.Body;
        if (attributes.OfType<FromHeaderAttribute>().Any()) return ModelBindingType.Header;
        if (attributes.OfType<FromFormAttribute>().Any()) return ModelBindingType.Form;
        return ModelBindingType.UNKOWN;
    }
}