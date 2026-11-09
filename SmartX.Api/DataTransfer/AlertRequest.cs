
namespace PROG7312_SmartX.DataTransfer
{
    public class AlertRequest
    {
        public string Message { get; set; } = string.Empty; //(Microsoft, 2026)

        public int Priority { get; set; }
    }
}

/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */