using PROG7312_SmartX.Services;

namespace PROG7312_SmartX.Endpoints;

public sealed record SearchActivityRequest(string SearchTerm, Guid? SensorId = null);

public static class ActivityEndpoints
{
    public static void MapActivityEndpoints(this WebApplication app)
    {
        app.MapPost("/api/activity/search", (SearchActivityRequest request, UserActivityService activity) =>
        {
            try { return Results.Ok(activity.RecordSearch(request.SearchTerm ?? "", request.SensorId)); }
            catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });
        app.MapGet("/api/activity/recent", (UserActivityService activity) => Results.Ok(activity.GetRecentActivity()));
        app.MapGet("/api/activity/patterns", (Guid? sensorId, DeviceLookupService devices, UserActivityService activity) =>
        {
            if (sensorId.HasValue && !devices.DeviceExists(sensorId.Value)) return Results.NotFound();
            return Results.Ok(activity.GetPatternRecommendations(sensorId));
        });
    }
}
