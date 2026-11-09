using PROG7312_SmartX.Algorithms;
using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services;

public class TelemetryHistoryService
{
    private readonly AvlTree<DateTime, TelemetryPacket<double>> history = new();
    private readonly Dictionary<Guid, AvlTree<DateTime, TelemetryPacket<double>>> sensors = new();
    private readonly object historyLock = new();

    public void AddReading(TelemetryPacket<double> packet)
    {
        lock (historyLock)
        {
            history.Add(packet.Timestamp, packet);
            if (!sensors.TryGetValue(packet.SensorId, out var tree))
                sensors[packet.SensorId] = tree = new();
            tree.Add(packet.Timestamp, packet);
        }
    }

    public List<TelemetryPacket<double>> GetAllReadings()
    {
        lock (historyLock) return history.All();
    }

    public List<TelemetryPacket<double>> GetSensorHistory(Guid sensorId)
    {
        lock (historyLock) return sensors.TryGetValue(sensorId, out var tree) ? tree.All() : [];
    }

    public List<TelemetryPacket<double>> Query(Guid sensorId, DateTime from, DateTime to)
    {
        if (from > to) throw new ArgumentException("Start must precede end.");
        lock (historyLock)
            return sensors.TryGetValue(sensorId, out var tree) ? tree.Range(from, to) : [];
    }

    public int GetReadingCount()
    {
        lock (historyLock) return history.Count;
    }
}
