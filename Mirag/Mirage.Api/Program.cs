using Mirage.Api.Infrastructure.Services.Endpoint;
using Mirage.Api.Infrastructure.Services.MockServer;
using Mirage.Api.Infrastructure.Services.ObjectGenerator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EndPointsService>();
builder.Services.AddSingleton<FakerService>();
builder.Services.AddSingleton<MockServerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

_ = Task.Run(async () =>
{
    var wireMockService = app.Services.GetRequiredService<MockServerService>();
    await wireMockService.ConfigServer();
});

app.Run();
