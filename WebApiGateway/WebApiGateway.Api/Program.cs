using EPR.Common.Functions.Extensions;
using EPR.Common.Logging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Serilog;
using WebApiGateway.Api.ConfigurationExtensions;
using WebApiGateway.Api.HealthChecks;
using WebApiGateway.Api.Middleware;
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
app.Run();