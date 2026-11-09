using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.DataTransfer
{
    public class registerSensor
    {
        public string MacAddress { get; set; } //sets unique hardware MAC address of the sensor (Microsoft, 2026).
        public string Location { get; set; }    //sets location of where sensor is installed
        public SensorType Category { get; set; }
    }
}
/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */