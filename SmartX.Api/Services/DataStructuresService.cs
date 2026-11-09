using System.Collections.Generic;
using System.Linq;
using SmartX.Api.Models;

namespace SmartX.Api.Services
{
    public class DataStructuresService
    {
        private readonly Queue<TelemetryItem> _telemetryQueue = new();
        private readonly PriorityQueue<SystemAlert, int> _criticalAlertsQueue = new();
        private readonly Stack<UserCommand> _commandStack = new();
        private readonly HashSet<string> _uniqueDeviceTracking = new();
        private readonly Dictionary<string, TelemetryItem> _deviceLookupCache = new();

        public void EnqueueTelemetry(TelemetryItem item)
        {
            _telemetryQueue.Enqueue(item);
            _deviceLookupCache[item.DeviceId] = item;
            _uniqueDeviceTracking.Add(item.DeviceId);
        }

        public TelemetryItem DequeueTelemetry()
        {
            return _telemetryQueue.Count > 0 ? _telemetryQueue.Dequeue() : null;
        }

        public IEnumerable<TelemetryItem> GetAllTelemetry() => _telemetryQueue.ToList();

        public void EnqueueAlert(SystemAlert alert, int priority)
        {
            _criticalAlertsQueue.Enqueue(alert, priority);
        }

        public SystemAlert DequeueNextAlert()
        {
            return _criticalAlertsQueue.Count > 0 ? _criticalAlertsQueue.Dequeue() : null;
        }
        public void PushCommand(UserCommand command)
        {
            _commandStack.Push(command);
        }

        public UserCommand UndoLastCommand()
        {
            return _commandStack.Count > 0 ? _commandStack.Pop() : null;
        }

        public IEnumerable<UserCommand> GetCommandHistory() => _commandStack.ToList();
        public bool IsDeviceUnique(string deviceId)
        {
            return _uniqueDeviceTracking.Contains(deviceId);
        }

        public int GetUniqueDeviceCount() => _uniqueDeviceTracking.Count;

        public TelemetryItem GetDeviceById(string deviceId)
        {
            return _deviceLookupCache.TryGetValue(deviceId, out var item) ? item : null;
        }
    }
}