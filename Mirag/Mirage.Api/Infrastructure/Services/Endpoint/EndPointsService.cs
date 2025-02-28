using Microsoft.AspNetCore.Mvc;
using Mirage.Api.Common;
using Mirage.Api.Infrastructure.Services.Endpoint.Dto;
using System.Reflection;

namespace Mirage.Api.Infrastructure.Services.Endpoint
{
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
                        continue;
                    }

                    var httpVerb = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
                    var httpMethods = httpVerb?.HttpMethods ?? new List<string> { "UNKNOWN" };
                    var route = routeEndpoint.RoutePattern.RawText ?? "UNKNOWN";

                    var controllerActionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();

                    if (controllerActionDescriptor == null)
                    {
                        continue;
                    }

                    var methodInfo = controllerActionDescriptor.MethodInfo;
                    var returnType = GetReturnType(methodInfo.ReturnType);
                    var returnTypename = GetReturnTypeName(methodInfo.ReturnType);
                    var parameters = methodInfo.GetParameters()
                        .Select(param => new ParameterDetail
                        {
                            Name = param.Name ?? "UNKNOWN",
                            Type = param.ParameterType,
                            ModelBinding = GetBindingSource(param)
                        })
                        .ToList();

                    result.Add(new MyRoute(route, httpMethods, returnType, returnTypename, parameters));
                }
            }

            return result;
        }

        private Type GetReturnType(Type returnType)
        {
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return returnType.GetGenericArguments()[0];
            }

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                return returnType.GetGenericArguments()[0];
            }

            return returnType;
        }
        private string GetReturnTypeName(Type returnType)
        {
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return $"Task<{GetNameCompletely(returnType.GetGenericArguments()[0])}>";
            }

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                return $"ActionResult<{GetNameCompletely(returnType.GetGenericArguments()[0])}>";
            }

            return GetNameCompletely(returnType);
        }

        private string GetNameCompletely(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeName = type.GetGenericTypeDefinition().Name;
                int index = genericTypeName.IndexOf('`');
                if (index > 0)
                {
                    genericTypeName = genericTypeName.Substring(0, index);
                }

                string genericArgs = string.Join(", ", type.GetGenericArguments().Select(t => GetNameCompletely(t)));
                return $"{genericTypeName}<{genericArgs}>";
            }
            return type.Name;
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
}