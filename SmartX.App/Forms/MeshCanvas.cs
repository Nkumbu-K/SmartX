using System.Drawing.Drawing2D;

namespace SmartX.App.Forms;

public sealed record MeshNodeDto(Guid Id, string MacAddress, string Location, bool Available)
{
    public override string ToString() => $"{MacAddress} — {Location}";
}
public sealed record MeshLinkDto(Guid From, Guid To, double Cost);
public sealed record MeshSnapshotDto(List<MeshNodeDto> Nodes, List<MeshLinkDto> Links);
public sealed record ForestDto(List<MeshLinkDto> Links, double TotalCost, int Components);
public sealed record RouteDto(bool Reachable, List<Guid> Hops, double TotalCost, string Message);
public sealed record TraversalDto(List<Guid> Visited, List<Guid> Unreachable, List<Guid> Unavailable);

public sealed class MeshCanvas : Panel
{
    public MeshSnapshotDto Snapshot { get; set; } = new([], []);
    public List<MeshLinkDto> Tree { get; set; } = [];
    public List<Guid> Route { get; set; } = [];
    public HashSet<Guid> Visited { get; set; } = [];
    public event Action<Guid>? NodeClicked;
    private readonly Dictionary<Guid, PointF> positions = new();

    public MeshCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(245, 248, 252);
        ResizeRedraw = true;
        MouseClick += (_, e) =>
        {
            foreach (var entry in positions)
                if (Math.Pow(e.X - entry.Value.X, 2) + Math.Pow(e.Y - entry.Value.Y, 2) <= 225)
                { NodeClicked?.Invoke(entry.Key); break; }
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        positions.Clear();
        var nodes = Snapshot.Nodes;
        float cx = Width / 2f, cy = Height / 2f;
        float radius = Math.Max(20, Math.Min(cx, cy) - 55);
        for (int i = 0; i < nodes.Count; i++)
        {
            double angle = 2 * Math.PI * i / nodes.Count - Math.PI / 2;
            positions[nodes[i].Id] = new(cx + radius * (float)Math.Cos(angle), cy + radius * (float)Math.Sin(angle));
        }
        bool Same(MeshLinkDto edge, Guid a, Guid b) =>
            (edge.From == a && edge.To == b) || (edge.From == b && edge.To == a);
        foreach (var link in Snapshot.Links)
        {
            if (!positions.TryGetValue(link.From, out var a) || !positions.TryGetValue(link.To, out var b)) continue;
            bool tree = Tree.Any(x => Same(x, link.From, link.To));
            bool route = Enumerable.Range(0, Math.Max(0, Route.Count - 1)).Any(i => Same(link, Route[i], Route[i + 1]));
            using var pen = new Pen(route ? Color.DarkOrange : tree ? Color.SeaGreen : Color.LightSlateGray, route ? 4 : tree ? 3 : 1);
            e.Graphics.DrawLine(pen, a, b);
            string cost = link.Cost.ToString("0.##");
            var point = new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2);
            var size = e.Graphics.MeasureString(cost, Font);
            e.Graphics.FillRectangle(Brushes.White, point.X, point.Y, size.Width, size.Height);
            e.Graphics.DrawString(cost, Font, Brushes.Black, point);
        }
        foreach (var node in nodes)
        {
            var p = positions[node.Id];
            using var brush = new SolidBrush(!node.Available ? Color.DimGray : Visited.Contains(node.Id) ? Color.MediumPurple : Color.SteelBlue);
            e.Graphics.FillEllipse(brush, p.X - 13, p.Y - 13, 26, 26);
            string label = node.MacAddress.Length > 5 ? node.MacAddress[^5..] : node.MacAddress;
            var size = e.Graphics.MeasureString(label, Font);
            e.Graphics.DrawString(label, Font, Brushes.Black, p.X - size.Width / 2, p.Y + 15);
        }
        if (nodes.Count == 0) e.Graphics.DrawString("Load sensors to view the simulated mesh.", Font, Brushes.Black, 15, 15);
    }
}
