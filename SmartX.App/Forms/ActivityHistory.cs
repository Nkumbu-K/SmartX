using System.Net.Http;
using System.Net.Http.Json;

namespace SmartX.App.Forms;

public sealed class ActivityHistory : Form
{
    private readonly HttpClient client = new() { BaseAddress = new Uri("https://localhost:7154/") };
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells };
    private readonly TextBox patterns = new() { Dock = DockStyle.Bottom, Height = 190, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
    private readonly Label status = new() { Dock = DockStyle.Bottom, Height = 45 };
    private readonly Guid? sensorId;
    private sealed record Device(Guid Id, string MacAddress, string Location);
    private sealed record Activity(Guid Id, string EventType, string SearchTerm, Guid? SensorId,
        string? CommandName, DateTime Timestamp, List<Guid> MatchedSensorIds, Guid? RelatedSearchId, Guid? SearchSensorId);

    public ActivityHistory(Guid? selectedSensor = null)
    {
        sensorId = selectedSensor;
        Text = "SmartX — Recorded Search Activity and Patterns";
        Width = 1150; Height = 750;
        var refresh = new Button { Text = "Refresh activity / patterns", Dock = DockStyle.Top, Height = 35 };
        refresh.Click += async (_, _) => await LoadActivity();
        Controls.Add(grid); Controls.Add(patterns); Controls.Add(status); Controls.Add(refresh);
        Shown += async (_, _) => await LoadActivity();
        FormClosed += (_, _) => client.Dispose();
    }
    private async Task LoadActivity()
    {
        try
        {
            var devices = await client.GetFromJsonAsync<List<Device>>("api/devices") ?? [];
            string Name(Guid? id) => id is Guid value ? devices.Find(d => d.Id == value)?.MacAddress ?? value.ToString() : "Not identified";
            var events = await client.GetFromJsonAsync<List<Activity>>("api/activity/recent") ?? [];
            grid.DataSource = events.Select(e => new
            {
                LocalTime = e.Timestamp.ToLocalTime(), Action = e.EventType, Search = e.SearchTerm,
                Sensor = Name(e.SensorId), Command = e.CommandName ?? "",
                Matches = string.Join(", ", e.MatchedSensorIds.Select(id => Name(id))),
                PairedSearch = e.RelatedSearchId.HasValue ? Name(e.SearchSensorId) : "None"
            }).ToList();
            var suggestions = await client.GetFromJsonAsync<List<string>>("api/activity/patterns" + (sensorId.HasValue ? $"?sensorId={sensorId}" : "")) ?? [];
            patterns.Text = (sensorId.HasValue ? $"PATTERNS FOR COMMAND TARGET {Name(sensorId)}" : "ALL SEARCH-COMMAND PATTERNS") +
                "\r\n\r\n" + (suggestions.Count == 0 ? "No repeated independent search-command sequence yet. Search an exact MAC, send a command, then repeat the search and command."
                    : string.Join("\r\n\r\n", suggestions));
            status.Text = $"{events.Count} most recent session events. Searches are logged after a pause or Enter. Ambiguous searches are logged but not assigned to an arbitrary device. Activity resets when the API restarts.";
        }
        catch (Exception ex) { status.Text = "Could not load activity: " + ex.Message; }
    }
}
