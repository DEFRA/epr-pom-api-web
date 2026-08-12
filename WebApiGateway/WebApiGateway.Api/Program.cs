using EPR.Common.Functions.Extensions;
using EPR.Common.Logging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Serilog;
using WebApiGateway.Api.ConfigurationExtensions;
using WebApiGateway.Api.HealthChecks;
using WebApiGateway.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var buildNumber = configuration.GetValue<string>("BUILD_NUMBER");
var gitSha = configuration.GetValue<string>("GIT_SHA");

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
    config.Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName);
    config.Enrich.WithProperty("BuildNumber", buildNumber ?? "NOT_SET");
    config.Enrich.WithProperty("GitSha", gitSha ?? "NOT_SET");
});

builder.Services
    .AddApplicationInsightsTelemetry()
    .AddHealthChecks();

builder.Services.Configure<AggregateHealthOptions>(configuration.GetSection(AggregateHealthOptions.SectionName));
builder.Services.AddSingleton<GatewayAggregateHealthService>();

builder.Services
    .AddCommonServices()
    .AddEprAccessControl()
    .ConfigureOptions(configuration)
    .RegisterServices()
    .RegisterHttpClients()
    .AddHttpContextAccessor()
    .ConfigureLogging();

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        options =>
        {
            builder.Configuration.Bind(Constants.AzureAdB2C, options);
        },
        options =>
        {
            builder.Configuration.Bind(Constants.AzureAdB2C, options);
        });

// Authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("FallbackPolicy", policy => policy.RequireAuthenticatedUser());

builder.Services.AddApiVersioning();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        Constants.Bearer,
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = Constants.Bearer
        });
    options.OperationFilter<AddAuthHeaderOperationFilter>();
    options.CustomSchemaIds(schema => schema.FullName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();
// Access is restricted by the deployed service boundary; this endpoint must remain anonymous for health probes.
app.MapGet(
        "/admin/health/all",
        async (bool? deep, HttpContext context, IOptions<AggregateHealthOptions> options, GatewayAggregateHealthService healthService) =>
        {
            if (!AggregateHealthHop.TryRead(context.Request, options.Value.MaximumDeepHealthHops, out var hop))
            {
                return Results.BadRequest();
            }

            var report = await healthService.CheckAsync(deep is true, hop, context.RequestAborted);
            context.Response.Headers.CacheControl = "no-store";

            return Results.Json(report, statusCode: report.Status == "Healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
        })
    .AllowAnonymous();
app.Run();
