using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Algorithms;

/// <summary>Immutable topology snapshot; algorithms use adjacency lists.</summary>
public sealed partial class MeshGraph
{
    private readonly Dictionary<Guid, MeshNode> nodes;
    private readonly Dictionary<Guid, List<(Guid To, double Cost)>> adjacency;
    public IReadOnlyList<MeshLink> Links { get; }

    public MeshGraph(MeshSnapshot snapshot)
    {
        nodes = snapshot.Nodes.ToDictionary(n => n.Id);
        adjacency = nodes.Keys.ToDictionary(id => id, _ => new List<(Guid, double)>());
        var links = new List<MeshLink>();
        var seen = new HashSet<(Guid, Guid)>();
        foreach (var link in snapshot.Links)
        {
            if (link.From == link.To || !nodes.ContainsKey(link.From) || !nodes.ContainsKey(link.To)
                || !double.IsFinite(link.Cost) || link.Cost <= 0)
                throw new ArgumentException("Links require distinct registered endpoints and a finite positive cost.");
            var key = link.From.CompareTo(link.To) < 0 ? (link.From, link.To) : (link.To, link.From);
            if (!seen.Add(key)) throw new ArgumentException("Duplicate undirected link.");
            links.Add(link);
            adjacency[link.From].Add((link.To, link.Cost));
            adjacency[link.To].Add((link.From, link.Cost));
        }
        foreach (var neighbours in adjacency.Values) neighbours.Sort((a, b) => a.To.CompareTo(b.To));
        Links = links.AsReadOnly();
    }

    private void RequireNode(Guid id)
    {
        if (!nodes.ContainsKey(id)) throw new ArgumentException("Sensor not found in topology.");
    }

    private TraversalResult Result(List<Guid> visited)
    {
        var seen = visited.ToHashSet();
        return new(visited, nodes.Values.Where(n => n.Available && !seen.Contains(n.Id)).Select(n => n.Id).Order().ToList(),
            nodes.Values.Where(n => !n.Available).Select(n => n.Id).Order().ToList());
    }
}
