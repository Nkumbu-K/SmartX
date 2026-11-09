namespace PROG7312_SmartX.Services
{
    public class FaultyDeviceService
    {
        private readonly HashSet<Guid> faultyDevices = new(); //Jamro (2018)

        private readonly object deviceLock = new();

        // Mark a device as faulty
        public bool MarkAsFaulty(Guid sensorId)
        {
            lock (deviceLock)
            {
                return faultyDevices.Add(sensorId);
            }
        }

        // Remove device after fixed
        public bool MarkAsRecovered(Guid sensorId)
        {
            lock (deviceLock)
            {
                return faultyDevices.Remove(sensorId); //Jamro (2018)
            }
        }

        // Check whether a device is faulty
        public bool IsFaulty(Guid sensorId)
        {
            lock (deviceLock)
            {
                return faultyDevices.Contains(sensorId);
            }
        }

        //Get all faulty devices
        public List<Guid> GetFaultyDevices()
        {
            lock (deviceLock)
            {
                return faultyDevices.ToList();
            }
        }

        // Count unique faulty devices
        public int GetFaultyCount()
        {
            lock (deviceLock)
            {
                return faultyDevices.Count;
            }
        }
    }
}
/*
 Reference List 

  Jamro, M. (2018) C# Data Structures and Algorithms. Birmingham: Packt Publishing.
*/