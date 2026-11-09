
using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services
{
    public class TelemetryHistoryService
    {
        private readonly SortedDictionary<DateTime,  List<TelemetryPacket<double>>> history = new(); //(Microsoft, 2026a)

        private readonly object historyLock = new();

        // Store a telemetry reading
        public void AddReading(TelemetryPacket<double> packet)
        {
            lock (historyLock)
            {
                if (!history.TryGetValue(packet.Timestamp,
                    out var readings))
                {
                    readings = new List<TelemetryPacket<double>>();
                    history[packet.Timestamp] = readings;
                }

                readings.Add(packet);
            }
        }

        // Retrieve all readings in chronological order
        public List<TelemetryPacket<double>> GetAllReadings()
        {
            lock (historyLock)
            {
                return history.Values //Jamro (2018, pp. 128-131)
                    .SelectMany(readings => readings)
                    .ToList();
            }
        }

        // Retrieve readings for a specific sensor
        public List<TelemetryPacket<double>> GetSensorHistory(Guid sensorId)
        {
            lock (historyLock)
            {
                return history.Values
                    .SelectMany(readings => readings)
                    .Where(packet => packet.SensorId == sensorId)
                    .ToList();
            }
        }

        // Return the total number of stored readings
        public int GetReadingCount()
        {
            lock (historyLock) //(Microsoft, 2026b)
            {
                return history.Values.Sum(readings => readings.Count);
            }
        }
    }
}

/*Reference List
 
Jamro, M. (2018) C# Data Structures and Algorithms. Birmingham: Packt Publishing.

Microsoft (2026a) SortedDictionary<TKey, TValue> Class. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.sorteddictionary-2

Microsoft (2026b) lock statement (C# Reference). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock
 */
