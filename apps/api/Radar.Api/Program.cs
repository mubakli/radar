using Microsoft.EntityFrameworkCore;
using Radar.Api.Data;
using Radar.Api.Features.Stories;
using Radar.Api.Features.Sources;
using Radar.Api.Features.Ingestion;
using Radar.Api.Features.Brief;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RadarDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Radar")));
builder.Services.Configure<FetchOptions>(builder.Configuration.GetSection("Fetch"));
builder.Services.Configure<BriefOptions>(builder.Configuration.GetSection("Brief"));
builder.Services.AddHttpClient<IFeedFetcher, HttpFeedFetcher>().ConfigurePrimaryHttpMessageHandler((sp) => new HttpClientHandler { AllowAutoRedirect = true, MaxAutomaticRedirections = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FetchOptions>>().Value.MaxRedirects });
builder.Services.AddScoped<FeedIngestionService>();
builder.Services.AddScoped<StoryGroupingService>();
builder.Services.AddCors(options => options.AddPolicy("web", policy =>
    policy.WithOrigins(builder.Configuration["WebOrigin"] ?? "http://localhost:3000")
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors("web");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapStoryEndpoints();
app.MapSourceEndpoints();
app.MapBriefEndpoints();

if (args.Contains("seed", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<RadarDbContext>();
    await SeedData.SeedAsync(db);
    return;
}

app.Run();

public partial class Program { }
