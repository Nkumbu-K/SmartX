using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Algorithms;

public sealed partial class MeshGraph
{
    // The route follows the minimum-total-cost network tree. It is not a
    // pairwise shortest weighted path; BFS on the full graph minimises hops.
    public MeshRoute Route(Guid source, Guid gateway, bool useTree = true)
    {
        RequireNode(source); RequireNode(gateway);
        if (!nodes[source].Available || !nodes[gateway].Available)
            return new(false, [], 0, "Source or gateway is unavailable.");
        var graph = useTree ? new MeshGraph(new(nodes.Values.ToList(), MinimumSpanningForest().Links)) : this;
        var previous = new Dictionary<Guid, (Guid Parent, double Cost)>();
        var seen = new HashSet<Guid> { source };
        var queue = new Queue<Guid>();
        queue.Enqueue(source);
        while (queue.TryDequeue(out var current))
        {
            if (current == gateway) break;
            foreach (var next in graph.adjacency[current])
            {
                if (!nodes[next.To].Available || !seen.Add(next.To)) continue;
                previous[next.To] = (current, next.Cost);
                queue.Enqueue(next.To);
            }
        }
        if (!seen.Contains(gateway)) return new(false, [], 0, "No available route to gateway.");
        var hops = new List<Guid> { gateway };
        double cost = 0;
        for (var current = gateway; current != source;)
        {
            var step = previous[current];
            cost += step.Cost;
            current = step.Parent;
            hops.Add(current);
        }
        hops.Reverse();
        return new(true, hops, cost, useTree ? "Route along the minimum spanning tree." : "Fewest-hop route through the available mesh.");
    }
}
