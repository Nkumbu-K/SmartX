namespace PROG7312_SmartX.DataTransfer
{
    public class TelemetryAnalysisRequest
    {
        public Guid SensorId { get; set; } //(Microsoft, 2026)

        public double Min { get; set; }

        public double Max { get; set; }
    }
}
/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */