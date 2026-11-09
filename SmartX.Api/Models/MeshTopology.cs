namespace PROG7312_SmartX.Models;

// A wireless link is undirected. Cost is a positive relative power estimate.
public sealed record MeshLink(Guid From, Guid To, double Cost);
public sealed record MeshNode(Guid Id, string MacAddress, string Location, bool Available);
public sealed record MeshSnapshot(List<MeshNode> Nodes, List<MeshLink> Links);
public sealed record TraversalResult(List<Guid> Visited, List<Guid> Unreachable, List<Guid> Unavailable);
public sealed record SpanningForest(List<MeshLink> Links, double TotalCost, int Components);
public sealed record MeshRoute(bool Reachable, List<Guid> Hops, double TotalCost, string Message);
