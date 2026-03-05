using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Configure CORS to allow all origins, methods, and headers for development purposes
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// Add rate limiting service. Partition by the request's IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromSeconds(3)
        }));

    options.OnRejected = async (context, token) =>
    {
        var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        string ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string cacheKey = $"last_valid_data_{ip}";

        // Try to retrieve the last saved value
        if (cache.TryGetValue(cacheKey, out var lastValue))
        {
            await context.HttpContext.Response.WriteAsJsonAsync(lastValue, token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync("Too many requests and no cached data available.", token);
        }
    };
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS for development
app.UseCors("DevCors");

// Enable rate limiting middleware
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();



app.Run();
