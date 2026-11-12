using System.Text.Json;
using PROG7312_SmartX.Algorithms;
using PROG7312_SmartX.Data;
using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services;

public sealed class MeshTopologyService
{
    private readonly DeviceLookupService devices;
    private readonly FaultyDeviceService faults;
    private readonly DataClass data;
    private readonly string path;
    private readonly object gate = new();
    private List<MeshLink> links = [];
    private HashSet<Guid> offline = [];
    private Guid? gatewayId;
    private sealed record SavedTopology(List<MeshLink> Links, HashSet<Guid> Offline, Guid? GatewayId = null);

    public MeshTopologyService(DeviceLookupService devices, FaultyDeviceService faults,
        DataClass data, IWebHostEnvironment environment)
    {
        this.devices = devices;
        this.faults = faults;
        this.data = data;
        string directory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "smartx-mesh.json");
        if (File.Exists(path))
        {
            var saved = JsonSerializer.Deserialize<SavedTopology>(File.ReadAllText(path))
                ?? throw new InvalidDataException("Saved topology could not be read.");
            links = saved.Links;
            offline = saved.Offline;
            gatewayId = saved.GatewayId;
        }
        // Upgrade legacy stores: the mesh's explicit offline set is authoritative.
        foreach (var sensor in devices.GetAllDevices())
        {
            sensor.MeshOffline = offline.Contains(sensor.Id);
            sensor.IsOnline = !sensor.MeshOffline && !faults.IsFaulty(sensor.Id);
        }
        _ = new MeshGraph(Snapshot());
        data.SaveToDisk();
    }

    public MeshSnapshot Snapshot()
    {
        lock (gate)
        {
            var nodes = devices.GetAllDevices().Select(s =>
            {
                bool faulty = faults.IsFaulty(s.Id);
                bool available = !offline.Contains(s.Id) && !faulty;
                return new MeshNode(s.Id, s.MacAddress, s.Location, available,
                    faulty ? "Faulty" : available ? "Online" : "Offline");
            }).ToList();
            var snapshot = new MeshSnapshot(nodes, links.ToList(), gatewayId);
            if (gatewayId is Guid id && nodes.Any(n => n.Id == id))
            {
                var reachable = new MeshGraph(snapshot).BreadthFirst(id).Visited.ToHashSet();
                nodes = nodes.Select(n => n with { Reachable = reachable.Contains(n.Id) }).ToList();
            }
            return new(nodes, links.ToList(), gatewayId);
        }
    }

    public void SetGateway(Guid id)
    {
        lock (gate)
        {
            RequireDevice(id);
            Save(links, offline, id);
            gatewayId = id;
        }
    }

    public void UpsertLink(MeshLink link)
    {
        lock (gate)
        {
            var updated = links.Where(e => !SameEdge(e, link.From, link.To)).Append(link).ToList();
            _ = new MeshGraph(new(Snapshot().Nodes, updated));
            Save(updated, offline, gatewayId);
            links = updated;
        }
    }

    public bool RemoveLink(Guid from, Guid to)
    {
        lock (gate)
        {
            var updated = links.Where(e => !SameEdge(e, from, to)).ToList();
            if (updated.Count == links.Count) return false;
            Save(updated, offline, gatewayId);
            links = updated;
            return true;
        }
    }

    public void SetAvailability(Guid id, bool available)
    {
        lock (gate)
        {
            RequireDevice(id);
            var updated = new HashSet<Guid>(offline);
            if (available) updated.Remove(id); else updated.Add(id);
            Save(links, updated, gatewayId);
            offline = updated;
            var sensor = devices.GetDevice(id)!;
            sensor.MeshOffline = !available;
            sensor.IsOnline = available && !faults.IsFaulty(id);
            data.SaveToDisk();
        }
    }

    // Creates a partial mesh only when no links exist; never deletes user data.
    public bool TryCreateDemo()
    {
        lock (gate)
        {
            if (links.Count > 0) return false;
            var ids = devices.GetAllDevices().Select(s => s.Id).Order().ToArray();
            if (ids.Length < 2) throw new ArgumentException("Register at least two sensors first.");
            var demo = new List<MeshLink>();
            for (int i = 1; i < ids.Length; i++) demo.Add(new(ids[i - 1], ids[i], 1 + i % 5));
            for (int i = 2; i < ids.Length; i++) demo.Add(new(ids[i - 2], ids[i], 4 + i % 3));
            Save(demo, offline, gatewayId);
            links = demo;
            return true;
        }
    }

    private void RequireDevice(Guid id)
    {
        if (!devices.DeviceExists(id)) throw new ArgumentException("Sensor not found.");
    }

    private static bool SameEdge(MeshLink edge, Guid from, Guid to) =>
        (edge.From == from && edge.To == to) || (edge.From == to && edge.To == from);

    private void Save(List<MeshLink> updatedLinks, HashSet<Guid> updatedOffline, Guid? updatedGateway)
    {
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(new SavedTopology(updatedLinks, updatedOffline, updatedGateway)));
        File.Move(temporary, path, overwrite: true);
    }
}
