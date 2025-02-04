using Microsoft.OpenApi.Models;
using Mirage.Api.Common;
using Mirage.Api.Infrastructure.Services.Endpoint;
using Mirage.Api.Infrastructure.Services.ObjectGenerator;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

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
            var routes = await _endPointsService.GetList();

            if (!routes.Any())
            {
                return;
            }

            var wireMockServer = WireMockServer.Start(9090);

            foreach (var rout in routes)
            {
                var request = Request.Create()
                                        .WithPath(rout.Route)
                                        .UsingMethod(rout.HttpMethods.First());
                var responce = Response.Create()
                                .WithStatusCode(200);
                foreach (var parameter in rout.Parameters)
                {
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Query.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithParam(parameter.Name);
                        continue;
                    }
                    else
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Body.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithBody(parameter.Name);
                        continue;
                    }
                    else
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Route.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithPath(parameter.Name);
                        continue;
                    }
                    else
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Header.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithHeader(parameter.Name);
                        continue;
                    }
                    else
                        continue;

                    if (string.IsNullOrEmpty(rout.ReturnType))
                    {
                        continue;
                    }

                    var responcsObj = FakerService.CreateInstance(rout.ReturnType);
                    responce.WithBody(responcsObj);
                }



                wireMockServer.Given(request)
                              .RespondWith(responce);
                _logger.LogInformation(rout.Route);
            }
        }
    }
}