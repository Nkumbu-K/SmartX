using System.Net.Http;
using System.Net.Http.Json;

namespace SmartX.App.Forms;

// Shared by the sensor browser and command dashboard. A pause or Enter completes a search.
internal sealed class ActivitySearchTracker : IDisposable
{
    private readonly HttpClient client;
    private readonly TextBox search;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 750 };
    private readonly SemaphoreSlim serial = new(1, 1);
    private int revision, recordedRevision;
    private bool disposed;

    public ActivitySearchTracker(HttpClient client, TextBox search)
    {
        this.client = client; this.search = search;
        search.MaxLength = 200;
        search.PlaceholderText = "MAC, location or ID; Enter to record search";
        search.TextChanged += Changed;
        search.KeyDown += KeyDown;
        timer.Tick += Tick;
    }
    private void Changed(object? sender, EventArgs e)
    {
        revision++; timer.Stop();
        if (search.Text.Trim().Length >= 2) timer.Start();
    }
    private async void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        await SafeFlush();
    }
    private async void Tick(object? sender, EventArgs e) { timer.Stop(); await SafeFlush(); }
    private async Task SafeFlush()
    {
        try { await FlushAsync(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Search tracking failed: " + ex.Message); }
    }
    public async Task FlushAsync()
    {
        timer.Stop();
        await serial.WaitAsync();
        try
        {
            if (disposed || recordedRevision == revision) return;
            string term = search.Text.Trim();
            int captured = revision;
            if (term.Length < 2) { recordedRevision = captured; return; }
            using var response = await client.PostAsJsonAsync("api/activity/search", new { searchTerm = term });
            response.EnsureSuccessStatusCode();
            recordedRevision = captured;
        }
        finally { serial.Release(); }
    }
    public void Dispose()
    {
        disposed = true; timer.Stop(); timer.Dispose();
        search.TextChanged -= Changed; search.KeyDown -= KeyDown;
    }
}
