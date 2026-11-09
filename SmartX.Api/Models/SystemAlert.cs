using System;

namespace SmartX.Api.Models
{
    public class SystemAlert
    {
        public string AlertId { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; }
        public int SeverityLevel { get; set; } // Lower value = higher priority in PriorityQueue (Microsoft, 2026)
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */