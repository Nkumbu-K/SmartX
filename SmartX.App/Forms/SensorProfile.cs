using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartX.App.Forms
{
    public partial class SensorProfile : Form
    {
        private readonly HttpClient _httpClient;

        private readonly Guid _sensorId;
        private readonly TextBox _recommendations = new()
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical
        };

        public SensorProfile(Guid sensorId)
        {
            InitializeComponent();

            _sensorId = sensorId;
            _httpClient = new HttpClient();
            // HttpClient instance used to send HTTP requests to the backend API (Microsoft, 2026)
            _httpClient.BaseAddress =  new Uri("https://localhost:7154/");

            ClientSize = new Size(900, 790);
            MinimumSize = new Size(850, 780);
            foreach (var box in new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6 }) box.ReadOnly = true;
            var actions = new FlowLayoutPanel { Location = new Point(20, 435), Size = new Size(850, 45), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            var refresh = new Button { Text = "Refresh profile / insights", AutoSize = true };
            var queues = new Button { Text = "Telemetry / Alert queues", AutoSize = true };
            refresh.Click += async (_, _) => await LoadSensor();
            queues.Click += async (_, _) =>
            {
                using var manager = new QueueManagement(_sensorId);
                manager.ShowDialog(this);
                await LoadSensor();
            };
            actions.Controls.AddRange([refresh, queues]);
            var group = new GroupBox
            {
                Text = "Recommendations and search-command patterns for THIS sensor",
                Location = new Point(20, 490), Size = new Size(850, 270),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(10, 25, 10, 10)
            };
            group.Controls.Add(_recommendations);
            Controls.Add(actions); Controls.Add(group);
            Shown += async (_, _) => await LoadSensor();
            FormClosed += (_, _) => _httpClient.Dispose();
        }

        private async Task LoadSensor() //fetches sensor details from the API
        {
            try
            {
                var response = await _httpClient.GetAsync( $"api/devices/{_sensorId}");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show( "Can't to load sensor.");
                    return;
                }

                string json =await response.Content.ReadAsStringAsync();
                // Parse the JSON response string efficiently using JsonDocument (Microsoft, 2026)
                using JsonDocument doc =  JsonDocument.Parse(json);

                JsonElement sensor =  doc.RootElement;

                textBox2.Text = sensor.GetProperty("macAddress").GetString();
                textBox3.Text = sensor.GetProperty("location").GetString();
                int category = sensor.GetProperty("category").GetInt32();
                textBox4.Text = GetCategoryName(category);
                bool faulty = sensor.TryGetProperty("isFaulty", out var fault) && fault.GetBoolean();
                bool online = sensor.TryGetProperty("isOnline", out var available) && available.GetBoolean();
                textBox5.Text = faulty ? "Faulty" : online ? "Online" : "Offline";
                textBox6.Text = sensor.TryGetProperty("lastCommunication", out var last) && last.ValueKind == JsonValueKind.String
                    && last.TryGetDateTime(out var recorded) ? recorded.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                    : "No communication recorded";

                await LoadAttachments();
                await LoadRecommendations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task LoadRecommendations()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<SensorRecommendationsDto>($"api/recommendations/{_sensorId}");
                if (result is null || result.SensorId != _sensorId) throw new InvalidDataException("Recommendation sensor mismatch.");
                _recommendations.Text = $"{result.MacAddress} — {result.Location}\r\n" +
                    $"Readings: {result.TotalReadings} | Commands: {result.TotalCommands} | Faulty: {result.IsFaulty}\r\n\r\n" +
                    string.Join("\r\n\r\n", result.Recommendations) +
                    "\r\n\r\nSEARCH-COMMAND PATTERNS\r\n" +
                    (result.PatternRecommendations.Count == 0 ? "No repeated pattern for this sensor in the current API session."
                        : string.Join("\r\n\r\n", result.PatternRecommendations));
            }
            catch (Exception ex) { _recommendations.Text = "Could not load this sensor's recommendations: " + ex.Message; }
        }

        private string GetCategoryName(int category)
        {
            //Matches a numeric sensor category to the correct representation.
            switch (category)
            {
                case 0:
                    return "Environmental";

                case 1:
                    return "Power Consumption";

                case 2:
                    return "Actuator";

                default:
                    return "Unknown";
            }
        }

        private async Task LoadAttachments() //gets and displays attachment
        {
            try
            {
                var response =   await _httpClient.GetAsync($"api/devices/{_sensorId}/attachments");

                if (!response.IsSuccessStatusCode)
                {
                    textBox1.Text =  "No attachments";
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.GetArrayLength() == 0)
                {
                    textBox1.Text = "No attachments";
                    return;
                }

                string fileName = doc.RootElement[0].GetProperty("attachmentname").GetString() ?? "Attachment";

                textBox1.Text = fileName;
            }
            catch
            {
                textBox1.Text ="No attachments";
            }
        }

        private async void button1_Click(object sender, EventArgs e) //Handles the file upload 
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using MultipartFormDataContent content =new MultipartFormDataContent(); //(Microsoft, 2026)

                using FileStream stream = File.OpenRead(dialog.FileName);

                using StreamContent fileContent = new StreamContent(stream);

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(  "application/octet-stream");

                content.Add( fileContent, "file", Path.GetFileName( dialog.FileName));

                var response = await _httpClient.PostAsync( $"api/devices/{_sensorId}/attachments", content);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show( await response.Content.ReadAsStringAsync());
                    return;
                }

                MessageBox.Show("Upload successful.");
                await LoadAttachments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e) //Opens the Telemetry form to view sensor readings
        {
            using Telemetry telemetry = new Telemetry(_sensorId);
            telemetry.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e) //Cancel
        {
            Close();
        }
    }
}

/*
 Reference List

Microsoft, 2026. MultipartFormDataContent Class. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.multipartformdatacontent
 
 Microsoft, 2026. How to serialize and deserialize JSON in .NET. Available at: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to
 */