using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SmartX.App.Forms;

public sealed record MeshNodeDto(Guid Id, string MacAddress, string Location, bool Available,
    string Status = "Online", bool Reachable = false)
{
    public override string ToString() => $"{MacAddress} — {Location}";
}
public sealed record MeshLinkDto(Guid From, Guid To, double Cost);
public sealed record MeshSnapshotDto(List<MeshNodeDto> Nodes, List<MeshLinkDto> Links, Guid? GatewayId = null);
public sealed record ForestDto(List<MeshLinkDto> Links, double TotalCost, int Components);
public sealed record RouteDto(bool Reachable, List<Guid> Hops, double TotalCost, string Message);
public sealed record TraversalDto(List<Guid> Visited, List<Guid> Unreachable, List<Guid> Unavailable);

public sealed class MeshCanvas : Panel
{
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public MeshSnapshotDto Snapshot { get; set; } = new([], []);
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<MeshLinkDto> Tree { get; set; } = [];
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<Guid> Route { get; set; } = [];
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HashSet<Guid> Visited { get; set; } = [];
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Guid? SourceId { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ViewMode { get; set; } = "Mesh";
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowAllCosts { get; set; }
    public event Action<Guid>? NodeClicked;
    private readonly Dictionary<Guid, PointF> positions = new();
    private readonly ToolTip tooltip = new();
    private Guid? hovered;

    public MeshCanvas()
    {
        DoubleBuffered = true; ResizeRedraw = true; AutoScroll = true;
        BackColor = Color.FromArgb(248, 250, 254);
        MouseClick += (_, e) =>
        {
            var id = Hit(e.Location);
            if (id.HasValue) NodeClicked?.Invoke(id.Value);
        };
        MouseMove += (_, e) =>
        {
            Guid? id = Hit(e.Location);
            if (id == hovered) return;
            hovered = id;
            var node = Snapshot.Nodes.Find(n => n.Id == id);
            tooltip.SetToolTip(this, node is null ? "" :
                $"{node.MacAddress}\n{node.Location}\n{node.Status}\n" +
                (Snapshot.GatewayId is null ? "Server gateway not set" : node.Reachable ? "Reachable from server gateway" : "Unreachable from server gateway"));
            Cursor = node is null ? Cursors.Default : Cursors.Hand;
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) tooltip.Dispose();
        base.Dispose(disposing);
    }

    private Guid? Hit(Point p)
    {
        float x = p.X - AutoScrollPosition.X, y = p.Y - AutoScrollPosition.Y;
        foreach (var entry in positions)
            if (Math.Abs(x - entry.Value.X) < 24 && Math.Abs(y - entry.Value.Y) < 24) return entry.Key;
        return null;
    }

    private static bool Same(MeshLinkDto edge, Guid a, Guid b) =>
        (edge.From == a && edge.To == b) || (edge.From == b && edge.To == a);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var ordered = Snapshot.Nodes.OrderBy(n => n.Id).ToList();
        const int columns = 5, cellWidth = 185, cellHeight = 145;
        var extent = new Size(columns * cellWidth + 65, Math.Max(300, ((ordered.Count + columns - 1) / columns) * cellHeight + 100));
        if (AutoScrollMinSize != extent) AutoScrollMinSize = extent;
        e.Graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        positions.Clear();
        for (int i = 0; i < ordered.Count; i++)
        {
            int row = i / columns, column = i % columns;
            if (row % 2 == 1) column = columns - 1 - column;
            positions[ordered[i].Id] = new(85 + column * cellWidth, 90 + row * cellHeight);
        }
        var nodes = Snapshot.Nodes.ToDictionary(n => n.Id);
        string title = ViewMode == "Route" ? "Orange: selected route. Other connections are faded."
            : ViewMode == "Tree" ? "Green: Kruskal's minimum spanning forest. Other connections are faded."
            : "Wireless mesh: lines are connections; positions are for readability, not physical distances.";
        e.Graphics.DrawString(title, Font, Brushes.Black, 12, 12);
        // Paint background edges first, then selected edges so routes remain visible.
        bool IsRoute(MeshLinkDto edge) => Enumerable.Range(0, Math.Max(0, Route.Count - 1))
            .Any(i => Same(edge, Route[i], Route[i + 1]));
        foreach (var link in Snapshot.Links.OrderBy(link => IsRoute(link) ? 2 : Tree.Any(t => Same(t, link.From, link.To)) ? 1 : 0))
        {
            if (!positions.TryGetValue(link.From, out var a) || !positions.TryGetValue(link.To, out var b)) continue;
            bool route = IsRoute(link), tree = Tree.Any(t => Same(t, link.From, link.To));
            bool emphasised = ViewMode == "Route" ? route : ViewMode == "Tree" ? tree : true;
            bool active = nodes[link.From].Available && nodes[link.To].Available;
            Color color = !emphasised ? Color.FromArgb(225, 229, 235)
                : !active ? Color.Silver : route ? Color.DarkOrange : tree ? Color.SeaGreen : Color.SlateGray;
            using var pen = new Pen(color, emphasised && (route || tree) ? 4 : 1.5f);
            if (!active) pen.DashStyle = DashStyle.Dash;
            e.Graphics.DrawLine(pen, a, b);
            if (emphasised && (ShowAllCosts || route || (ViewMode == "Tree" && tree)))
            {
                string label = link.Cost.ToString("0.##");
                SizeF size = e.Graphics.MeasureString(label, Font);
                float x = (a.X + b.X) / 2 - size.Width / 2;
                float y = (a.Y + b.Y) / 2 - size.Height / 2;
                e.Graphics.FillRectangle(Brushes.White, x - 2, y - 1, size.Width + 4, size.Height + 2);
                e.Graphics.DrawString(label, Font, Brushes.Black, x, y);
            }
        }
        foreach (var node in ordered)
        {
            var p = positions[node.Id];
            bool gateway = node.Id == Snapshot.GatewayId;
            Color color = node.Status == "Faulty" ? Color.Firebrick : !node.Available ? Color.DimGray
                : Visited.Contains(node.Id) ? Color.MediumPurple
                : Snapshot.GatewayId.HasValue && !node.Reachable ? Color.Goldenrod : Color.SteelBlue;
            using var brush = new SolidBrush(color);
            if (gateway) e.Graphics.FillRectangle(brush, p.X - 18, p.Y - 18, 36, 36);
            else e.Graphics.FillEllipse(brush, p.X - 17, p.Y - 17, 34, 34);
            if (SourceId == node.Id)
            {
                using var outline = new Pen(Color.Black, 3);
                e.Graphics.DrawEllipse(outline, p.X - 23, p.Y - 23, 46, 46);
            }
            string mac = node.MacAddress.Length > 8 ? node.MacAddress[^8..] : node.MacAddress;
            string state = gateway ? "SERVER GATEWAY" : !node.Available ? node.Status
                : Snapshot.GatewayId.HasValue && !node.Reachable ? "Unreachable" : "";
            using var format = new StringFormat { Alignment = StringAlignment.Center };
            var area = new RectangleF(p.X - 85, p.Y + 28, 170, 75);
            e.Graphics.FillRectangle(Brushes.White, area);
            e.Graphics.DrawString($"{mac}\n{node.Location}\n{state}", Font, Brushes.Black, area, format);
        }
        if (ordered.Count == 0) e.Graphics.DrawString("No registered sensors. Register devices first.", Font, Brushes.Black, 20, 70);
    }
}
