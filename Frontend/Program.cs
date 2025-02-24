using Frontend.Services;
using Microsoft.Extensions.FileProviders;
using Polly;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddRazorPages();

// Configure HttpClient with SSL handling for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient("ApiGateway", client =>
    {
        var baseUrl = builder.Configuration["ApiGateway:BaseUrl"] ?? "https://localhost:7108";
        client.BaseAddress = new Uri(baseUrl);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
}
else
{
    builder.Services.AddHttpClient("ApiGateway", client =>
    {
        var baseUrl = builder.Configuration["ApiGateway:BaseUrl"] ?? "https://localhost:7108";
        client.BaseAddress = new Uri(baseUrl);
    })
    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
}

// Register ProductService
builder.Services.AddScoped<ProductService>();

// Add health checks with SSL handling for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHealthChecks()
        .AddUrlGroup(new Uri(builder.Configuration["ApiGateway:BaseUrl"] + "/health"), 
            "api_gateway_health",
            tags: new[] { "api_gateway" },
            timeout: TimeSpan.FromSeconds(5));
}
else
{
    builder.Services.AddHealthChecks()
        .AddUrlGroup(new Uri(builder.Configuration["ApiGateway:BaseUrl"] + "/health"), 
            "api_gateway_health",
            tags: new[] { "api_gateway" });
}

var app = builder.Build();

// Configure error handling with Application Insights
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/html";
        var telemetry = app.Services.GetRequiredService<Microsoft.ApplicationInsights.TelemetryClient>();
        
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            telemetry.TrackException(error.Error);
            await context.Response.WriteAsync("<h1>Error</h1><p>Sorry, something went wrong. Please try again later.</p>");
        }
    });
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve files from node_modules
app.UseStaticFiles(); // Serve files from wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "node_modules")),
    RequestPath = "/node_modules"
});

app.UseRouting();
app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");
app.MapRazorPages();

app.Run();
