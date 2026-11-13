using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartX.App.Forms;

/// <summary>Visible queue operations, backed by the existing FIFO and priority queue API.</summary>
public sealed class QueueManagement : Form
{
    private readonly HttpClient client = new() { BaseAddress = new Uri("https://localhost:7154/") };
    private readonly ComboBox sensors = new() { Width = 390, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown reading = new() { Minimum = -1000000000, Maximum = 1000000000, DecimalPlaces = 3, Value = 25, Width = 120 };
    private readonly TextBox unit = new() { Width = 90, Text = "°C", MaxLength = 30 };
    private readonly TextBox alert = new() { Width = 390, MaxLength = 500 };
    private readonly ComboBox priority = new() { Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label telemetryCount = new() { AutoSize = true, Text = "Pending readings: loading" };
    private readonly Label alertCount = new() { AutoSize = true, Text = "Pending alerts: loading" };
    private readonly TextBox telemetryLog = Log();
    private readonly TextBox alertLog = Log();
    private readonly Label status = new() { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(8) };
    private readonly FlowLayoutPanel telemetryBar = Bar();
    private readonly FlowLayoutPanel alertBar = Bar();
    private readonly Guid? initialSensor;
    private bool busy;
    private sealed record Device(Guid Id, string MacAddress, string Location)
    {
        public override string ToString() => $"{MacAddress} — {Location}";
    }
    private static TextBox Log() => new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
    private static FlowLayoutPanel Bar() => new() { Dock = DockStyle.Top, Height = 150, Padding = new Padding(8), AutoScroll = true };

    public QueueManagement(Guid? sensorId = null)
    {
        initialSensor = sensorId;
        Text = "SmartX — Telemetry FIFO and Alert Priority Queue";
        Width = 1080; Height = 800; MinimumSize = new Size(850, 650);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var telemetryTab = new TabPage("Telemetry queue (FIFO)");
        var alertsTab = new TabPage("Alert priority queue");
        telemetryBar.Controls.AddRange([new Label { Text = "Sensor", AutoSize = true }, sensors,
            new Label { Text = "Reading", AutoSize = true }, reading, new Label { Text = "Unit", AutoSize = true }, unit]);
        Button(telemetryBar, "Add reading to queue", async () =>
        {
            if (sensors.SelectedItem is not Device device) throw new InvalidOperationException("Choose a sensor.");
            if (string.IsNullOrWhiteSpace(unit.Text)) throw new InvalidOperationException("Enter a unit.");
            var result = await Request("api/telemetry/queue", new { sensorId = device.Id, number = (double)reading.Value, unit = unit.Text.Trim() });
            Append(telemetryLog, $"QUEUED {device.MacAddress}: {reading.Value} {unit.Text.Trim()} | packet {result.GetProperty("packetId").GetString()}");
            status.Text = "Reading is waiting in the FIFO queue. It enters saved history only after Process next.";
        });
        Button(telemetryBar, "Process next reading (FIFO)", async () =>
        {
            var result = await Request("api/telemetry/queue/process");
            if (!result.TryGetProperty("packet", out var packet)) { status.Text = "Telemetry queue is empty."; return; }
            Guid id = packet.GetProperty("sensorId").GetGuid();
            string name = sensors.Items.Cast<Device>().FirstOrDefault(d => d.Id == id)?.MacAddress ?? id.ToString();
            Append(telemetryLog, $"PROCESSED {name}: {packet.GetProperty("number")} {packet.GetProperty("unit").GetString()} | packet {packet.GetProperty("packetId").GetString()}");
            status.Text = "Oldest pending reading processed, saved and indexed in AVL history. Sensor History can now retrieve it.";
        });
        Button(telemetryBar, "Refresh pending counts", Counts);
        telemetryBar.Controls.Add(telemetryCount);
        priority.Items.AddRange(["1 — High", "2 — Medium", "3 — Low"]); priority.SelectedIndex = 1;
        alertBar.Controls.AddRange([new Label { Text = "Alert message", AutoSize = true }, alert,
            new Label { Text = "Priority", AutoSize = true }, priority]);
        Button(alertBar, "Add alert to priority queue", async () =>
        {
            if (string.IsNullOrWhiteSpace(alert.Text)) throw new InvalidOperationException("Enter an alert message.");
            int level = priority.SelectedIndex + 1;
            await Request("api/alerts/queue", new { message = alert.Text.Trim(), priority = level });
            Append(alertLog, $"QUEUED priority {level}: {alert.Text.Trim()}");
            status.Text = "Alert queued. Smaller priority numbers are processed first; equal-priority order is unspecified.";
        });
        Button(alertBar, "Process highest-priority alert", async () =>
        {
            var result = await Request("api/alerts/queue/process");
            if (!result.TryGetProperty("processedAlert", out var processed)) { status.Text = "Alert queue is empty."; return; }
            Append(alertLog, $"PROCESSED: {processed.GetString()}");
            status.Text = "Highest-priority pending alert processed. Processing records handling; it does not perform a hardware action.";
        });
        Button(alertBar, "Refresh pending counts", Counts);
        alertBar.Controls.Add(alertCount);
        telemetryTab.Controls.Add(telemetryLog); telemetryTab.Controls.Add(telemetryBar);
        alertsTab.Controls.Add(alertLog); alertsTab.Controls.Add(alertBar);
        tabs.TabPages.AddRange([telemetryTab, alertsTab]);
        Controls.Add(tabs); Controls.Add(status);
        Append(telemetryLog, "Activity in this window. Pending counts include all API clients. Queue two different readings, then process twice to observe FIFO order.");
        Append(alertLog, "Activity in this window. Add Low first, then High. Process: High must be returned first. Queues are session-local and reset when the API restarts.");
        Shown += async (_, _) => await Run(async () =>
        {
            var devices = await client.GetFromJsonAsync<List<Device>>("api/devices") ?? [];
            sensors.DataSource = devices;
            if (devices.Find(d => d.Id == initialSensor) is Device selected) sensors.SelectedItem = selected;
            status.Text = "Choose a queue tab. Add items, inspect pending counts and process items.";
        });
        FormClosed += (_, _) => client.Dispose();
    }
    private static void Append(TextBox log, string text) => log.AppendText($"{DateTime.Now:HH:mm:ss} | {text}{Environment.NewLine}{Environment.NewLine}");
    private void Button(FlowLayoutPanel bar, string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += async (_, _) => await Run(action);
        bar.Controls.Add(button);
    }
    private async Task Run(Func<Task> action)
    {
        if (busy) return;
        busy = true; telemetryBar.Enabled = false; alertBar.Enabled = false;
        try { await action(); await Counts(); }
        catch (Exception ex) { if (!IsDisposed) status.Text = $"Operation failed: {ex.Message}"; }
        finally { busy = false; if (!IsDisposed) { telemetryBar.Enabled = true; alertBar.Enabled = true; } }
    }
    private async Task Counts()
    {
        using var telemetry = JsonDocument.Parse(await client.GetStringAsync("api/telemetry/queue/status"));
        using var alerts = JsonDocument.Parse(await client.GetStringAsync("api/alerts/queue/status"));
        telemetryCount.Text = $"Pending readings (all devices): {telemetry.RootElement.GetProperty("pendingPackets")}";
        alertCount.Text = $"Pending alerts: {alerts.RootElement.GetProperty("pendingAlerts")}";
    }
    private async Task<JsonElement> Request(string endpoint, object? body = null)
    {
        using var response = body is null ? await client.PostAsync(endpoint, null) : await client.PostAsJsonAsync(endpoint, body);
        string text = await response.Content.ReadAsStringAsync();
        // A 404 from these two fixed processing endpoints means an empty queue.
        if (response.StatusCode == HttpStatusCode.NotFound && endpoint.EndsWith("/process"))
            return JsonSerializer.SerializeToElement(new { });
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {text}");
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }
}
