using System.Net;
using Eventarium.Providers.GitHub;
using Eventarium.Server.Configuration;
using Eventarium.Server.Connectors;
using Eventarium.Server.Streaming;
using Microsoft.AspNetCore.Server.Kestrel.Core;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
