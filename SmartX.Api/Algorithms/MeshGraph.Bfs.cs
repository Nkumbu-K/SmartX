using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Algorithms;

public sealed partial class MeshGraph
{
    public TraversalResult BreadthFirst(Guid start)
    {
        RequireNode(start);
        if (!nodes[start].Available) return Result([]);
        var visited = new List<Guid>();
        var seen = new HashSet<Guid> { start };
        var queue = new Queue<Guid>();
        queue.Enqueue(start);
        while (queue.TryDequeue(out var current))
        {
            visited.Add(current);
            foreach (var neighbour in adjacency[current])
                if (nodes[neighbour.To].Available && seen.Add(neighbour.To)) queue.Enqueue(neighbour.To);
        }
        return Result(visited);
    }
}
