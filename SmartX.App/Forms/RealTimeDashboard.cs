using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartX.App.Forms
{
    public partial class RealTimeDashboard : Form
    {
        private readonly HttpClient httpClient = new HttpClient //Microsoft (2026b)
        {
            BaseAddress = new Uri("https://localhost:7154/")
        };

        private readonly System.Windows.Forms.Timer telemetryTimer = new() //Microsoft (2026c)
        {
            Interval = 3000
        };

        private bool isRefreshingTelemetry;
        private bool isRefreshingDevices;
        private bool rebindingDevices;
        private int recommendationVersion;
        private ActivitySearchTracker searchTracker = null!;

        private List<SensorDto> registeredSensors = new();
        private List<TelemetryDto> telemetryReadings = new();
        private HashSet<Guid> faultySensorIds = new();
        private TextBox txtSearch = null!;
        private ComboBox cmbCategoryFilter = null!;
        private ComboBox cmbFaultFilter = null!;
        private ListBox lstSuggestedActions = null!;
        private Label lblRecommendationStatus = null!;

        public RealTimeDashboard()
        {
            InitializeComponent();

            AddBackToMenuButton();

            ConfigureExistingControls();
            CreateFilteringControls();
            CreateRecommendationControls();
            searchTracker = new ActivitySearchTracker(httpClient, txtSearch);
            AddFeedbackControls();
            cmbDevices.SelectedIndexChanged += async (_, _) =>
            {
                if (!isRefreshingDevices && !rebindingDevices) await LoadRecommendationsAsync();
            };

            telemetryTimer.Tick += TelemetryTimer_Tick;
            Shown += RealTimeDashboard_Shown;
            FormClosing += RealTimeDashboard_FormClosing;
        }

        private void AddBackToMenuButton()
        {
            Button btnBackToMenu = new Button
            {
                Name = "btnBackToMenu",
                Text = "Back",
                Size = new Size(160, 35),
                Location = new Point(10, ClientSize.Height - 45),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            btnBackToMenu.Click += (_, _) => Close();

            Controls.Add(btnBackToMenu);
            btnBackToMenu.BringToFront();
        }

        private void ConfigureExistingControls()
        {
            cmbCommands.Items.Clear();
            cmbCommands.Items.AddRange(new object[]
            {
                "Enable",
                "Disable",
                "Restart",
                "Triggering a water pump",
                "Toggling a smart relay"
            });

            cmbCommands.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCommands.SelectedIndex = 0;

            cmbDevices.DropDownStyle = ComboBoxStyle.DropDownList;

            dgvTelemetry.AllowUserToAddRows = false;
            dgvTelemetry.ReadOnly = true;
            dgvTelemetry.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTelemetry.MultiSelect = false;
            dgvTelemetry.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void CreateFilteringControls()
        {
            var grpFilters = new GroupBox
            {
                Name = "grpFilters",
                Text = "Search and Filter Devices",
                Location = new Point(61, 490),
                Size = new Size(886, 90)
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(15, 28),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Name = "txtSearch",
                Location = new Point(75, 25),
                Size = new Size(200, 27)
            };

            var lblCategory = new Label
            {
                Text = "Category:",
                Location = new Point(290, 28),
                AutoSize = true
            };

            cmbCategoryFilter = new ComboBox
            {
                Name = "cmbCategoryFilter",
                Location = new Point(365, 25),
                Size = new Size(145, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbCategoryFilter.Items.Add("All categories");
            cmbCategoryFilter.SelectedIndex = 0;

            var lblFault = new Label
            {
                Text = "Fault status:",
                Location = new Point(525, 28),
                AutoSize = true
            };

            cmbFaultFilter = new ComboBox
            {
                Name = "cmbFaultFilter",
                Location = new Point(615, 25),
                Size = new Size(160, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbFaultFilter.Items.AddRange(new object[]
            {
                "All devices",
                "Faulty only",
                "Healthy only"
            });
            cmbFaultFilter.SelectedIndex = 0;

            grpFilters.Controls.AddRange(new Control[]
            {
                lblSearch,
                txtSearch,
                lblCategory,
                cmbCategoryFilter,
                lblFault,
                cmbFaultFilter
            });

            Controls.Add(grpFilters);

            txtSearch.TextChanged += (_, _) =>
            {
                ApplyDeviceFilters();
                DisplayFilteredTelemetry();
            };

            cmbCategoryFilter.SelectedIndexChanged +=
                (_, _) =>
                {
                    ApplyDeviceFilters();
                    DisplayFilteredTelemetry();
                };

            cmbFaultFilter.SelectedIndexChanged +=
                (_, _) =>
                {
                    ApplyDeviceFilters();
                    DisplayFilteredTelemetry();
                };

            ClientSize = new Size(
                Math.Max(ClientSize.Width, 1020),
                730
            );
        }

        private void CreateRecommendationControls()
        {
            var grpRecommendations = new GroupBox
            {
                Name = "grpRecommendations",
                Text = "Suggested Actions / Automated Insights",
                Location = new Point(61, 590),
                Size = new Size(886, 190)
            };

            lstSuggestedActions = new ListBox
            {
                Name = "lstSuggestedActions",
                Location = new Point(12, 25),
                Size = new Size(650, 145),
                HorizontalScrollbar = true
            };

            lblRecommendationStatus = new Label
            {
                Name = "lblRecommendationStatus",
                Text = "Recommendations have not been loaded yet.",
                Location = new Point(675, 28),
                Size = new Size(195, 130),
                AutoSize = false
            };

            grpRecommendations.Controls.Add(lstSuggestedActions);
            grpRecommendations.Controls.Add(lblRecommendationStatus);

            Controls.Add(grpRecommendations);
        }

        private async void RealTimeDashboard_Shown(object? sender, EventArgs e)
        {
            await LoadDevicesAsync();
            await RefreshFaultyDevicesAsync();
            await LoadCommandHistoryAsync();
            await RefreshTelemetryAsync();
            await LoadRecommendationsAsync();
        }

        private async Task LoadDevicesAsync()
        {
            if (isRefreshingDevices)
                return;

            isRefreshingDevices = true;

            try
            {
                registeredSensors = await httpClient.GetFromJsonAsync<List<SensorDto>>("api/devices") ?? new List<SensorDto>(); //Microsoft (2026d)

                PopulateCategoryFilter();
                ApplyDeviceFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load devices: " + ex.Message, "API Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isRefreshingDevices = false;
            }
        }

        private void PopulateCategoryFilter()
        {
            string? previousSelection = cmbCategoryFilter.SelectedItem?.ToString();

            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add("All categories");

            foreach (int category in registeredSensors
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c))
            {
                cmbCategoryFilter.Items.Add($"Category {category}");
            }

            if (previousSelection != null && cmbCategoryFilter.Items.Contains(previousSelection))
            {
                cmbCategoryFilter.SelectedItem = previousSelection;
            }
            else
            {
                cmbCategoryFilter.SelectedIndex = 0;
            }
        }

        private void ApplyDeviceFilters()
        {
            string search = txtSearch.Text.Trim();

            string categorySelection = cmbCategoryFilter.SelectedItem?.ToString() ?? "All categories";

            string faultSelection = cmbFaultFilter.SelectedItem?.ToString() ?? "All devices";

            var filteredSensors = registeredSensors //Jamro (2018, pp. 64-67)
                .Where(sensor => string.IsNullOrWhiteSpace(search) ||
                    sensor.MacAddress.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    sensor.Location.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    sensor.Id.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                .Where(sensor =>
                    categorySelection == "All categories" || categorySelection == $"Category {sensor.Category}")
                .Where(sensor => faultSelection == "All devices" ||
                    (faultSelection == "Faulty only" && faultySensorIds.Contains(sensor.Id)) ||
                    (faultSelection == "Healthy only" && !faultySensorIds.Contains(sensor.Id)))
                .ToList();

            // Avoid resetting the selection when it is still valid.
            Guid? previousId = (cmbDevices.SelectedItem as SensorDto)?.Id;

            rebindingDevices = true;
            try
            {
                cmbDevices.DataSource = null;
                cmbDevices.DisplayMember = nameof(SensorDto.MacAddress);
                cmbDevices.ValueMember = nameof(SensorDto.Id);
                cmbDevices.DataSource = filteredSensors;
                if (previousId.HasValue)
                {
                    int index = filteredSensors.FindIndex(s => s.Id == previousId.Value);
                    if (index >= 0) cmbDevices.SelectedIndex = index;
                }
            }
            finally { rebindingDevices = false; }
            if (!isRefreshingDevices) _ = LoadRecommendationsAsync();
        }

        private async Task RefreshFaultyDevicesAsync()
        {
            try
            {
                var result = await httpClient.GetFromJsonAsync<FaultyDevicesDto>("api/devices/faulty");

                faultySensorIds = result?.SensorIds?.ToHashSet() ?? new HashSet<Guid>();

                ApplyDeviceFilters();
            }
            catch (Exception ex)
            {
                lblRecommendationStatus.Text = "Could not refresh fault status.";

                System.Diagnostics.Debug.WriteLine("Fault status refresh failed: " + ex.Message);
            }
        }

        private async Task RefreshTelemetryAsync()
        {
            if (isRefreshingTelemetry)
                return;

            isRefreshingTelemetry = true;

            try
            {
                telemetryReadings =
                    await httpClient.GetFromJsonAsync<List<TelemetryDto>>("api/telemetry/history")
                    ?? new List<TelemetryDto>();

                await RefreshFaultyDevicesAsync();
                DisplayFilteredTelemetry();
            }
            catch (Exception ex)
            {
                telemetryTimer.Stop();

                MessageBox.Show("Unable to refresh telemetry: " + ex.Message, "Telemetry Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isRefreshingTelemetry = false;
            }
        }

        private void DisplayFilteredTelemetry()
        {
            dgvTelemetry.Rows.Clear();

            string search = txtSearch.Text.Trim();
            string categorySelection = cmbCategoryFilter.SelectedItem?.ToString() ?? "All categories";
            string faultSelection = cmbFaultFilter.SelectedItem?.ToString() ?? "All devices";

            foreach (var reading in telemetryReadings.OrderByDescending(r => r.Timestamp))
            {
                var sensor = registeredSensors.FirstOrDefault(s => s.Id == reading.SensorId);

                string macAddress = sensor?.MacAddress ?? reading.SensorId.ToString();

                string location = sensor?.Location ?? string.Empty;
                int? category = sensor?.Category;

                bool isFaulty = faultySensorIds.Contains(reading.SensorId);

                bool matchesSearch =
                    string.IsNullOrWhiteSpace(search) ||
                    macAddress.Contains(
                        search, StringComparison.OrdinalIgnoreCase) ||
                    location.Contains(
                        search, StringComparison.OrdinalIgnoreCase) ||
                    reading.SensorId.ToString().Contains(
                        search, StringComparison.OrdinalIgnoreCase);

                bool matchesCategory = categorySelection == "All categories" || category.HasValue && categorySelection == $"Category {category.Value}";

                bool matchesFault = faultSelection == "All devices" || (faultSelection == "Faulty only" && isFaulty) || (faultSelection == "Healthy only" && !isFaulty);

                if (!matchesSearch || !matchesCategory || !matchesFault)
                    continue;

                dgvTelemetry.Rows.Add(
                    reading.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    macAddress,
                    category.HasValue
                        ? $"Category {category.Value}"
                        : "Unknown",
                    $"{reading.Number:0.##} {reading.Unit}",
                    isFaulty ? "FAULTY" : "Normal",
                    isFaulty ? "High (Faulty)" : "Normal"
                );
            }
        }

        private async Task LoadCommandHistoryAsync()
        {
            try
            {
                var commands = await httpClient.GetFromJsonAsync<List<DeviceCommandDto>>("api/commands/history")
                    ?? new List<DeviceCommandDto>();

                lstHistory.Items.Clear();

                foreach (var command in commands)
                {
                    var sensor = registeredSensors.FirstOrDefault(s => s.Id == command.SensorId);

                    string deviceName = sensor?.MacAddress ?? command.SensorId.ToString();

                    lstHistory.Items.Add($"{command.Timestamp.ToLocalTime():HH:mm:ss} | " + $"{deviceName} | {command.CommandName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load command history: " + ex.Message, "Command History Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadRecommendationsAsync()
        {
            int version = ++recommendationVersion;
            if (cmbDevices.SelectedItem is not SensorDto selected)
            {
                lstSuggestedActions.Items.Clear();
                lstSuggestedActions.Items.Add("Select a sensor to view its recommendations.");
                lblRecommendationStatus.Text = "No selected sensor.";
                return;
            }
            lstSuggestedActions.Items.Clear();
            lstSuggestedActions.Items.Add($"Loading recommendations for {selected.MacAddress}...");
            lblRecommendationStatus.Text = $"Selected: {selected.MacAddress}";
            try
            {
                var result = await httpClient.GetFromJsonAsync<SensorRecommendationsDto>($"api/recommendations/{selected.Id}");
                if (IsDisposed || version != recommendationVersion) return;
                if (result is null || result.SensorId != selected.Id) throw new InvalidDataException("Recommendation sensor mismatch.");
                lstSuggestedActions.Items.Clear();
                lstSuggestedActions.Items.Add($"SENSOR: {result.MacAddress} — {result.Location}");
                foreach (string text in result.Recommendations) lstSuggestedActions.Items.Add(text);
                lstSuggestedActions.Items.Add("SEARCH-COMMAND PATTERNS:");
                foreach (string text in result.PatternRecommendations) lstSuggestedActions.Items.Add(text);
                if (result.PatternRecommendations.Count == 0) lstSuggestedActions.Items.Add("No repeated independent sequence for this sensor yet.");
                lblRecommendationStatus.Text = $"{result.MacAddress}\r\n{result.Location}\r\nReadings: {result.TotalReadings}\r\nCommands: {result.TotalCommands}\r\nUpdated {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                if (IsDisposed || version != recommendationVersion) return;
                lstSuggestedActions.Items.Clear();
                lstSuggestedActions.Items.Add("Unable to load this sensor's recommendations: " + ex.Message);
                lblRecommendationStatus.Text = "Load failed.";
            }
        }

        private void AddFeedbackControls()
        {
            ClientSize = new Size(Math.Max(ClientSize.Width, 1040), 900);
            AutoScroll = true;
            AutoScrollMinSize = new Size(1040, 900);
            if (Controls["btnBackToMenu"] is Button back)
            {
                back.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                back.Location = new Point(10, 840);
            }
            var actions = new FlowLayoutPanel
            {
                Location = new Point(185, 840), Size = new Size(830, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            var queues = new Button { Text = "Telemetry / Alert queues", AutoSize = true };
            var activity = new Button { Text = "Search activity / patterns", AutoSize = true };
            var refresh = new Button { Text = "Refresh insights", AutoSize = true };
            var profile = new Button { Text = "Selected sensor profile", AutoSize = true };
            queues.Click += async (_, _) =>
            {
                using var manager = new QueueManagement((cmbDevices.SelectedItem as SensorDto)?.Id);
                manager.ShowDialog(this);
                await RefreshTelemetryAsync(); await LoadRecommendationsAsync();
            };
            activity.Click += async (_, _) =>
            {
                try
                {
                    await searchTracker.FlushAsync();
                    using var viewer = new ActivityHistory((cmbDevices.SelectedItem as SensorDto)?.Id);
                    viewer.ShowDialog(this);
                }
                catch (Exception ex) { MessageBox.Show("Could not load activity: " + ex.Message); }
            };
            refresh.Click += async (_, _) => await LoadRecommendationsAsync();
            profile.Click += async (_, _) =>
            {
                if (cmbDevices.SelectedItem is not SensorDto sensor) { MessageBox.Show("Select a sensor first."); return; }
                using var details = new SensorProfile(sensor.Id);
                details.ShowDialog(this);
                await RefreshFaultyDevicesAsync(); await LoadRecommendationsAsync();
            };
            actions.Controls.AddRange([queues, activity, refresh, profile]);
            Controls.Add(actions);
        }

        private async void TelemetryTimer_Tick(object? sender, EventArgs e)
        {
            if (isRefreshingDevices || isRefreshingTelemetry)
            {
                return;
            }

            await LoadDevicesAsync();
            await RefreshTelemetryAsync();
            await LoadRecommendationsAsync();
        }

        private async void btnStart_Click_1(object sender, EventArgs e)
        {
            telemetryTimer.Start();
            await RefreshTelemetryAsync();
            await LoadRecommendationsAsync();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            telemetryTimer.Stop();
        }

        private async void bnSend_Click(object sender, EventArgs e)
        {
            if (cmbDevices.SelectedItem is not SensorDto selectedSensor)
            {
                MessageBox.Show("Please select a device from the current filter results.");
                return;
            }

            if (cmbCommands.SelectedItem is not string selectedCommand)
            {
                MessageBox.Show("Please select a command.");
                return;
            }

            var request = new CommandRequestDto
            {
                SensorId = selectedSensor.Id,
                CommandName = selectedCommand
            };

            try
            {
                // Flush a pending completed search before recording the successful command.
                await searchTracker.FlushAsync();
                using var response = await httpClient.PostAsJsonAsync("api/commands/send", request);

                if (response.IsSuccessStatusCode)
                {
                    await LoadCommandHistoryAsync();
                    await LoadRecommendationsAsync();

                    MessageBox.Show($"{selectedCommand} command recorded successfully!", "Command Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show($"Unable to send command: {error}", "Command Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API connection error: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnUndo_Click(object sender, EventArgs e)
        {
            try
            {
                using var response = await httpClient.PostAsync("api/commands/undo", null);

                if (response.IsSuccessStatusCode)
                {
                    await LoadCommandHistoryAsync();
                    await LoadRecommendationsAsync();

                    MessageBox.Show("Last command removed from history successfully!", "Undo Command", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    MessageBox.Show($"Unable to undo command: {error}", "Undo Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API connection error: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RealTimeDashboard_FormClosing(object? sender, FormClosingEventArgs e)
        {
            telemetryTimer.Stop();
            searchTracker.Dispose();
            telemetryTimer.Dispose();
            httpClient.Dispose();
        }
    }

    public class SensorDto
    {
        public Guid Id { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Category { get; set; }
    }

    public class TelemetryDto
    {
        public Guid PacketId { get; set; }
        public Guid SensorId { get; set; }
        public double Number { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class CommandRequestDto
    {
        public Guid SensorId { get; set; }
        public string CommandName { get; set; } = string.Empty;
    }

    public class DeviceCommandDto
    {
        public Guid CommandId { get; set; }
        public Guid SensorId { get; set; }
        public string CommandName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class FaultyDevicesDto
    {
        public int TotalFaultyDevices { get; set; }
        public List<Guid> SensorIds { get; set; } = new();
    }
}

/*Reference List
 
 Jamro, M. (2018) C# Data Structures and Algorithms. Birmingham: Packt Publishing.

Microsoft (2026b) HttpClient Class. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient 

Microsoft (2026c) Timer Class (System.Windows.Forms). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.timer 

Microsoft (2026g) JsonDocument Class. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument 
 */