using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services;

// Records user searches and commands, then detects repeated search
public sealed class UserActivityService
{
    private readonly object _gate = new();
    private readonly List<ActivityRecord> _events = new();
    private readonly DeviceLookupService _devices;
    private readonly CommandHistoryService _commands;
    private const int PatternWindowMinutes = 10;
    private const int MinimumPatternOccurrences = 2;

    public UserActivityService(DeviceLookupService devices, CommandHistoryService commands)
    {
        _devices = devices;
        _commands = commands;
    }

    public ActivityRecord RecordSearch(string searchTerm)
    {
        searchTerm = searchTerm.Trim();
        var matched = _devices.GetAllDevices().FirstOrDefault(d =>
            d.MacAddress.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            d.Location.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            d.Id.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        var record = new ActivityRecord
        {
            EventType = "Search",
            SearchTerm = searchTerm,
            SensorId = matched?.Id,
            Timestamp = DateTime.UtcNow
        };
        lock (_gate) _events.Add(record);
        return record;
    }

    public void RecordCommand(Guid sensorId, string commandName)
    {
        lock (_gate)
        {
            _events.Add(new ActivityRecord
            {
                EventType = "Command",
                SensorId = sensorId,
                CommandName = commandName,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public List<ActivityRecord> GetRecentActivity(int take = 100)
    {
        lock (_gate) return _events.OrderByDescending(e => e.Timestamp).Take(take).ToList();
    }

    public List<string> GetPatternRecommendations()
    {
        List<ActivityRecord> events;
        lock (_gate) events = _events.OrderBy(e => e.Timestamp).ToList();
        var suggestions = new List<string>();
        var searches = events.Where(e => e.EventType == "Search" && e.SensorId.HasValue).ToList();

        foreach (var searchGroup in searches.GroupBy(e => e.SensorId!.Value))
        {
            var searchId = searchGroup.Key;
            var matchedCommands = new List<ActivityRecord>();
            foreach (var search in searchGroup)
            {
                var command = events.FirstOrDefault(e => e.EventType == "Command" &&
                    e.SensorId.HasValue && e.Timestamp >= search.Timestamp &&
                    e.Timestamp <= search.Timestamp.AddMinutes(PatternWindowMinutes));
                if (command != null) matchedCommands.Add(command);
            }

            var repeated = matchedCommands.GroupBy(c => new { c.SensorId, c.CommandName })
                .Where(g => g.Count() >= MinimumPatternOccurrences);
            foreach (var group in repeated)
            {
                var searchedDevice = _devices.GetDevice(searchId);
                var commandedDevice = group.Key.SensorId.HasValue ? _devices.GetDevice(group.Key.SensorId.Value) : null;
                suggestions.Add($"Observed pattern: searching for {searchedDevice?.MacAddress ?? searchId.ToString()} is repeatedly followed by '{group.Key.CommandName}' on {commandedDevice?.MacAddress ?? "a device"}. Consider this command when a similar situation occurs; no command was executed automatically.");
            }
        }
        return suggestions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}

public sealed class ActivityRecord
{
    public string EventType { get; set; } = string.Empty;
    public string SearchTerm { get; set; } = string.Empty;
    public Guid? SensorId { get; set; }
    public string? CommandName { get; set; }
    public DateTime Timestamp { get; set; }
}
