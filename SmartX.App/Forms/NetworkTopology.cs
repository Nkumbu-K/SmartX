using System.Net.Http;
using System.Net.Http.Json;

namespace SmartX.App.Forms;

public sealed class NetworkTopology : Form
{
    private readonly HttpClient client = new() { BaseAddress = new Uri("https://localhost:7154/") };
    private readonly MeshCanvas canvas = new() { Dock = DockStyle.Fill };
    private readonly ComboBox source = new() { Width = 310, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox gateway = new() { Width = 310, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown cost = new() { Minimum = 0.01m, Maximum = 1000000, DecimalPlaces = 2, Value = 1, Width = 90 };
    private readonly TextBox output = new() { Dock = DockStyle.Right, Width = 330, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
    private readonly FlowLayoutPanel toolbar = new() { Dock = DockStyle.Top, Height = 135, AutoScroll = true };
    private readonly Label status = new() { Dock = DockStyle.Bottom, Height = 28 };
    private bool busy;

    public NetworkTopology()
    {
        Text = "SmartX — Network Topology and Mesh Routing (Simulation)";
        Width = 1350; Height = 850; MinimumSize = new Size(950, 650);
        toolbar.Controls.AddRange([new Label { Text = "Sensor / link from", AutoSize = true }, source,
            new Label { Text = "Gateway / link to", AutoSize = true }, gateway,
            new Label { Text = "Power cost", AutoSize = true }, cost]);
        AddButton("Refresh", Reload);
        AddButton("Create Demo", async () => { await Send(HttpMethod.Post, "api/network/demo"); await Reload(); });
        AddButton("Save Link", async () => { await Send(HttpMethod.Put, "api/network/links", new MeshLinkDto(Source, Gateway, (double)cost.Value)); await Reload(); });
        AddButton("Remove Link", async () => { await Send(HttpMethod.Delete, $"api/network/links/{Source}/{Gateway}"); await Reload(); });
        AddButton("Set Offline", () => Availability(false));
        AddButton("Set Online", () => Availability(true));
        AddButton("BFS", () => Traverse("bfs"));
        AddButton("DFS", () => Traverse("dfs"));
        AddButton("Minimum Tree", ShowTree);
        AddButton("Tree Route", () => ShowRoute(true));
        AddButton("Fewest Hops", () => ShowRoute(false));
        AddButton("Sensor History", () => { using var history = new HistoricalTelemetry(Source); history.ShowDialog(this); return Task.CompletedTask; });
        Controls.Add(canvas); Controls.Add(output); Controls.Add(toolbar); Controls.Add(status);
        canvas.NodeClicked += id => source.SelectedItem = ((List<MeshNodeDto>)source.DataSource).Find(n => n.Id == id);
        Shown += async (_, _) => await Run(Reload);
        FormClosed += (_, _) => client.Dispose();
    }

    private Guid Source => (source.SelectedItem as MeshNodeDto)?.Id ?? throw new InvalidOperationException("Select a sensor.");
    private Guid Gateway => (gateway.SelectedItem as MeshNodeDto)?.Id ?? throw new InvalidOperationException("Select a gateway.");
    private void AddButton(string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += async (_, _) => await Run(action);
        toolbar.Controls.Add(button);
    }
    private async Task Run(Func<Task> action)
    {
        if (busy) return;
        busy = true; toolbar.Enabled = false;
        try { await action(); }
        catch (Exception ex) { status.Text = "Action failed; see details."; output.Text = ex.Message; }
        finally { busy = false; toolbar.Enabled = true; }
    }
    private async Task Send(HttpMethod method, string path, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }
    private async Task Reload()
    {
        var snapshot = await client.GetFromJsonAsync<MeshSnapshotDto>("api/network/") ?? new([], []);
        Guid? selectedSource = (source.SelectedItem as MeshNodeDto)?.Id;
        Guid? selectedGateway = (gateway.SelectedItem as MeshNodeDto)?.Id;
        source.DataSource = snapshot.Nodes.ToList();
        gateway.DataSource = snapshot.Nodes.ToList();
        if (selectedSource.HasValue) source.SelectedItem = snapshot.Nodes.Find(n => n.Id == selectedSource);
        if (selectedGateway.HasValue) gateway.SelectedItem = snapshot.Nodes.Find(n => n.Id == selectedGateway);
        else if (gateway.Items.Count > 1) gateway.SelectedIndex = 1;
        canvas.Snapshot = snapshot;
        canvas.Tree = []; canvas.Route = []; canvas.Visited = []; canvas.Invalidate();
        output.Text = string.Join(Environment.NewLine, snapshot.Nodes.Select(n => $"{n.MacAddress} | {(n.Available ? "online" : "unavailable")} | {n.Location}"));
        status.Text = $"{snapshot.Nodes.Count} nodes, {snapshot.Links.Count} links. Blue: online; grey: unavailable; purple: visited; green: tree; orange: route. Click a node to select it.";
    }
    private async Task Availability(bool available)
    {
        await Send(HttpMethod.Put, $"api/network/nodes/{Source}/availability", new { available });
        await Reload();
    }
    private string Name(Guid id) => canvas.Snapshot.Nodes.Find(n => n.Id == id)?.MacAddress ?? id.ToString();
    private async Task Traverse(string algorithm)
    {
        await Reload();
        var result = await client.GetFromJsonAsync<TraversalDto>($"api/network/traverse/{Source}?algorithm={algorithm}");
        if (result is null) return;
        canvas.Visited = result.Visited.ToHashSet(); canvas.Invalidate();
        output.Text = $"{algorithm.ToUpperInvariant()} visit order:\r\n{string.Join("\r\n", result.Visited.Select(Name))}\r\n\r\nUnreachable online nodes:\r\n{string.Join("\r\n", result.Unreachable.Select(Name))}\r\n\r\nUnavailable:\r\n{string.Join("\r\n", result.Unavailable.Select(Name))}";
    }
    private async Task ShowTree()
    {
        await Reload();
        var tree = await client.GetFromJsonAsync<ForestDto>("api/network/mst");
        if (tree is null) return;
        canvas.Tree = tree.Links; canvas.Invalidate();
        output.Text = $"Kruskal: {tree.Links.Count} links\r\nTotal power cost: {tree.TotalCost:0.##}\r\nActive components: {tree.Components}\r\n" +
            (tree.Components == 1 ? "Connected minimum spanning tree." : "Disconnected forest; no single tree connects all active nodes.") +
            "\r\n\r\n" + string.Join("\r\n", tree.Links.Select(e => $"{Name(e.From)} ↔ {Name(e.To)}: {e.Cost}"));
    }
    private async Task ShowRoute(bool useTree)
    {
        await ShowTree();
        var route = await client.GetFromJsonAsync<RouteDto>($"api/network/route?source={Source}&gateway={Gateway}&useTree={useTree}");
        if (route is null) return;
        if (!useTree) canvas.Tree = [];
        canvas.Route = route.Hops; canvas.Invalidate();
        output.Text = $"{route.Message}\r\nReachable: {route.Reachable}\r\nRoute cost: {route.TotalCost:0.##}\r\n\r\n{string.Join("\r\n↓\r\n", route.Hops.Select(Name))}\r\n\r\nTree routes follow the minimum total network cost. Fewest Hops uses BFS; neither claims the cheapest weighted path for an individual pair.";
    }
}
