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

            //        wireMockServer
            //.Given(
            //    Request.Create()
            //        .WithPath("/api/owners/*/test")
            //        .UsingGet()
            //)
            //.RespondWith(
            //    Response.Create()
            //        .WithStatusCode(200)
            //        .WithHeader("Content-Type", "application/json")
            //        .WithBody(@"
            //            {
            //                ""id"": ""afe6cf9a-eca3-496e-a1a9-83fee0132f05"",
            //                ""firstName"": ""Ehsan"",
            //                ""lastName"": ""Rakhshani"",
            //                ""phone"": ""09365957533""
            //            }
            //        ")
            //);

            //        // 2️⃣ GET api/owners?id=GUID (query string)
            //        wireMockServer
            //            .Given(
            //                Request.Create()
            //                    .WithPath("/api/owners")
            //                    .WithParam("id", new RegexMatcher(".*")) // هر GUIDی قبول میشه
            //                    .UsingGet()
            //            )
            //            .RespondWith(
            //                Response.Create()
            //                    .WithStatusCode(200)
            //                    .WithHeader("Content-Type", "application/json")
            //                    .WithBody(@"
            //            {
            //                ""id"": ""b72ee075-d8f2-4ca5-96d8-a8d687aa2ea6"",
            //                ""firstName"": ""Mohammad"",
            //                ""lastName"": ""Asghar"",
            //                ""phone"": ""09365957534""
            //            }
            //        ")
            //            );

            //        // 3️⃣ GET api/owners/AllGetOwners (لیست کامل)
            //        wireMockServer
            //            .Given(
            //                Request.Create()
            //                    .WithPath("/api/owners/AllGetOwners")
            //                    .UsingGet()
            //            )
            //            .RespondWith(
            //                Response.Create()
            //                    .WithStatusCode(200)
            //                    .WithHeader("Content-Type", "application/json")
            //                    .WithBody(@"
            //            [
            //                {
            //                    ""id"": ""afe6cf9a-eca3-496e-a1a9-83fee0132f05"",
            //                    ""firstName"": ""Ehsan"",
            //                    ""lastName"": ""Rakhshani"",
            //                    ""phone"": ""09365957533""
            //                },
            //                {
            //                    ""id"": ""b72ee075-d8f2-4ca5-96d8-a8d687aa2ea6"",
            //                    ""firstName"": ""Mohammad"",
            //                    ""lastName"": ""Asghar"",
            //                    ""phone"": ""09365957534""
            //                },
            //                {
            //                    ""id"": ""dfe891ff-331b-490e-b407-ab0980f89bbc"",
            //                    ""firstName"": ""Akbar"",
            //                    ""lastName"": ""Ahmad"",
            //                    ""phone"": ""09345957535""
            //                }
            //            ]
            //        ")
            //            );

            //        Console.WriteLine("✅ Mock server is running on http://localhost:5000");
            //        Console.ReadLine();

            foreach (var rout in routes)
            {
                var uri = rout.Route;

                string pattern = @"\{[^}]*\}";
                uri = Regex.Replace($"/{uri}", pattern, "*");

                var request = Request.Create()
                                     .WithPath(uri)
                                     .UsingMethod(rout.HttpMethods.First());

                foreach (var parameter in rout.Parameters)
                {
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Query.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithParam(parameter.Name, new RegexMatcher(".*"));
                    }
                    else
                    if (parameter.ModelBinding.ToString().Equals(ModelBindingType.Body.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithBody(parameter.Name);
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
               
                if (string.IsNullOrEmpty(rout.ReturnTypeName))
                {
                    responcsObj = $"This is for test FOR http://localhost:9090/{rout.Route}. Son of Bitch.";
                }
                else
                {
                    responcsObj = FakerService.CreateMockInstance(rout.ReturnType);
                }

                var response = Response.Create()
                                       .WithStatusCode(200)
                                       .WithHeader("Content-Type", "json")
                                       .WithBodyAsJson(responcsObj, Encoding.UTF8, true);

                wireMockServer.Given(request)
                              .RespondWith(response);
            }
        }
    }
}