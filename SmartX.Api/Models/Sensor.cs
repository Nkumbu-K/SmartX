using System;
using System.ComponentModel.DataAnnotations;

namespace PROG7312_SmartX.Models
{
    public class Sensor
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string MacAddress { get; set; } //Unique Identifier
        public required string Location { get; set; } //Deployment Location (Microsoft, 2026)
        public SensorType Category { get; set; }
        public DateTime? LastCommunication { get; set; }
        public bool IsOnline { get; set; } = true;
        public bool MeshOffline { get; set; }
        public bool IsFaulty { get; set; }
        public List<SensorAttachments> attachment { get; set; } = new List<SensorAttachments>();
    }
}
/*Reference List
 
 Microsoft, 2026. Model Binding in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0
 */
