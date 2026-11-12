using System.Net.Http;
using System.Net.Http.Json;

namespace SmartX.App.Forms;

public sealed class HistoricalTelemetry : Form
{
    private readonly HttpClient client = new() { BaseAddress = new Uri("https://localhost:7154/") };
    private readonly ComboBox sensors = new() { Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker from = Picker();
    private readonly DateTimePicker to = Picker();
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false
    };
    private readonly Label heading = new() { Dock = DockStyle.Top, Height = 35, Padding = new Padding(8) };
    private readonly Label status = new() { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(8) };
    private readonly FlowLayoutPanel toolbar = new() { Dock = DockStyle.Top, Height = 90, AutoScroll = true };
    private readonly Guid? initialSensor;
    private bool busy, loading;

    private sealed record SensorItem(Guid Id, string MacAddress, string Location)
    {
        public override string ToString() => $"{MacAddress} — {Location}";
    }
    private static DateTimePicker Picker() => new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss", Width = 185
    };

    public HistoricalTelemetry(Guid? sensorId = null)
    {
        initialSensor = sensorId;
        Text = "SmartX — Sensor Telemetry History (AVL Search)";
        Width = 1150; Height = 700; MinimumSize = new Size(850, 500);
        var search = new Button { Text = "Search range", AutoSize = true };
        var all = new Button { Text = "Show all / reset dates", AutoSize = true };
        toolbar.Controls.AddRange([sensors, new Label { Text = "From (local)", AutoSize = true }, from,
            new Label { Text = "To (local)", AutoSize = true }, to, search, all]);
        Controls.Add(grid); Controls.Add(heading); Controls.Add(toolbar); Controls.Add(status);
        sensors.SelectedIndexChanged += async (_, _) =>
        {
            if (!loading) await Run(LoadAll);
        };
        search.Click += async (_, _) => await Run(Search);
        all.Click += async (_, _) => await Run(LoadAll);
        Shown += async (_, _) => await Run(Initialise);
        FormClosed += (_, _) => client.Dispose();
    }

    private async Task Run(Func<Task> action)
    {
        if (busy) return;
        busy = true; toolbar.Enabled = false;
        try { await action(); }
        catch (Exception ex)
        {
            if (!IsDisposed) { grid.DataSource = null; status.Text = $"Could not load history: {ex.Message}"; }
        }
        finally { busy = false; if (!IsDisposed) toolbar.Enabled = true; }
    }

    private async Task Initialise()
    {
        var items = await client.GetFromJsonAsync<List<SensorItem>>("api/devices") ?? [];
        loading = true;
        try
        {
            sensors.DataSource = items;
            if (initialSensor.HasValue && items.Find(x => x.Id == initialSensor.Value) is SensorItem selected)
                sensors.SelectedItem = selected;
        }
        finally { loading = false; }
        if (items.Count == 0) { status.Text = "Register a sensor before viewing history."; return; }
        await LoadAll();
    }

    private async Task LoadAll()
    {
        if (sensors.SelectedItem is not SensorItem sensor) return;
        grid.DataSource = null;
        heading.Text = $"Sensor: {sensor.MacAddress} — {sensor.Location}";
        var readings = await client.GetFromJsonAsync<List<Telemetry.TelemetryDto>>(
            $"api/telemetry/history/{sensor.Id}") ?? [];
        if (readings.Count > 0)
        {
            DateTime earliest = readings.Min(r => r.Timestamp).ToLocalTime();
            DateTime latest = readings.Max(r => r.Timestamp).ToLocalTime();
            // Align visible whole-second controls so fractional readings are included.
            earliest = earliest.AddTicks(-(earliest.Ticks % TimeSpan.TicksPerSecond));
            latest = latest.AddTicks(-(latest.Ticks % TimeSpan.TicksPerSecond)).AddSeconds(1);
            from.Value = earliest; to.Value = latest;
        }
        else { from.Value = DateTime.Now.AddDays(-7); to.Value = DateTime.Now; }
        Display(sensor, readings);
        status.Text = readings.Count == 0 ? "This sensor has no recorded telemetry."
            : $"Showing all {readings.Count} readings, oldest first. Dates cover the saved history; narrow them and Search range to use the AVL index.";
    }

    private async Task Search()
    {
        if (sensors.SelectedItem is not SensorItem sensor) return;
        if (from.Value > to.Value) { status.Text = "Start must be before or equal to end."; return; }
        string start = Uri.EscapeDataString(from.Value.ToUniversalTime().ToString("O"));
        string end = Uri.EscapeDataString(to.Value.ToUniversalTime().ToString("O"));
        var readings = await client.GetFromJsonAsync<List<Telemetry.TelemetryDto>>(
            $"api/telemetry/history/{sensor.Id}/range?from={start}&to={end}") ?? [];
        Display(sensor, readings);
        status.Text = readings.Count == 0 ? "No readings for this sensor within the selected period. Use Show all to reset the dates."
            : $"{readings.Count} readings returned by the sensor's AVL timestamp index. Both bounds are inclusive; times are displayed locally.";
    }

    private void Display(SensorItem sensor, List<Telemetry.TelemetryDto> readings)
    {
        heading.Text = $"Sensor: {sensor.MacAddress} — {sensor.Location}";
        grid.DataSource = readings.OrderBy(r => r.Timestamp).Select(r => new
        {
            MacAddress = sensor.MacAddress, sensor.Location, LocalTime = r.Timestamp.ToLocalTime(),
            Reading = r.Number, r.Unit, r.PacketId
        }).ToList();
        if (grid.Columns["LocalTime"] is DataGridViewColumn time)
            time.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss.fff";
    }
}
