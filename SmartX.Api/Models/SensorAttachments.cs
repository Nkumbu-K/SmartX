namespace PROG7312_SmartX.Models
{
    public class SensorAttachments
    {
        public Guid Id { get; set; } = Guid.NewGuid(); //Filename ID
        public string attachmentname { get; set; } //Filename (Microsoft, 2026)
        public string Path { get; set; } //File path

    }
}
/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */