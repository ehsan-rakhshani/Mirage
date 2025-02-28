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
        private readonly FakerService _fakerService;
        private readonly ILogger<EndPointsService> _logger;
        public MockServerService(EndPointsService endPointsService, FakerService fakerService, ILogger<EndPointsService> logger)
        {
            _endPointsService = endPointsService;
            _logger = logger;
            _fakerService = fakerService;
        }
        public async Task ConfigServer()
        {
            var routes = await _endPointsService.GetList();

            if (!routes.Any())
            {
                return;
            }

            var wireMockServer = WireMockServer.Start(9090);
            var r = Request.Create()
                           .WithPath("/IsAlive")
                           .UsingMethod("GET");
            var t = Response.Create()
                  .WithStatusCode(200)
                  .WithHeader("Content-Type", "text/plain")
                  .WithBody("I'm alive MOTHERFUCKER.");
            wireMockServer
              .Given(r)
              .RespondWith(t);

            foreach (var rout in routes)
            {
                var request = Request.Create()
                                     .WithPath($"/{rout.Route}")
                                     .UsingMethod(rout.HttpMethods.First());
                var responce = Response.Create()
                                       .WithStatusCode(200)
                                       .WithHeader("Content-Type", "text/plain");
                foreach (var parameter in rout.Parameters)
                {
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Query.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithParam(parameter.Name);
                    }
                    else
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Body.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithBody(parameter.Name);
                    }
                    else
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Route.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithPath(parameter.Name);
                    }
                    else
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Header.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithHeader(parameter.Name);
                    }
                }

                string responcsObj = string.Empty;
                if (string.IsNullOrEmpty(rout.ReturnTypeName))
                {
                    responcsObj = $"This is for test FOR http://localhost:9090/{rout.Route}. Son of Bitch.";
                }
                else
                {
                    responcsObj = _fakerService.CreateFakeData(rout.ReturnType);
                }

                responce.WithBody(responcsObj);
                wireMockServer.Given(request)
                              .RespondWith(responce);
                _logger.LogInformation(rout.Route);
            }
        }
    }
}