namespace PROG7312_SmartX.Algorithms;

/// <summary>Union by rank and path compression for cycle detection.</summary>
public sealed class DisjointSet
{
    private readonly Dictionary<Guid, Guid> parent;
    private readonly Dictionary<Guid, int> rank;
    public int Components { get; private set; }

    public DisjointSet(IEnumerable<Guid> ids)
    {
        parent = ids.ToDictionary(id => id, id => id);
        rank = parent.Keys.ToDictionary(id => id, _ => 0);
        Components = parent.Count;
    }

    private Guid Find(Guid id)
    {
        if (parent[id] != id) parent[id] = Find(parent[id]);
        return parent[id];
    }

    public bool Union(Guid first, Guid second)
    {
        var a = Find(first); var b = Find(second);
        if (a == b) return false;
        if (rank[a] < rank[b]) (a, b) = (b, a);
        parent[b] = a;
        if (rank[a] == rank[b]) rank[a]++;
        Components--;
        return true;
    }
}
