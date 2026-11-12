using System.Net.Http;
using System.Net.Http.Json;

namespace SmartX.App.Forms;

public sealed class NetworkTopology : Form
{
    private readonly HttpClient client = new() { BaseAddress = new Uri("https://localhost:7154/") };
    private readonly MeshCanvas canvas = new() { Dock = DockStyle.Fill };
    private readonly ComboBox source = Selector();
    private readonly ComboBox linkTo = Selector();
    private readonly ComboBox gatewayChoice = Selector();
    private readonly NumericUpDown cost = new() { Minimum = 0.01m, Maximum = 1000000, DecimalPlaces = 2, Value = 1, Width = 85 };
    private readonly TextBox output = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = true };
    private readonly DataGridView nodeGrid = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
    };
    private readonly FlowLayoutPanel toolbar = new() { Dock = DockStyle.Top, Height = 190, AutoScroll = true, Padding = new Padding(6) };
    private readonly Label status = new() { Dock = DockStyle.Bottom, Height = 65, Padding = new Padding(6) };
    private readonly Label gatewayLabel = new() { AutoSize = true, MaximumSize = new Size(900, 0) };
    private readonly TabControl reports = new() { Dock = DockStyle.Right, Width = 420 };
    private readonly ToolTip help = new();
    private bool busy, updating;
    private bool? routeMode;

    private static ComboBox Selector() => new() { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
    private Guid Source => Selected(source) ?? throw new InvalidOperationException("Select a source sensor.");
    private Guid LinkTo => Selected(linkTo) ?? throw new InvalidOperationException("Select the other end of the connection.");
    private Guid Gateway => canvas.Snapshot.GatewayId ?? throw new InvalidOperationException("Select a server gateway and click Save Gateway first.");
    private static Guid? Selected(ComboBox box) => (box.SelectedItem as MeshNodeDto)?.Id;

    public NetworkTopology()
    {
        Text = "SmartX — Network Topology and Server Routing (Simulation)";
        Width = 1550; Height = 950; MinimumSize = new Size(1100, 700);
        var editing = Row();
        editing.Controls.AddRange([Label("Source sensor / link from"), source, Label("Link to"), linkTo,
            Label("Estimated link cost"), cost]);
        AddButton(editing, "Save Link", async () =>
        {
            await Send(HttpMethod.Put, "api/network/links", new MeshLinkDto(Source, LinkTo, (double)cost.Value));
            await AfterChange("Connection saved. Costs are relative estimates, not measured watts.");
        });
        AddButton(editing, "Remove Link", async () =>
        {
            await Send(HttpMethod.Delete, $"api/network/links/{Source}/{LinkTo}");
            await AfterChange("Direct connection removed; both sensors remain registered.");
        });
        var destination = Row();
        destination.Controls.AddRange([Label("Gateway connected to central server"), gatewayChoice]);
        AddButton(destination, "Save Gateway", async () =>
        {
            Guid id = Selected(gatewayChoice) ?? throw new InvalidOperationException("Choose a gateway.");
            await Send(HttpMethod.Put, $"api/network/gateway/{id}");
            await AfterChange("Server gateway saved.");
        });
        destination.Controls.Add(gatewayLabel);
        var operations = Row();
        AddButton(operations, "Refresh", async () => await AfterChange("Network refreshed."));
        AddButton(operations, "Create Demo", async () =>
        {
            await Send(HttpMethod.Post, "api/network/demo");
            await AfterChange("Sample partial mesh created. Select and save a server gateway.");
        });
        AddButton(operations, "Set Offline", () => Availability(false));
        AddButton(operations, "Set Online", () => Availability(true));
        AddButton(operations, "Mark Faulty", async () =>
        {
            await Send(HttpMethod.Post, $"api/devices/{Source}/faulty");
            await AfterChange("Sensor marked faulty; routes exclude it.");
        });
        AddButton(operations, "Clear Fault", async () =>
        {
            await Send(HttpMethod.Post, $"api/devices/{Source}/recover");
            await AfterChange("Fault cleared. A manually offline device stays offline until Set Online is used.");
        });
        AddButton(operations, "Discover: BFS", () => Traverse("bfs"));
        AddButton(operations, "Discover: DFS", () => Traverse("dfs"));
        AddButton(operations, "Minimum Tree", async () => { routeMode = null; await Reload(); await DisplayTree(); });
        AddButton(operations, "Route to Server: Tree", () => StartRoute(true));
        AddButton(operations, "Route to Server: Fewest Hops", () => StartRoute(false));
        AddButton(operations, "Sensor History", () =>
        {
            using var history = new HistoricalTelemetry(Source);
            history.ShowDialog(this);
            return Task.CompletedTask;
        });
        var options = Row();
        var showCosts = new CheckBox { Text = "Show all connection costs", AutoSize = true };
        showCosts.CheckedChanged += (_, _) => { canvas.ShowAllCosts = showCosts.Checked; canvas.Invalidate(); };
        options.Controls.AddRange([showCosts, Label("Hover for full MAC/location. Click a node to select the source. The saved gateway represents the central-server uplink.")]);
        toolbar.Controls.AddRange([editing, destination, operations, options]);
        toolbar.SetFlowBreak(editing, true); toolbar.SetFlowBreak(destination, true); toolbar.SetFlowBreak(operations, true);
        var resultsTab = new TabPage("Results"); resultsTab.Controls.Add(output);
        var nodesTab = new TabPage("Device status"); nodesTab.Controls.Add(nodeGrid);
        reports.TabPages.AddRange([resultsTab, nodesTab]);
        Controls.Add(canvas); Controls.Add(reports); Controls.Add(toolbar); Controls.Add(status);
        help.SetToolTip(cost, "Relative cost of this single connection. Existing cost loads when endpoints change. Save Link applies edits. This is not measured watts or total route cost.");
        source.SelectedIndexChanged += (_, _) => SelectionChanged();
        linkTo.SelectedIndexChanged += (_, _) => { if (!updating) LoadLinkCost(); };
        canvas.NodeClicked += id =>
        {
            if (!busy && source.DataSource is List<MeshNodeDto> items) source.SelectedItem = items.Find(n => n.Id == id);
        };
        Shown += async (_, _) => await Run(async () => { await Reload(); ShowOverview(); });
        FormClosed += (_, _) => { client.Dispose(); help.Dispose(); };
    }

    private static Label Label(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(4, 7, 4, 0) };
    private static FlowLayoutPanel Row() => new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, MaximumSize = new Size(1450, 0) };
    private void AddButton(FlowLayoutPanel row, string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += async (_, _) => await Run(action);
        row.Controls.Add(button);
    }
    private async Task Run(Func<Task> action)
    {
        if (busy) return;
        busy = true; toolbar.Enabled = false;
        try { await action(); }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                ClearHighlights(); routeMode = null;
                output.Text = $"Action could not complete:\r\n{ex.Message}";
                reports.SelectedIndex = 0;
            }
        }
        finally { busy = false; if (!IsDisposed) toolbar.Enabled = true; }
    }
    private async Task Send(HttpMethod method, string path, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }
    private static void Bind(ComboBox box, List<MeshNodeDto> items, Guid? selected, int fallback = 0)
    {
        box.DataSource = items.ToList();
        if (selected.HasValue && items.Find(n => n.Id == selected.Value) is MeshNodeDto item) box.SelectedItem = item;
        else if (items.Count > fallback) box.SelectedIndex = fallback;
    }
    private async Task Reload()
    {
        Guid? selectedSource = Selected(source), selectedLink = Selected(linkTo);
        var snapshot = await client.GetFromJsonAsync<MeshSnapshotDto>("api/network/") ?? new([], []);
        updating = true;
        try
        {
            Bind(source, snapshot.Nodes, selectedSource);
            Bind(linkTo, snapshot.Nodes, selectedLink, snapshot.Nodes.Count > 1 ? 1 : 0);
            Bind(gatewayChoice, snapshot.Nodes, snapshot.GatewayId);
        }
        finally { updating = false; }
        canvas.Snapshot = snapshot; canvas.SourceId = Selected(source);
        ClearHighlights(); LoadLinkCost();
        gatewayLabel.Text = snapshot.GatewayId.HasValue ? $"Saved: {NameOf(snapshot.GatewayId.Value)} → central server"
            : "No saved gateway. Choose a device and Save Gateway.";
        nodeGrid.DataSource = snapshot.Nodes.Select(n => new
        {
            n.MacAddress, n.Location, State = n.Status,
            GatewayConnection = snapshot.GatewayId is null ? "Gateway not set" : n.Reachable ? "Reachable" : "Unreachable",
            Role = n.Id == snapshot.GatewayId ? "Server gateway" : "Sensor"
        }).ToList();
        int reachable = snapshot.Nodes.Count(n => n.Reachable);
        int offline = snapshot.Nodes.Count(n => n.Status == "Offline");
        int faulty = snapshot.Nodes.Count(n => n.Status == "Faulty");
        int unreachable = snapshot.Nodes.Count(n => n.Available && !n.Reachable);
        status.Text = $"{snapshot.Nodes.Count} devices | {snapshot.Links.Count} links | Offline: {offline} | Faulty: {faulty} | " +
            (snapshot.GatewayId.HasValue ? $"Reachable: {reachable} | Online but unreachable: {unreachable}" : "Save a server gateway to calculate reachability") +
            "\r\nBlue: reachable/available; gold: online but unreachable; grey: offline; red: faulty; purple: visited. Square: server gateway; black outline: source; green: tree; orange: route.";
    }
    private void ClearHighlights()
    {
        canvas.Tree = []; canvas.Route = []; canvas.Visited = []; canvas.ViewMode = "Mesh"; canvas.Invalidate();
    }
    private void SelectionChanged()
    {
        if (updating) return;
        routeMode = null; canvas.SourceId = Selected(source); ClearHighlights(); LoadLinkCost(); ShowOverview();
    }
    private void LoadLinkCost()
    {
        var edge = canvas.Snapshot.Links.Find(e => (e.From == Selected(source) && e.To == Selected(linkTo))
            || (e.To == Selected(source) && e.From == Selected(linkTo)));
        cost.Value = edge is null ? 1 : Math.Clamp((decimal)Math.Min(edge.Cost, (double)cost.Maximum), cost.Minimum, cost.Maximum);
    }
    private string NameOf(Guid id) => canvas.Snapshot.Nodes.Find(n => n.Id == id) is MeshNodeDto n
        ? $"{n.MacAddress} ({n.Location})" : id.ToString();
    private void ShowOverview()
    {
        reports.SelectedIndex = 0;
        output.Text = "SIMULATED WIRELESS MESH\r\n\r\n" +
            "1. Select and Save Gateway: that device represents the central-server uplink.\r\n\r\n" +
            "2. Select a source by MAC/location, then Route to Server.\r\n\r\n" +
            "3. Discover runs BFS/DFS from the saved gateway. Device status lists reachability.\r\n\r\n" +
            "4. Use Source / Link to and Estimated link cost to edit a direct wireless connection. Save Link adds or updates; Remove Link removes only that connection.\r\n\r\n" +
            "5. Sensor History opens the selected source's saved timestamped readings.\r\n\r\n" +
            "Costs are relative estimates. The uplink after the gateway is outside the cost model. This simulates connections; it does not discover radio links or forward live hardware packets.";
    }
    private async Task AfterChange(string message)
    {
        bool? recalculate = routeMode;
        await Reload();
        if (recalculate.HasValue) await DisplayRoute(recalculate.Value);
        else ShowOverview();
        output.Text = message + "\r\n\r\n" + output.Text;
    }
    private async Task Availability(bool available)
    {
        Guid id = Source;
        await Send(HttpMethod.Put, $"api/network/nodes/{id}/availability", new { available });
        await AfterChange(available ? "Manual offline setting cleared. A faulty device still requires Clear Fault."
            : "Selected sensor set offline. Any previously displayed route has been recalculated.");
    }
    private async Task Traverse(string algorithm)
    {
        routeMode = null; await Reload();
        var result = await client.GetFromJsonAsync<TraversalDto>($"api/network/traverse/{Gateway}?algorithm={algorithm}");
        if (result is null) return;
        canvas.Visited = result.Visited.ToHashSet(); canvas.Invalidate();
        string List(IEnumerable<Guid> ids) => string.Join("\r\n", ids.Select(NameOf).DefaultIfEmpty("None"));
        output.Text = $"{algorithm.ToUpperInvariant()} FROM SERVER GATEWAY\r\n{NameOf(Gateway)}\r\n\r\n" +
            $"Visit order ({result.Visited.Count}):\r\n{List(result.Visited)}\r\n\r\n" +
            $"Online but unreachable ({result.Unreachable.Count}):\r\n{List(result.Unreachable)}\r\n\r\n" +
            $"Offline/faulty ({result.Unavailable.Count}):\r\n{List(result.Unavailable)}\r\n\r\n" +
            "Discovery skips unavailable nodes. The final visited set is purple; the list shows the traversal order.";
        reports.SelectedIndex = 0;
    }
    private async Task<ForestDto> GetTree() => await client.GetFromJsonAsync<ForestDto>("api/network/mst") ?? new([], 0, 0);
    private async Task DisplayTree()
    {
        var tree = await GetTree();
        canvas.Tree = tree.Links; canvas.ViewMode = "Tree"; canvas.Invalidate();
        string state = tree.Components == 0 ? "No available devices." : tree.Components == 1
            ? "All available devices are connected." : "Disconnected forest: not all available devices can reach each other.";
        output.Text = $"KRUSKAL MINIMUM SPANNING FOREST\r\nSelected links: {tree.Links.Count}\r\n" +
            $"Total network tree cost: {tree.TotalCost:0.##} relative units\r\nActive components: {tree.Components}\r\n{state}\r\n\r\n" +
            string.Join("\r\n", tree.Links.Select(e => $"{NameOf(e.From)} ↔ {NameOf(e.To)}: {e.Cost:0.##}")) +
            "\r\n\r\nKruskal minimises the total selected network connection cost, not every individual sensor-to-gateway path.";
        reports.SelectedIndex = 0;
    }
    private async Task StartRoute(bool useTree)
    {
        await Reload(); await DisplayRoute(useTree);
    }
    private async Task DisplayRoute(bool useTree)
    {
        _ = Gateway;
        var tree = await GetTree();
        var route = await client.GetFromJsonAsync<RouteDto>($"api/network/route-to-server?source={Source}&useTree={useTree}");
        if (route is null) return;
        routeMode = useTree;
        canvas.Tree = useTree ? tree.Links : []; canvas.Route = route.Hops; canvas.ViewMode = "Route"; canvas.Invalidate();
        output.Text = $"SOURCE\r\n{NameOf(Source)}\r\n\r\nSERVER-CONNECTED GATEWAY\r\n{NameOf(Gateway)}\r\n\r\n" +
            $"{route.Message}\r\nReachable: {route.Reachable}\r\nHops: {Math.Max(0, route.Hops.Count - 1)}\r\n" +
            (route.Reachable ? $"Selected route cost: {route.TotalCost:0.##} relative units\r\n" : "Selected route cost: N/A (no route)\r\n") +
            $"Total network MST/forest cost: {tree.TotalCost:0.##} relative units\r\n\r\n" +
            (route.Reachable ? string.Join("\r\n↓\r\n", route.Hops.Select(NameOf)) + "\r\n↓\r\nCentral server (via gateway uplink; not included in cost)"
                : "No available path. Check offline/faulty devices and wireless links.") +
            "\r\n\r\nTree routing follows Kruskal's minimum-total-cost network. Fewest Hops uses BFS. Neither guarantees the cheapest weighted individual route. All costs are estimates, not measured watts.";
        reports.SelectedIndex = 0;
    }
}
