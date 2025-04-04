using Mirage.Api.Common;
using Mirage.Api.Infrastructure.Services.Endpoint;
using Mirage.Api.Infrastructure.Services.ObjectGenerator;
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
                _logger.LogWarning("هیچ مسیر (route) ای یافت نشد.");
                return;
            }

            // استارت WireMock Server در پورت 9090
            var wireMockServer = WireMockServer.Start(new WireMockServerSettings
            {
                Port = 9090,
                StartAdminInterface = true,
            });

            _logger.LogInformation("WireMock Server در پورت 9090 اجرای خود را آغاز کرد.");

            var requests = Request.Create()
                                  .WithPath("/api/ownerss")
                                  .WithParam("id", new WildcardMatcher("*"))
                                  .UsingGet();

            var responses = Response.Create()
                                   .WithStatusCode(200)
                                   .WithHeader("Content-Type", "text/plain")
                                   .WithBody("This is a stub for /api/owners with query id.");

            wireMockServer.Given(requests)
                          .RespondWith(responses);

            foreach (var rout in routes)
            {
                // فرض: rout.Route مثلا "api/owners" باشد (بدون پارامتر در مسیر)
                var uri = rout.Route;

                // اگر بخواهید همچنان پشتیبانی از مسیرهایی مانند api/owners/{id} را داشته باشید:
                // رشته‌هایی داخل آکلاد را با "*" جایگزین می‌کنیم، در صورت وجود.
                string pattern = @"\{[^}]*\}";
                uri = Regex.Replace(uri, pattern, "*");

                _logger.LogInformation($"مسیر نهایی برای Mock: /{uri}");

                // ایجاد request با استفاده از WildcardMatcher برای مسیر
                var request = Request.Create()
                                     .WithPath(new WildcardMatcher($"/{uri}"))
                                     .UsingGet();

                // بررسی پارامترها:
                foreach (var parameter in rout.Parameters)
                {
                    // در صورتیکه پارامتر از نوع Query باشد:
                    if (parameter.ModelBinding.ToString().Equals("Query", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // در صورت نیاز می‌توانید مقدار دقیق را هم چک کنید؛ مثلاً:
                        // request.WithParam(parameter.Name, "1");
                        // ولی اگر مقدار داینامیک است، استفاده از WildcardMatcher مفید است:
                        request.WithParam(parameter.Name, new WildcardMatcher("*"));
                    }
                    // برای Header
                    else if (parameter.ModelBinding.ToString().Equals("Header", StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithHeader(parameter.Name);
                    }
                    // برای Body
                    else if (parameter.ModelBinding.ToString().Equals("Body", StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.WithBody(parameter.Name);
                    }
                    // برای پارامترهای مسیر نیازی به افزودن شرط جدا نیست.
                }

                // ساخت پاسخ برای درخواست mock شده
                string responcsObj = $"This is for test FOR http://localhost:9090/{rout.Route}.";
                var response = Response.Create()
                                       .WithStatusCode(200)
                                       .WithHeader("Content-Type", "text/plain")
                                       .WithBody(responcsObj);

                wireMockServer.Given(request)
                              .RespondWith(response);

                _logger.LogInformation($"Stub برای مسیر /{uri} اضافه شد.");
            }
        }
    }
}
