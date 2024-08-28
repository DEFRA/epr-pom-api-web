using EPR.Common.Functions.Extensions;
using EPR.Common.Logging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using WebApiGateway.Api.ConfigurationExtensions;
using WebApiGateway.Api.HealthChecks;
using WebApiGateway.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddCommonServices()
    .AddEprAccessControl()
    .AddApplicationInsightsTelemetry()
    .RegisterServices()
    .ConfigureOptions(configuration)
    .RegisterHttpClients()
    .AddApplicationInsightsTelemetry()
    .AddLogging()
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
builder.Services.AddHealthChecks();
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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();
app.Run();