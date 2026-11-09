using System.Text.Json;
using PROG7312_SmartX.Algorithms;
using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services;

public sealed class MeshTopologyService
{
    private readonly DeviceLookupService devices;
    private readonly FaultyDeviceService faults;
    private readonly string path;
    private readonly object gate = new();
    private List<MeshLink> links = [];
    private HashSet<Guid> offline = [];
    private sealed record SavedTopology(List<MeshLink> Links, HashSet<Guid> Offline);

    public MeshTopologyService(DeviceLookupService devices, FaultyDeviceService faults, IWebHostEnvironment environment)
    {
        this.devices = devices;
        this.faults = faults;
        var directory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "smartx-mesh.json");
        if (File.Exists(path))
        {
            var saved = JsonSerializer.Deserialize<SavedTopology>(File.ReadAllText(path))
                ?? throw new InvalidDataException("Could not read saved topology.");
            links = saved.Links;
            offline = saved.Offline;
            _ = new MeshGraph(Snapshot());
        }
    }

    // Simulation availability is persisted separately from legacy sensor status.
    public MeshSnapshot Snapshot()
    {
        lock (gate)
            return new(devices.GetAllDevices().Select(s => new MeshNode(s.Id, s.MacAddress, s.Location,
                !offline.Contains(s.Id) && !faults.IsFaulty(s.Id))).ToList(), links.ToList());
    }

    public void UpsertLink(MeshLink link)
    {
        lock (gate)
        {
            var updated = links.Where(e => !SameEdge(e, link.From, link.To)).Append(link).ToList();
            _ = new MeshGraph(new(Snapshot().Nodes, updated));
            Save(updated, offline);
            links = updated;
        }
    }

    public bool RemoveLink(Guid from, Guid to)
    {
        lock (gate)
        {
            var updated = links.Where(e => !SameEdge(e, from, to)).ToList();
            if (updated.Count == links.Count) return false;
            Save(updated, offline);
            links = updated;
            return true;
        }
    }

    public void SetAvailability(Guid id, bool available)
    {
        lock (gate)
        {
            if (!devices.DeviceExists(id)) throw new ArgumentException("Sensor not found.");
            var updated = new HashSet<Guid>(offline);
            if (available) updated.Remove(id); else updated.Add(id);
            Save(links, updated);
            offline = updated;
        }
    }

    private static bool SameEdge(MeshLink edge, Guid from, Guid to) =>
        (edge.From == from && edge.To == to) || (edge.From == to && edge.To == from);

    private void Save(List<MeshLink> updatedLinks, HashSet<Guid> updatedOffline)
    {
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(new SavedTopology(updatedLinks, updatedOffline)));
        File.Move(temporary, path, overwrite: true);
    }
}
