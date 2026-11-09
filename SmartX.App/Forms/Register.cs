using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace SmartX.App.Forms
{
    public partial class Register : Form
    {
        private readonly HttpClient _httpClient;
        public Register()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            // HttpClient instance used to send HTTP requests to the backend API (Microsoft, 2026)
            _httpClient.BaseAddress = new Uri("https://localhost:7154/"); 

            LoadCategories();
        }

        private void LoadCategories()
        {
            //Populate sensor category dropdown
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Environmental");
            comboBox1.Items.Add("Power Consumption");
            comboBox1.Items.Add("Actuator");
            comboBox1.SelectedIndex = 0;
        }

        private async void btnReg_Click(object sender, EventArgs e)
        {
            //Validations
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show( "Please enter a MAC Address.","Validation",MessageBoxButtons.OK,MessageBoxIcon.Warning);

                    return;
                }

                if (string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show( "Please enter a Location.","Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var request = new
                {
                    macAddress = textBox1.Text.Trim(),
                    location = textBox2.Text.Trim(),
                    category = comboBox1.SelectedIndex
                };

                //// Serialize the request object into a JSON string
                string json = JsonSerializer.Serialize(request); //(Microsoft, 2026)

                StringContent content = new StringContent(json, Encoding.UTF8,"application/json");

                HttpResponseMessage response = await _httpClient.PostAsync( "api/devices/register", content);

                //Error Handling
                if (!response.IsSuccessStatusCode)
                {
                    string error =
                    await response.Content.ReadAsStringAsync();

                    MessageBox.Show( error, "API Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }

                string result = await response.Content.ReadAsStringAsync();
                MessageBox.Show( "Sensor registered successfully.", "Success", MessageBoxButtons.OK,MessageBoxIcon.Information);

                textBox1.Clear();
                textBox2.Clear();
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message, "Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
/*
 
 Reference List

 Microsoft, 2026. HttpClient Class (System.Net.Http). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient

 Microsoft, 2026. How to serialize and deserialize JSON in .NET. Available at: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to
 */