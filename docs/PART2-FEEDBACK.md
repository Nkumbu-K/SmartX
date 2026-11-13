# Lecturer feedback corrections: Part 2

## Original feedback

> The submission has a working menu and uses the required data structures sensibly
> (stack, queue, priority queue, dictionaries, HashSet), with solid trend-based
> recommendations. It loses marks because search-pattern tracking is written but
> never wired in, the alert and telemetry queues aren't used from the UI, and
> recommendations aren't shown per sensor.

## Changes implemented

| Feedback | Completed implementation | Evidence to demonstrate |
| --- | --- | --- |
| Search-pattern tracking was not wired in | Registered UserActivityService; search endpoints; shared search tracker for sensor browser/dashboard; successful commands recorded on the server; patterns included with the selected sensor's recommendations | Search an exact MAC, send Restart, repeat the search and command, then open Search activity / patterns |
| Telemetry queue not used from the UI | Queue tools opened from dashboard or sensor profile; sensor, value and unit inputs; enqueue, pending count, process-next and activity log | Queue 11 and 22, process twice; inspect processing order and saved history |
| Alert priority queue not used from the UI | Alert message and priority inputs; enqueue, pending count, process-highest-priority and activity log | Enqueue a Low alert before a High alert; High must process first |
| Recommendations not displayed per sensor | Dashboard requests /api/recommendations/{sensorId}; profile shows its own insights and patterns; stale asynchronous responses are discarded | Select two different sensors and verify the MAC/location heading changes and no other sensor's insights appear |

The sensor profile and sensor browser also display actual online/fault information.
The profile's Last Communication field uses recorded backend data rather than the
current computer time. Both direct ingestion and processed queued telemetry update
that timestamp.

## Files to transfer

This branch builds on the earlier Part 3 usability corrections. Add these FIVE new
files at the same paths in the school repository:

1. `SmartX.Api/Endpoints/ActivityEndpoints.cs`
2. `SmartX.App/Forms/ActivitySearchTracker.cs`
3. `SmartX.App/Forms/SensorRecommendationsDto.cs`
4. `SmartX.App/Forms/QueueManagement.cs`
5. `SmartX.App/Forms/ActivityHistory.cs`

Replace these SIX existing files together:

1. `SmartX.Api/Program.cs`
2. `SmartX.Api/Services/UserActivityService.cs`
3. `SmartX.Api/Services/RecommendationService.cs`
4. `SmartX.App/Forms/RealTimeDashboard.cs`
5. `SmartX.App/Forms/SensorProfile.cs`
6. `SmartX.App/Forms/Sensors.cs`

Optional supporting files: `README.md`, this guide, and
`scripts/check_part2_feedback.py`. No designer files or new NuGet dependencies are
needed. Keep school data, Git metadata and solution configuration. Existing AVL,
graph, MST and network screens stay in the project. Rebuild after transferring
all eleven code files, then restart BOTH the API and desktop.

## Search tracking behaviour

Search text must contain 2–200 characters. Pausing for 750 ms or pressing Enter
records the current search. It does not create an event for each keystroke. Before
the command dashboard submits a command, it flushes any pending search so the
search is recorded first. A command event is recorded only after server-side
validation and successful command-history insertion.

Each command can be paired with the latest preceding unambiguous search within
ten minutes. Each search can support at most one pattern occurrence. Two or more
independent sequences with the same searched sensor, command target and command
produce a suggestion. Command names are grouped case-insensitively. Multiple
searches followed by a single command, or repeated commands without another search,
do not manufacture a repeated pattern. Ambiguous/unmatched searches appear in
activity but are not assigned to an arbitrary sensor. The endpoint also accepts
an explicit matching sensor ID for clients that resolve an ambiguous search.

Patterns are scoped to the commanded sensor in its recommendations. A suggestion
never executes a command automatically. Undo removes a command-history record,
not the fact that the command event occurred in the activity log.

Activity is session-local, retains at most 2,000 events, and resets on API restart.
The viewer shows the most recent 100 events. This single-operator educational API
does not distinguish separate authenticated users; a multiuser version would need
user/session identifiers on events.

## Manual Windows tests

### 1. Search patterns

1. Open the Real-Time Command/Dashboard section from the main menu.
2. Enter one sensor's FULL MAC in Search. Press Enter or pause for one second.
3. Select Restart and send it successfully.
4. Clear Search, re-enter the SAME MAC and press Enter/pause again.
5. Send Restart again. These must be two separate searches and successful commands.
6. Click Refresh insights. The selected sensor's pattern section should describe
   the two sequences.
7. Open Search activity / patterns to inspect search and command events and the
   paired search information.
8. Select another sensor. Recommendations must identify that sensor and must not
   display the first sensor's target-specific pattern.

If you repeat the command without repeating the search, that is not a second
search-command sequence. Use a full MAC or unique GUID; a broad search such as
Office can match several sensors and will not generate a sensor-specific pattern.

### 2. Telemetry FIFO

1. Click Telemetry / Alert queues on the dashboard, or in a sensor profile.
2. Choose a sensor and the Telemetry queue tab.
3. Queue value 11 with a unit, then queue value 22.
4. Inspect the pending count. Counts include all devices/clients; on a clean API
   session with no other producer there should be two waiting packets.
5. Process next twice. The log must show 11 before 22.
6. The pending count decreases with each processed item.
7. Close queue tools and open the selected sensor's history. Processed readings
   should appear and can be retrieved using the AVL range query.
8. Processing an empty queue should show an empty message, not crash.

Enqueueing alone does not add the packet to saved history. Processing updates the
main data store, AVL indexes, Last Communication and disk snapshot.

### 3. Alert priority queue

1. Open the Alert priority queue tab.
2. Enqueue message Low first with priority 3 (Low).
3. Enqueue message High second with priority 1 (High).
4. Enqueue message Medium third with priority 2 (Medium).
5. Process highest priority three times. Expected order: High second, Medium
   third, Low first. Existing higher-priority alerts may be returned first if the
   API queue was not initially empty.
6. Check decreasing pending counts and the processed-message log.

Lower priority numbers are handled first; order within the same numeric priority
is unspecified by PriorityQueue. Processing represents handling a queued alert,
not automatically performing a hardware action.

### 4. Recommendations and sensor status

1. Open a sensor profile from the sensor browser or dashboard.
2. Verify its MAC/location heading and its own readings/command counts.
3. Mark that device faulty through the network dashboard, then refresh the profile.
4. The profile must display Faulty and its recommendations must include inspection.
5. Clear the fault and refresh. If it remains manually offline, its state must
   still be Offline. The profile does not hardcode Active or invent a last-contact
   time; no recorded contact is shown explicitly.

## Automated checks completed

Full solution cross-compilation passed. Existing algorithm checks passed, including
AVL range pruning and exhaustive small-graph MST comparisons. The Part 3 HTTP
integration suite passed after these changes.

The new isolated Part 2 API suite checks:

- One command cannot be counted multiple times for repeated searches.
- Repeated commands without a new search do not produce a search pattern.
- Two independent sequences create one correctly scoped pattern suggestion.
- Invalid commands do not create activity; ambiguous searches are not guessed.
- Selected-sensor recommendations and fault flags match the requested sensor.
- Telemetry queue processing is FIFO, updates saved history and records contact time.
- Priority processing returns High, Medium, Low despite reversed insertion order.
- Pending counts and empty-queue responses are correct.

Run after building the API:

```sh
python scripts/check_part2_feedback.py
```

An optional first argument selects the dotnet executable. These tests use a
temporary store and do not change your saved school data. API and compilation
checks do not replace the Windows UI tests above; record actual results and
screenshots from the final school copy before claiming interactive verification.
