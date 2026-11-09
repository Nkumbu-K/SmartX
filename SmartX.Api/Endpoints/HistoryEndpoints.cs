using PROG7312_SmartX.Services;

namespace PROG7312_SmartX.Endpoints;

public static class HistoryEndpoints
{
    public static void MapHistoryQueries(this WebApplication app)
    {
        app.MapGet("/api/telemetry/history/{sensorId:guid}/range",
            (Guid sensorId, DateTimeOffset from, DateTimeOffset to,
             DeviceLookupService devices, TelemetryHistoryService history) =>
            {
                if (devices.GetDevice(sensorId) is null)
                    return Results.NotFound(new { message = "Sensor not found." });
                if (from > to)
                    return Results.BadRequest(new { message = "Start must precede end." });
                return Results.Ok(history.Query(sensorId, from.UtcDateTime, to.UtcDateTime));
            });
    }
}
