using Microsoft.AspNetCore.Routing;
using Mirage.Api.Common;
using Mirage.Api.Infrastructure.Services.Endpoint;
using Mirage.Api.Infrastructure.Services.ObjectGenerator;
using System.Text;
using System.Text.RegularExpressions;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace Mirage.Api.Infrastructure.Services.MockServer
{
    public class MockServerService
    {
        private readonly EndPointsService _endPointsService;
        private readonly ILogger<EndPointsService> _logger;

        public MockServerService(EndPointsService endPointsService, ILogger<EndPointsService> logger)
        {
            _endPointsService = endPointsService;
            _logger = logger;
        }

        public async Task ConfigServer()
        {
            try
            {
                var routes = await _endPointsService.GetList();

                if (!routes.Any())
                {
                    return;
                }

                var wireMockServer = WireMockServer.Start(new WireMockServerSettings
                {
                    Port = 9090,
                    StartAdminInterface = true,
                });

                foreach (var route in routes)
                {
                    var rawPath = $"/{route.Route}";
                    string pattern = @"\{[^}]*\}";
                    string wildcardPath = Regex.Replace(rawPath, pattern, ".*");

                    var request = Request.Create()
                                         .WithPath(new RegexMatcher($"^{wildcardPath}"))
                                         .SetUsingHttpType(route.HttpMethods);


                    foreach (var parameter in route.Parameters)
                    {
                        if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Query.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            request.WithParam(parameter.Name, new RegexMatcher(".*"));
                        }
                        else
                        if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Body.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            request.WithBody(new WildcardMatcher("*"));
                        }
                        else
                        if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Route.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            request.WithPath(parameter.Name, "*");
                        }
                        else
                        if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Header.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            request.WithHeader(parameter.Name);
                        }
                        else
                        {
                            throw new NotImplementedException("");
                        }
                    }

                    object responcsObj;

                    if (string.IsNullOrEmpty(route.ReturnTypeName))
                    {
                        responcsObj = $"This is for test FOR http://localhost:9090/{route.Route}. Son of Bitch.";
                    }
                    else
                    {
                        if (route.SampleType is null)
                        {
                            responcsObj = FakerService.CreateMockInstance(route.ReturnType);

                        }
                        else
                        if (typeof(MyAbstractClass).IsAssignableFrom(route.SampleType) && route.SampleType != typeof(MyAbstractClass))
                       {
                                var instancess = Activator.CreateInstance(route.SampleType) as MyAbstractClass;
                                responcsObj = instancess.InstanceMethod();
                            }
                        else
                        {
                            responcsObj = FakerService.CreateMockInstance(route.ReturnType);

                        }
                    }

                    var response = Response.Create()
                                           .WithStatusCode(200)
                                           .WithHeader("Content-Type", "application/json")
                                           .WithBodyAsJson(responcsObj, Encoding.UTF8, true);

                    wireMockServer.Given(request)
                                  .RespondWith(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }

    public static class WireMockExtension
    {
        public static IRequestBuilder SetUsingHttpType(this IRequestBuilder request, IEnumerable<string> httpVerbType)
        {
            switch (httpVerbType.FirstOrDefault().ToLowerInvariant())
            {
                case "get": return request.UsingGet();
                case "post": return request.UsingPost();
                case "put": return request.UsingPut();
                case "delete":
                case "del": return request.UsingDelete();
                default: throw new ArgumentException($"Unsupported HTTP verb: {httpVerbType}");
            }
        }
    }
}