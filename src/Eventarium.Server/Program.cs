using System.Net;
using Eventarium.Providers.GitHub;
using Eventarium.Server.Configuration;
using Eventarium.Server.Connectors;
using Eventarium.Server.Streaming;
using Eventarium.Server.Telemetry;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry;
using OpenTelemetry.Resources;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

bool hasOtlpEndpoint =
    !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]) ||
    !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"]) ||
    !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"]) ||
    !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"]);
if (hasOtlpEndpoint)
{
    Version serviceVersion = typeof(Program).Assembly.GetName().Version ?? new Version(0, 0, 0);
    _ = builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(
                serviceName: "eventarium",
                serviceVersion: serviceVersion.ToString(fieldCount: 3),
                autoGenerateServiceInstanceId: false,
                serviceInstanceId: "1")
            .AddEnvironmentVariableDetector())
        .WithTracing(tracing => tracing.AddSource("*"))
        .WithMetrics(metrics => metrics.AddMeter("*"))
        .WithLogging(
            configureBuilder: _ => { },
            configureOptions: options =>
            {
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
            })
        .UseOtlpExporter();
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    });
});

builder.Services.Configure<ForgeOptions>(builder.Configuration.GetSection("Eventarium:Forge"));
builder.Services.AddHttpClient(nameof(GitHubRepositoryEventPoller), httpClient =>
{
    httpClient.DefaultRequestVersion = HttpVersion.Version30;
    httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    AutomaticDecompression = DecompressionMethods.All
});
builder.Services.AddSingleton<IForgeSourceFactory, FakeForgeSourceFactory>();
builder.Services.AddSingleton<IForgeSourceFactory>(services =>
    new GitHubForgeSourceFactory(
        services.GetRequiredService<IHttpClientFactory>(),
        Environment.GetEnvironmentVariable("GITHUB_TOKEN")));
builder.Services.AddSingleton<IForgeSourceFactory>(_ =>
    new GitHubWebhookSourceFactory(
        Environment.GetEnvironmentVariable("GITHUB_WEBHOOK_SECRET")));
builder.Services.AddSingleton<ForgeSourceRegistry>();
builder.Services.AddSingleton<ForgeFeedBroker>();
builder.Services.AddSingleton<EventariumMetrics>();
builder.Services.AddHostedService<ForgeConnectorService>();
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    _ = app.UseHsts();
}

_ = app.UseHttpsRedirection();

app.MapHealthChecks("/healthz");
app.MapStaticAssets();
app.MapForgeFeed();
app.MapGitHubWebhooks();
app.MapFallbackToFile("index.html");

await app.RunAsync();
