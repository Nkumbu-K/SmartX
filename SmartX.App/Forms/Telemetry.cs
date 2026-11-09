using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartX.App.Forms
{
    public partial class Telemetry : Form
    {
        public class TelemetryDto
        {
            public Guid PacketId { get; set; }
            public Guid SensorId { get; set; }
            public double Number { get; set; }
            public string Unit { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }

        private readonly HttpClient _httpClient;
        private readonly Guid? _sensorId;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Open telemetry for all sensors.
        public Telemetry() : this(null)
        {
        }

        // Open telemetry only for a specific sensor.
        public Telemetry(Guid? sensorId)
        {
            InitializeComponent();

            _sensorId = sensorId;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7154/")
            };

            Text = _sensorId.HasValue
                ? "Sensor Telemetry"
                : "All Sensor Telemetry";

            Shown += Telemetry_Shown;
            FormClosing += Telemetry_FormClosing;
        }

        private async void Telemetry_Shown(object? sender, EventArgs e)
        {
            await LoadTelemetryAsync();
        }

        private async Task LoadTelemetryAsync()
        {
            try
            {
                string endpoint = _sensorId.HasValue
                    ? $"api/telemetry/history/{_sensorId.Value}"
                    : "api/telemetry/history";

                using HttpResponseMessage response =
                    await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        $"Could not load telemetry.\n\n" +
                        $"HTTP status: {(int)response.StatusCode}\n{error}",
                        "Telemetry Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string json = await response.Content.ReadAsStringAsync();

                List<TelemetryDto> readings =
                    JsonSerializer.Deserialize<List<TelemetryDto>>(
                        json,
                        _jsonOptions) ?? new List<TelemetryDto>();

                DisplayTelemetry(readings);
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "Could not connect to the SmartX API. " +
                    "Make sure the API is running at https://localhost:7154/.",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (JsonException ex)
            {
                MessageBox.Show(
                    $"The API returned telemetry in an unexpected format.\n\n{ex.Message}",
                    "Data Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred while loading telemetry.\n\n{ex.Message}",
                    "Telemetry Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DisplayTelemetry(List<TelemetryDto> readings)
        {
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // Recreate the headings 
            Label heading = new Label
            {
                AutoSize = true,
                Left = 10,
                Top = 5,
                Text = _sensorId.HasValue
                    ? "TELEMETRY FOR SELECTED SENSOR"
                    : "SENSOR TELEMETRY",
                Font = new Font(Font, FontStyle.Bold)
            };

            panel1.Controls.Add(heading);

            Label eventsHeading = new Label
            {
                AutoSize = true,
                Left = 10,
                Top = 5,
                Text = "RECENT EVENTS",
                Font = new Font(Font, FontStyle.Bold)
            };

            panel2.Controls.Add(eventsHeading);

            int top = 35;

            foreach (TelemetryDto reading in readings
                .OrderByDescending(r => r.Timestamp)
                .Take(10))
            {
                bool isAnomaly = reading.Number > 80 || reading.Number < 10;

                Label label = new Label
                {
                    AutoSize = false,
                    Width = Math.Max(100, panel1.ClientSize.Width - 20),
                    Height = 25,
                    Left = 10,
                    Top = top,
                    Text =
                        $"{reading.SensorId.ToString()[..8]} | " +
                        $"{reading.Number} {reading.Unit} | " +
                        $"{(isAnomaly ? "ANOMALY" : "NORMAL")}"
                };

                panel1.Controls.Add(label);
                top += 27;
            }

            if (readings.Count == 0)
            {
                panel1.Controls.Add(new Label
                {
                    AutoSize = true,
                    Left = 10,
                    Top = 35,
                    Text = "No telemetry readings are available."
                });
            }

            top = 35;

            foreach (TelemetryDto reading in readings
                .OrderByDescending(r => r.Timestamp)
                .Take(20))
            {
                Label label = new Label
                {
                    AutoSize = false,
                    Width = Math.Max(100, panel2.ClientSize.Width - 20),
                    Height = 25,
                    Left = 10,
                    Top = top,
                    Text =
                        $"{reading.Timestamp.ToLocalTime():G} - " +
                        $"{reading.Number} {reading.Unit} - " +
                        $"{reading.SensorId.ToString()[..8]}"
                };

                panel2.Controls.Add(label);
                top += 25;
            }

            if (readings.Count == 0)
            {
                panel2.Controls.Add(new Label
                {
                    AutoSize = true,
                    Left = 10,
                    Top = 35,
                    Text = "No recent events to display."
                });
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using Register register = new Register();
            register.ShowDialog();
        }

        private void btnViewSensor_Click(object sender, EventArgs e)
        {
            using Sensors sensors = new Sensors();
            sensors.ShowDialog();
        }

        private void Telemetry_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _httpClient.Dispose();
        }
    }
}