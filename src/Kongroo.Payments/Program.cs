using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using Kongroo.Payments;
using Kongroo.Payments.Infrastructure;
using Kongroo.Payments.Presentation;
using Kongroo.Payments.Presentation.Authorization;
using Kongroo.Payments.Presentation.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    )
);

builder.Services.AddSerilog(configuration =>
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithProcessName()
        .Enrich.WithThreadId()
        .Enrich.WithThreadName()
        .Enrich.WithProperty("Application", AppDomain.CurrentDomain.FriendlyName)
);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder
    .Services.AddHealthChecks()
    .AddApplicationLifecycleHealthCheck()
    .AddResourceUtilizationHealthCheck()
    .AddNpgSql(_ => builder.Configuration.GetConnectionString("Database")!, tags: ["ready"])
    .AddDbContextCheck<PaymentsDbContext>(tags: ["ready"]);

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions =
            builder.Configuration.GetRequiredSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Configuration section 'Jwt' is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtOptions.CreateSigningKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole("Admin"));

builder.Services.AddPaymentsModule(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapPaymentEndpoints();

app.MapOpenApi();
app.MapScalarApiReference();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
