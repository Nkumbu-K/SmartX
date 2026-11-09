
namespace PROG7312_SmartX.Models
{
    public class DeviceCommand
    {
        public Guid CommandId { get; set; } = Guid.NewGuid();

        public Guid SensorId { get; set; }

        public string CommandName { get; set; } = string.Empty; //(Microsoft, 2026)

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */