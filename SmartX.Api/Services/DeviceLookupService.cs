using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services
{
    public class DeviceLookupService
    {
        private readonly Dictionary<Guid, Sensor> devices = new(); //(Jamro, 2018, pp. 116, 121–122)

        private readonly object deviceLock = new();

        // Get a sensor in the dictionary
        public void AddDevice(Sensor sensor)
        {
            lock (deviceLock)
            {
                devices[sensor.Id] = sensor;
            }
        }

        // Get a sensor using its unique ID
        public Sensor? GetDevice(Guid id)
        {
            lock (deviceLock)
            {
                return devices.TryGetValue(id, out var sensor)
                    ? sensor
                    : null;
            }
        }

        // Get all registered sensors
        public List<Sensor> GetAllDevices()
        {
            lock (deviceLock) //(Microsoft, 2026a)
            {
                return devices.Values.ToList();
            }
        }

        // Check if sensor exists
        public bool DeviceExists(Guid id)
        {
            lock (deviceLock)
            {
                return devices.ContainsKey(id);
            }
        }

        public int GetDeviceCount()
        {
            lock (deviceLock)
            {
                return devices.Count;
            }
        }
    }
}

/*Reference List
 
 Microsoft, 2026a. lock statement (C# Reference). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock
 
 Jamro, M. (2018) C# Data Structures and Algorithms. Birmingham: Packt Publishing.
 */