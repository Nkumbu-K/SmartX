using PROG7312_SmartX.Algorithms;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

foreach (int[] keys in new[] { new[] { 3, 2, 1 }, new[] { 1, 2, 3 }, new[] { 3, 1, 2 }, new[] { 1, 3, 2 } })
{
    var tree = new AvlTree<int, int>();
    foreach (int key in keys) tree.Add(key, key);
    Check(tree.Height == 2, "Each AVL rotation must balance three nodes.");
    Check(tree.All().SequenceEqual(new[] { 1, 2, 3 }), "AVL traversal must sort keys.");
}
var large = new AvlTree<int, int>();
for (int i = 0; i < 100_000; i++) large.Add(i, i);
Check(large.Height < 25, "Sorted input must not degenerate into a chain.");
large.Add(50_000, -1);
Check(large.Range(50_000, 50_000, out int visited).SequenceEqual(new[] { 50_000, -1 }), "Equal timestamps must retain both packets.");
Check(visited < 25, "Narrow queries must prune unrelated branches.");
Check(large.Range(-5, -1).Count == 0, "Out-of-range search must be empty.");
Check(large.Range(10, 12).SequenceEqual(new[] { 10, 11, 12 }), "Bounds must be inclusive.");
try { large.Range(2, 1); throw new Exception("Reversed range accepted."); }
catch (ArgumentException) { }
Console.WriteLine($"AVL checks passed: 100,001 readings, height {large.Height}, exact query visited {visited} nodes.");

// Distinct sensors can share timestamps without leaking into one another's query.
var history = new PROG7312_SmartX.Services.TelemetryHistoryService();
var timestamp = new DateTime(2026, 11, 9, 12, 0, 0, DateTimeKind.Utc);
var a = Guid.Parse("00000000-0000-0000-0000-000000000001");
var b = Guid.Parse("00000000-0000-0000-0000-000000000002");
var c = Guid.Parse("00000000-0000-0000-0000-000000000003");
var d = Guid.Parse("00000000-0000-0000-0000-000000000004");
foreach (var id in new[] { a, b, a })
    history.AddReading(new PROG7312_SmartX.Models.TelemetryPacket<double>(id, 25, "°C") { Timestamp = timestamp });
Check(history.Query(a, timestamp, timestamp).Count == 2 && history.GetReadingCount() == 3, "Per-sensor timestamp buckets must preserve duplicates.");

PROG7312_SmartX.Models.MeshSnapshot Snapshot(bool onlineB = true, bool isolateD = false) => new(
    new[] { a, b, c, d }.Select(id => new PROG7312_SmartX.Models.MeshNode(id, id.ToString(), "Test", id != b || onlineB)).ToList(),
    new List<PROG7312_SmartX.Models.MeshLink> { new(a, b, 1), new(b, c, 2), new(a, c, 8) }
        .Concat(isolateD ? [] : new PROG7312_SmartX.Models.MeshLink[] { new(c, d, 3), new(b, d, 7) }).ToList());
var graph = new MeshGraph(Snapshot());
Check(graph.BreadthFirst(a).Visited.SequenceEqual(new[] { a, b, c, d }), "BFS should be deterministic and cycle-safe.");
Check(graph.DepthFirst(a).Visited.Count == 4, "DFS should visit each reachable node once.");
var forest = graph.MinimumSpanningForest();
Check(forest.TotalCost == 6 && forest.Links.Count == 3 && forest.Components == 1, "Known MST should cost six.");
Check(graph.Route(a, d).Hops.SequenceEqual(new[] { a, b, c, d }), "Tree route should follow selected MST edges.");
Check(graph.Route(a, d, false).Hops.Count == 3, "Full-graph BFS should minimise hop count.");
Check(graph.Route(a, a).Hops.SequenceEqual(new[] { a }), "Self route should have zero cost and one node.");
var failed = new MeshGraph(Snapshot(onlineB: false));
Check(failed.Route(a, d).Hops.SequenceEqual(new[] { a, c, d }), "Failure should reroute through surviving alternative.");
Check(!failed.Route(b, d).Reachable && failed.BreadthFirst(b).Visited.Count == 0, "Unavailable source must not transmit.");
Check(failed.BreadthFirst(a).Unavailable.SequenceEqual(new[] { b }), "Unavailable nodes must be reported.");
var disconnected = new MeshGraph(Snapshot(isolateD: true));
Check(disconnected.MinimumSpanningForest().Components == 2 && !disconnected.Route(a, d).Reachable, "Isolated nodes must produce a forest and no route.");
Check(disconnected.BreadthFirst(a).Unreachable.SequenceEqual(new[] { d }), "Unreachable online node must be distinguished from offline.");
Check(new MeshGraph(new([], [])).MinimumSpanningForest().Components == 0, "Empty mesh must be valid.");
foreach (double invalid in new[] { 0, -1, double.NaN, double.PositiveInfinity })
{
    try { _ = new MeshGraph(new(Snapshot().Nodes, [new(a, b, invalid)])); throw new Exception("Invalid cost accepted."); }
    catch (ArgumentException) { }
}
foreach (var edges in new List<PROG7312_SmartX.Models.MeshLink>[] { [new(a, a, 1)], [new(a, Guid.NewGuid(), 1)], [new(a, b, 1), new(b, a, 2)] })
{
    try { _ = new MeshGraph(new(Snapshot().Nodes, edges)); throw new Exception("Invalid edge accepted."); }
    catch (ArgumentException) { }
}

// Independent exhaustive spanning-tree oracle for small random complete graphs.
var random = new Random(7312);
for (int trial = 0; trial < 30; trial++)
{
    var ids = new[] { a, b, c, d };
    var edges = new List<PROG7312_SmartX.Models.MeshLink>();
    for (int i = 0; i < 4; i++)
        for (int j = i + 1; j < 4; j++) edges.Add(new(ids[i], ids[j], random.Next(1, 20)));
    double best = double.PositiveInfinity;
    for (int mask = 0; mask < (1 << edges.Count); mask++)
    {
        var candidate = edges.Where((_, index) => (mask & (1 << index)) != 0).ToList();
        if (candidate.Count != 3) continue;
        var reached = new HashSet<Guid> { a };
        for (int pass = 0; pass < 4; pass++)
            foreach (var edge in candidate)
                if (reached.Contains(edge.From) || reached.Contains(edge.To)) { reached.Add(edge.From); reached.Add(edge.To); }
        if (reached.Count == 4) best = Math.Min(best, candidate.Sum(e => e.Cost));
    }
    Check(new MeshGraph(new(Snapshot().Nodes, edges)).MinimumSpanningForest().TotalCost == best,
        "Kruskal must match independent exhaustive optimum.");
}
Console.WriteLine("History and mesh checks passed, including 30 exhaustive MST comparisons and failure rerouting.");
