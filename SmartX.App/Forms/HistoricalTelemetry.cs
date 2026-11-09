using System.Net.Http;
using System.Net.Http.Json;

namespace SmartX.App.Forms;

public sealed class HistoricalTelemetry : Form
{
    private readonly HttpClient client = new() { BaseAddress = new Uri("https://localhost:7154/") };
    private readonly ComboBox sensors = new() { Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker from = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss", Width = 175, Value = DateTime.Now.AddDays(-7) };
    private readonly DateTimePicker to = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss", Width = 175 };
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
    private readonly Label status = new() { Dock = DockStyle.Bottom, Height = 30 };
    private readonly Guid? initialSensor;
    private sealed record SensorItem(Guid Id, string MacAddress, string Location)
    {
        public override string ToString() => $"{MacAddress} — {Location}";
    }

    public HistoricalTelemetry(Guid? sensorId = null)
    {
        initialSensor = sensorId;
        Text = "Historical Telemetry — AVL Range Search";
        Width = 1100; Height = 650;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 90, AutoScroll = true };
        var search = new Button { Text = "Search", AutoSize = true };
        bar.Controls.AddRange([sensors, new Label { Text = "From (local)", AutoSize = true }, from,
            new Label { Text = "To (local)", AutoSize = true }, to, search]);
        Controls.Add(grid); Controls.Add(bar); Controls.Add(status);
        Shown += async (_, _) => await LoadSensors();
        search.Click += async (_, _) => await Search();
        FormClosed += (_, _) => client.Dispose();
    }

    private async Task LoadSensors()
    {
        try
        {
            var items = await client.GetFromJsonAsync<List<SensorItem>>("api/devices") ?? [];
            sensors.DataSource = items;
            if (initialSensor.HasValue) sensors.SelectedItem = items.Find(x => x.Id == initialSensor.Value);
            status.Text = "Choose a sensor and local date/time bounds, then Search.";
        }
        catch (Exception ex) { status.Text = $"Could not load sensors: {ex.Message}"; }
    }

    private async Task Search()
    {
        if (sensors.SelectedItem is not SensorItem sensor) return;
        if (from.Value > to.Value) { status.Text = "Start must precede end."; return; }
        try
        {
            string start = Uri.EscapeDataString(from.Value.ToUniversalTime().ToString("O"));
            string end = Uri.EscapeDataString(to.Value.ToUniversalTime().ToString("O"));
            var readings = await client.GetFromJsonAsync<List<Telemetry.TelemetryDto>>(
                $"api/telemetry/history/{sensor.Id}/range?from={start}&to={end}") ?? [];
            grid.DataSource = readings.Select(r => new { r.PacketId, r.SensorId, LocalTime = r.Timestamp.ToLocalTime(), r.Number, r.Unit }).ToList();
            status.Text = $"{readings.Count} reading(s); inclusive bounds, ordered by timestamp.";
        }
        catch (Exception ex) { status.Text = $"Search failed: {ex.Message}"; }
    }
}
