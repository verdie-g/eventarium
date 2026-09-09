using Eventarium.Client;
using Eventarium.Client.Presentation;
using Eventarium.Client.Streaming;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddSingleton<DisplayMotionProfile>();
builder.Services.AddSingleton<ForgeFeedStore>();
builder.Services.AddSingleton<IForgeFeed>(services => services.GetRequiredService<ForgeFeedStore>());
builder.Services.AddHostedService<ForgeFeedHostedService>();
builder.Services.AddSingleton<HomePageViewModel>();

await builder.Build().RunAsync();
