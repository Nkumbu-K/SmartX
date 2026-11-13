namespace SmartX.App.Forms;

public sealed class SensorRecommendationsDto
{
    public Guid SensorId { get; set; }
    public string MacAddress { get; set; } = "";
    public string Location { get; set; } = "";
    public int TotalReadings { get; set; }
    public int TotalCommands { get; set; }
    public bool IsFaulty { get; set; }
    public List<string> Recommendations { get; set; } = [];
    public List<string> PatternRecommendations { get; set; } = [];
}
