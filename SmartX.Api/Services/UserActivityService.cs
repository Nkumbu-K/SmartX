namespace PROG7312_SmartX.Services;

/// <summary>Session-local search/command events. One search can support one pattern occurrence.</summary>
public sealed class UserActivityService(DeviceLookupService devices)
{
    private readonly object gate = new();
    private readonly List<ActivityRecord> events = [];
    private const int WindowMinutes = 10;
    private const int MaximumEvents = 2000;

    public ActivityRecord RecordSearch(string term, Guid? selectedSensorId = null)
    {
        term = term.Trim();
        if (term.Length < 2 || term.Length > 200)
            throw new ArgumentException("Search text must contain between 2 and 200 characters.");
        var matches = devices.GetAllDevices().Where(d =>
            d.MacAddress.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            d.Location.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            d.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)).Select(d => d.Id).ToList();
        if (selectedSensorId.HasValue && !matches.Contains(selectedSensorId.Value))
            throw new ArgumentException("The selected sensor does not match this search.");
        var record = new ActivityRecord(Guid.NewGuid(), "Search", term,
            selectedSensorId ?? (matches.Count == 1 ? matches[0] : null), null, DateTime.UtcNow, matches);
        lock (gate) { events.Add(record); Trim(); }
        return record;
    }

    // Called ONLY by the validated successful command endpoint, never by the client.
    public void RecordCommand(Guid sensorId, string command)
    {
        lock (gate)
        {
            var now = DateTime.UtcNow;
            var paired = events.Where(e => e.RelatedSearchId.HasValue).Select(e => e.RelatedSearchId!.Value).ToHashSet();
            // Use the latest preceding search, not an arbitrary older match.
            var search = events.LastOrDefault(e => e.EventType == "Search");
            bool canPair = search is not null && search.SensorId.HasValue && !paired.Contains(search.Id)
                && now - search.Timestamp <= TimeSpan.FromMinutes(WindowMinutes);
            events.Add(new(Guid.NewGuid(), "Command", "", sensorId, command.Trim(), now, [],
                canPair ? search!.Id : null, canPair ? search!.SensorId : null));
            Trim();
        }
    }

    private void Trim()
    {
        if (events.Count > MaximumEvents) events.RemoveRange(0, events.Count - MaximumEvents);
    }

    public List<ActivityRecord> GetRecentActivity(int take = 100)
    {
        lock (gate) return events.AsEnumerable().Reverse().Take(Math.Clamp(take, 1, 500)).ToList();
    }

    public List<string> GetPatternRecommendations(Guid? sensorId = null)
    {
        lock (gate)
        {
            return events.Where(e => e.EventType == "Command" && e.RelatedSearchId.HasValue
                    && e.SearchSensorId.HasValue && (!sensorId.HasValue || e.SensorId == sensorId))
                .GroupBy(e => (SearchId: e.SearchSensorId!.Value, TargetId: e.SensorId!.Value,
                    Command: e.CommandName!.ToUpperInvariant()))
                .Where(g => g.Select(e => e.RelatedSearchId).Distinct().Count() >= 2)
                .Select(g =>
                {
                    string searched = devices.GetDevice(g.Key.SearchId)?.MacAddress ?? g.Key.SearchId.ToString();
                    string target = devices.GetDevice(g.Key.TargetId)?.MacAddress ?? g.Key.TargetId.ToString();
                    return $"Observed in {g.Count()} independent search-command sequences: searching for {searched} " +
                        $"was followed by '{g.First().CommandName}' on {target}. Consider reviewing whether this action is appropriate; no command is executed automatically.";
                }).ToList();
        }
    }
}

public sealed record ActivityRecord(Guid Id, string EventType, string SearchTerm, Guid? SensorId,
    string? CommandName, DateTime Timestamp, List<Guid> MatchedSensorIds,
    Guid? RelatedSearchId = null, Guid? SearchSensorId = null);
