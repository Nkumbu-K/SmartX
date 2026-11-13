using System.Text.Json;

namespace SmartX.App.Forms
{
    public partial class Sensors : Form
    {
        private readonly HttpClient _httpClient;
        private readonly ActivitySearchTracker _searchTracker;
        public class SensorDto //Represents a Data Transfer Object
        {
            public Guid Id { get; set; }
            public string MacAddress { get; set; } = "";
            public string Location { get; set; } = "";
            public int Category { get; set; }
            public bool IsOnline { get; set; }
            public bool IsFaulty { get; set; }
        }

        private List<SensorDto> _sensors = new List<SensorDto>(); 

        private Guid _selectedSensorId;

        public Sensors()
        {
            InitializeComponent();

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7154/")
            };

            _searchTracker = new ActivitySearchTracker(_httpClient, textBox1);
            AddBackToMenuButton();

            LoadSensors();

            FormClosing += (_, _) => { _searchTracker.Dispose(); _httpClient.Dispose(); };
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

        private async void LoadSensors() //retrieves the list of sensors
        {
            try
            {
                var response = await _httpClient.GetAsync("api/devices");

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Could not load sensors.");
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();

                //// Deserialize JSON response into a  list
                _sensors = JsonSerializer.Deserialize<List<SensorDto>>(  json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true //(Microsoft, 2026)
                        }) ?? new();

                textBox1_TextChanged(textBox1, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DisplaySensors(List<SensorDto> sensors)
        {
            panel1.Controls.Clear();

            if (!sensors.Any(s => s.Id == _selectedSensorId)) _selectedSensorId = Guid.Empty;
            panel1.AutoScroll = true;
            int top = 10;

            foreach (var sensor in sensors)
            {
                Label lbl = new Label();

                lbl.Width = 630;
                lbl.Height = 25;

                lbl.Text =
                    $"{sensor.MacAddress} | " +
                    $"{sensor.Location} | " +
                    $"{GetCategory(sensor.Category)} | " +
                    (sensor.IsFaulty ? "Faulty" : sensor.IsOnline ? "Online" : "Offline");

                lbl.Tag = sensor.Id;

                lbl.Click += Sensor_Click;
                panel1.Controls.Add(lbl);
                lbl.Top = top;
                lbl.Left = 10;
                top += 30;
            }
        }

        private void Sensor_Click( object? sender,EventArgs e)
        {
            if (sender is not Label lbl || lbl.Tag is not Guid id) return;
            _selectedSensorId = id;

            // Reset background colors of all items in the panel
            foreach (Control control in panel1.Controls)
            {
                control.BackColor =  SystemColors.Control;
            }
            lbl.BackColor =  Color.LightBlue;
        }

        private string GetCategory(int category)
        {
            return category switch
            {
                0 => "Environmental",
                1 => "Power Consumption",
                2 => "Actuator",
                _ => "Unknown"
            };
        }

        //verifying selection and opening the detailed sensor profile form.
        private async void btnViewSensors_Click( object sender,EventArgs e)
        {
            if (_selectedSensorId == Guid.Empty)
            {
                MessageBox.Show( "Select a sensor first.");
                return;
            }

            try
            {
                await _searchTracker.FlushAsync();
                using var profile = new SensorProfile(_selectedSensorId);
                profile.ShowDialog(this);
                LoadSensors();
            }
            catch (Exception ex) { MessageBox.Show("Could not open sensor: " + ex.Message); }
        }

        //Filters the displayed sensor list
        private void textBox1_TextChanged(object sender,EventArgs e)
        {
            string search = textBox1.Text.ToLower();

            var filtered = _sensors.Where(s => s.MacAddress.ToLower().Contains(search) || s.Location.ToLower().Contains(search) || s.Id.ToString().ToLower().Contains(search))  .ToList();
            DisplaySensors(filtered);
        }
    }
}

/*
 
 Reference List
Microsoft, 2026. How to serialize and deserialize JSON in .NET. Available at: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to
 */