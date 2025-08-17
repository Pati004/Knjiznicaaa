using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KnjiznicaBlazor;
using KnjiznicaBlazor.Services;
using System.Net.Http;
using Microsoft.Win32;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with error handling
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient()
    {
        BaseAddress = new Uri("https://localhost:7253/") // API URL
    };

    // Add default headers
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

    return httpClient;
});

// Register your service
builder.Services.AddScoped<KnjiznicaService>();

// Add logging for debugging
builder.Services.AddLogging();

try
{
    var host = builder.Build();

    // Test API connection before running
    var httpClient = host.Services.GetRequiredService<HttpClient>();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Test API connectivity
        var response = await httpClient.GetAsync("api/avtorji");
        logger.LogInformation($"API connection test: {response.StatusCode}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to API at startup");
        // Continue running even if API is not available initially
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Application startup failed: {ex.Message}");
    throw;
}

// Register your services
builder.Services.AddScoped<KnjiznicaService>();

var app = builder.Build();
await app.RunAsync();