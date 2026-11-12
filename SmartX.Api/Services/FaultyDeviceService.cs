using PROG7312_SmartX.Data;

namespace PROG7312_SmartX.Services;

public sealed class FaultyDeviceService
{
    private readonly HashSet<Guid> faultyDevices;
    private readonly object gate = new();
    private readonly DeviceLookupService devices;
    private readonly DataClass data;

    public FaultyDeviceService(DeviceLookupService devices, DataClass data)
    {
        this.devices = devices;
        this.data = data;
        faultyDevices = data.sensor.Where(s => s.IsFaulty).Select(s => s.Id).ToHashSet();
    }

    public bool MarkAsFaulty(Guid id) => SetFault(id, true);
    public bool MarkAsRecovered(Guid id) => SetFault(id, false);

    private bool SetFault(Guid id, bool faulty)
    {
        lock (gate)
        {
            var sensor = devices.GetDevice(id);
            if (sensor is null) return false;
            bool changed = faulty ? faultyDevices.Add(id) : faultyDevices.Remove(id);
            sensor.IsFaulty = faulty;
            sensor.IsOnline = !sensor.MeshOffline && !faulty;
            data.SaveToDisk();
            return changed;
        }
    }

    public bool IsFaulty(Guid id) { lock (gate) return faultyDevices.Contains(id); }
    public List<Guid> GetFaultyDevices() { lock (gate) return faultyDevices.ToList(); }
    public int GetFaultyCount() { lock (gate) return faultyDevices.Count; }
}
