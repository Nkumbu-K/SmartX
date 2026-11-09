using PROG7312_SmartX.Algorithms;
using PROG7312_SmartX.Services;
using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Endpoints;

public sealed record AvailabilityRequest(bool Available);

public static class MeshEndpoints
{
    public static void MapMeshEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/network");
        group.MapPut("/links", (MeshLink link, MeshTopologyService mesh) =>
        {
            try { mesh.UpsertLink(link); return Results.Ok(mesh.Snapshot()); }
            catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });
        group.MapDelete("/links/{from:guid}/{to:guid}", (Guid from, Guid to, MeshTopologyService mesh) =>
            mesh.RemoveLink(from, to) ? Results.NoContent() : Results.NotFound());
        group.MapPut("/nodes/{id:guid}/availability", (Guid id, AvailabilityRequest request,
            DeviceLookupService devices, MeshTopologyService mesh) =>
        {
            if (!devices.DeviceExists(id)) return Results.NotFound(new { message = "Sensor not found." });
            mesh.SetAvailability(id, request.Available);
            return Results.Ok(mesh.Snapshot());
        });
        group.MapGet("/", (MeshTopologyService mesh) => Results.Ok(mesh.Snapshot()));
        group.MapGet("/mst", (MeshTopologyService mesh) => Results.Ok(new MeshGraph(mesh.Snapshot()).MinimumSpanningForest()));
        group.MapGet("/traverse/{start:guid}", (Guid start, string? algorithm, MeshTopologyService mesh) =>
        {
            if (algorithm is not null && algorithm != "bfs" && algorithm != "dfs")
                return Results.BadRequest(new { message = "Algorithm must be bfs or dfs." });
            var snapshot = mesh.Snapshot();
            if (!snapshot.Nodes.Any(n => n.Id == start)) return Results.NotFound(new { message = "Sensor not found." });
            var graph = new MeshGraph(snapshot);
            return Results.Ok(algorithm == "dfs" ? graph.DepthFirst(start) : graph.BreadthFirst(start));
        });
        group.MapGet("/route", (Guid source, Guid gateway, bool? useTree, MeshTopologyService mesh) =>
        {
            var snapshot = mesh.Snapshot();
            if (!snapshot.Nodes.Any(n => n.Id == source) || !snapshot.Nodes.Any(n => n.Id == gateway))
                return Results.NotFound(new { message = "Source or gateway not found." });
            return Results.Ok(new MeshGraph(snapshot).Route(source, gateway, useTree ?? true));
        });
    }
}
