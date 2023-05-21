using Microsoft.AspNetCore.Http.Extensions;
using Orleans.Configuration;
using Orleans.Runtime;
using WebApi;

var builder = WebApplication.CreateBuilder();


if (builder.Environment.IsDevelopment())
{
    builder.Host.UseOrleans(builder =>
    {
        builder.UseLocalhostClustering();
        // configure the Orleans silos to persist grains in memory
        builder.AddMemoryGrainStorage("urls");
    });
}
else
{
    // using default storage account and connectionString in appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("azurite");

    builder.Host.UseOrleans(builder =>
    {
        // Use azure table storage for silo data for the Orleans cluster
        builder.UseAzureStorageClustering(options =>
            options.ConfigureTableServiceClient(connectionString))
        // for persisting grains
            .AddAzureTableGrainStorage("urls",
                            options => options.ConfigureTableServiceClient(connectionString));
        builder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "url-shortener";
            options.ServiceId = "urls";
        });
    });
}

var app = builder.Build();

app.MapGet("/shorten/{redirect}",
    async (IGrainFactory grains, HttpRequest request, string redirect) =>
    {
        var decodedUrl = Uri.UnescapeDataString(redirect);

        // Create a unique, short ID
        var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");

        // obtain grain reference to the target grain and persist it it with the shortened ID and full URL
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        await shortenerGrain.SetUrl(decodedUrl);

        // Return the shortened URL for later use
        var resultBuilder = new UriBuilder($"{request.Scheme}://{request.Host.Value}")
        {
            Path = $"/go/{shortenedRouteSegment}"
        };

        return Results.Ok(resultBuilder.Uri);
    });

app.MapGet("/go/{shortenedRouteSegment}",
    async (IGrainFactory grains, string shortenedRouteSegment) =>
    {
        // Retrieve the grain using the shortened ID and redirect to the original URL        
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        var url = await shortenerGrain.GetUrl();

        return Results.Redirect(url);
    });

app.Run();