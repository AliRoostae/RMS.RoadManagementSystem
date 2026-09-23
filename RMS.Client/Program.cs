using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RMS.Client;
using RMS.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthSessionStore>();
builder.Services.AddScoped<RmsAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<RmsAuthenticationStateProvider>());
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AnalyticsApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<AccidentApiService>();
builder.Services.AddScoped<CarApiService>();
builder.Services.AddScoped<ImageApiService>();
builder.Services.AddScoped<LookupApiService>();
builder.Services.AddScoped<PassengerApiService>();
builder.Services.AddScoped<PeopleApiService>();
builder.Services.AddScoped<RoadApiService>();
builder.Services.AddScoped<OpenStreetMapSearchService>();
builder.Services.AddScoped<OpenStreetMapRoutingService>();

await builder.Build().RunAsync();