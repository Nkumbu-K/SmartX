
using System;

namespace PROG7312_SmartX.Models
{
    public class TelemetryPacket<T>
    {
        public Guid PacketId { get; set; } = Guid.NewGuid();

        public Guid SensorId { get; set; } = Guid.Empty;

        public T Number { get; set; } = default!;

        public string Unit { get; set; } = string.Empty; //(Microsoft, 2026)

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public TelemetryPacket()
        {
        }

        public TelemetryPacket(Guid sensorId, T number, string unit)
        {
            SensorId = sensorId;
            Number = number;
            Unit = unit;
        }
    }
}
/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */