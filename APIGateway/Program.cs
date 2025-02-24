using APIGateway.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Configure HttpClient and BackendService with SSL handling for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<BackendService>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = 
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
}
else
{
    builder.Services.AddHttpClient<BackendService>()
        .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
}

// Register BackendService as implementation of IBackendService
builder.Services.AddScoped<IBackendService, BackendService>();

// Enable CORS with specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins(
                    "https://localhost:7155", // Frontend HTTPS
                    "http://localhost:5155"   // Frontend HTTP
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("X-Pagination");
        });
});

// Add health checks with SSL handling for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHealthChecks()
        .AddUrlGroup(
            new Uri(builder.Configuration["BackendApi:BaseUrl"] + "/health"), 
            name: "backend-api",
            tags: new[] { "backend" },
            timeout: TimeSpan.FromSeconds(5));
}
else
{
    builder.Services.AddHealthChecks()
        .AddUrlGroup(
            new Uri(builder.Configuration["BackendApi:BaseUrl"] + "/health"), 
            name: "backend-api",
            tags: new[] { "backend" });
}

// Add Health Check UI and storage
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(30); // Evaluate every 30 seconds
    setup.MaximumHistoryEntriesPerEndpoint(60); // Store up to 60 results
    setup.AddHealthCheckEndpoint("backend-api", "/health");
    setup.AddHealthCheckEndpoint("frontend", "https://localhost:7155/health");
})
.AddInMemoryStorage();

var app = builder.Build();

// Configure error handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var telemetry = app.Services.GetRequiredService<Microsoft.ApplicationInsights.TelemetryClient>();
        
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            telemetry.TrackException(error.Error);
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

// Map health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Map Health Check UI endpoints
app.MapHealthChecksUI(config =>
{
    config.UIPath = "/health-ui"; // UI available at /health-ui
});

app.MapControllers();

app.Run();
