using System;

namespace SmartX.Api.Models
{
    public class TelemetryItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DeviceId { get; set; } //(Microsoft, 2026)
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */