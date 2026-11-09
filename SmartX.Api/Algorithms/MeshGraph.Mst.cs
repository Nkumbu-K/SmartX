using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Algorithms;

public sealed partial class MeshGraph
{
    // O(E log E). Disconnected active nodes produce a forest, never a fake route.
    public SpanningForest MinimumSpanningForest()
    {
        var sets = new DisjointSet(nodes.Values.Where(n => n.Available).Select(n => n.Id));
        var chosen = new List<MeshLink>();
        double cost = 0;
        foreach (var link in Links.Where(e => nodes[e.From].Available && nodes[e.To].Available)
            .OrderBy(e => e.Cost)
            .ThenBy(e => e.From.CompareTo(e.To) < 0 ? e.From : e.To)
            .ThenBy(e => e.From.CompareTo(e.To) < 0 ? e.To : e.From))
        {
            if (!sets.Union(link.From, link.To)) continue;
            chosen.Add(link);
            cost += link.Cost;
        }
        return new(chosen, cost, sets.Components);
    }
}
