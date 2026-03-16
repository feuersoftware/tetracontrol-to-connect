using FeuerSoftware.TetraControl2Connect.Services;

namespace FeuerSoftware.TetraControl2Connect.Endpoints;

public static class UpdateEndpoints
{
    public static WebApplication MapUpdateEndpoints(this WebApplication app)
    {
        app.MapGet("/api/update", (IUpdateService updateService) =>
        {
            var update = updateService.LatestUpdate;
            return Results.Ok(new
            {
                hasUpdate = update is not null,
                latestVersion = update?.LatestVersion,
                releaseUrl = update?.ReleaseUrl
            });
        }).WithTags("Update");

        return app;
    }
}
