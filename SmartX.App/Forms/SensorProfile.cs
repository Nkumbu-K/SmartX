using System.Net.Http.Headers;
using System.Text.Json;

namespace SmartX.App.Forms
{
    public partial class SensorProfile : Form
    {
        private readonly HttpClient _httpClient;

        private readonly Guid _sensorId;

        public SensorProfile(Guid sensorId)
        {
            InitializeComponent();

            _sensorId = sensorId;
            _httpClient = new HttpClient();
            // HttpClient instance used to send HTTP requests to the backend API (Microsoft, 2026)
            _httpClient.BaseAddress =  new Uri("https://localhost:7154/");

            LoadSensor();
        }

        private async void LoadSensor() //fetches sensor details from the API
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
                textBox5.Text = "Active";
                textBox6.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await LoadAttachments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

                string fileName = doc.RootElement[0].GetProperty("attachmentname").GetString();

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