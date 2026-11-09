using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Algorithms;

public sealed partial class MeshGraph
{
    public TraversalResult DepthFirst(Guid start)
    {
        RequireNode(start);
        if (!nodes[start].Available) return Result([]);
        var visited = new List<Guid>();
        var seen = new HashSet<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(start);
        while (stack.TryPop(out var current))
        {
            if (!seen.Add(current)) continue;
            visited.Add(current);
            // Reverse push order keeps ascending IDs deterministic.
            foreach (var neighbour in adjacency[current].AsEnumerable().Reverse())
                if (nodes[neighbour.To].Available && !seen.Contains(neighbour.To)) stack.Push(neighbour.To);
        }
        return Result(visited);
    }
}
