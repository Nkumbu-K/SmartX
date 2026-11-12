# SmartX Part 3: corrected history and mesh dashboard

## Required algorithms

- **AVL tree:** global and per-sensor timestamp indexes in `AvlTree.cs` and
  `TelemetryHistoryService.cs`. Range queries prune irrelevant branches.
- **Adjacency-list graph:** `MeshGraph.cs`. Vertices are registered sensors;
  undirected edges represent simulated wireless connections. Positive edge
  weights are user-entered relative energy estimates, not measured watts.
- **BFS and DFS:** discover the available devices reachable from the saved server
  gateway. Offline/faulty devices are excluded. Traversal order appears in Results.
- **Kruskal:** sorts edges by increasing cost and uses disjoint sets to avoid
  cycles. It produces an MST for a connected active graph, or a forest otherwise.
- **Gateway routes:** a BFS through the Kruskal forest gives the unique tree path
  from source to saved gateway. Fewest Hops searches the full active graph instead.

An MST minimises total network connection cost; it does not guarantee the cheapest
weighted route for every individual source/destination. Tree routing displays the
route along this selected network. The gateway represents the central-server
uplink, which is explicitly labelled and is outside the estimated cost model.
This is a simulation, not live ESP32 discovery or hardware packet forwarding.

## Copy complete replacement files into the school repository

Keep the same paths. Replace all these files together before building:

1. `SmartX.Api/Models/Sensor.cs`
2. `SmartX.Api/Models/MeshTopology.cs`
3. `SmartX.Api/Services/FaultyDeviceService.cs`
4. `SmartX.Api/Services/MeshTopologyService.cs`
5. `SmartX.Api/Endpoints/MeshEndpoints.cs`
6. `SmartX.Api/Program.cs`
7. `SmartX.App/Forms/HistoricalTelemetry.cs`
8. `SmartX.App/Forms/MeshCanvas.cs`
9. `SmartX.App/Forms/NetworkTopology.cs`
10. `README.md`

Also copy this guide into `docs/PART3.md`. No new NuGet package is required.
Do not replace `App_Data`, `.git`, `.vs`, `bin`, or `obj`. Existing sensor,
telemetry and mesh JSON files remain readable; no reset is required. Back up your
existing project and data before replacing files.

If the school solution lists `SmartX.Checks/SmartX.Checks.csproj` but that folder
is absent, remove that project line from `Smart-X_v2.slnx`. The optional checks
project is not required to run the API or desktop application. Keep both existing
API and App project entries. Do not copy the downloaded solution merely to change
application code, since it includes the optional checks project.

## Run on Windows

1. Install .NET 10 SDK and Visual Studio desktop/ASP.NET workloads.
2. Open the school solution and rebuild.
3. Trust the development certificate: `dotnet dev-certs https --trust`.
4. Start the API: `dotnet run --project SmartX.Api`.
5. Start the desktop: `dotnet run --project SmartX.App`.
6. Open Network Topology from the main menu.

Desktop API address: `https://localhost:7154/`, matching existing launch settings.
If Windows blocks downloaded `.resx` files, unblock the trusted ZIP before
extracting it. Keep your existing local data rather than overwriting it with
sample data from a download.

## History: what changed and how to test

Select a source sensor, then Sensor History. The screen initially shows ALL saved
readings for that sensor, oldest first, and sets the visible local time controls
to the saved history's bounds. This works even when saved readings are weeks old.
MAC, location, local timestamp, reading, unit and packet ID appear in the table.
Changing sensor loads its history and clears the previous sensor's results.

Search range calls the AVL endpoint with inclusive UTC bounds. Date controls show
whole seconds; initial bounds are rounded outward to retain fractional-second
readings. Show all / reset dates loads all readings and resets the controls.

Test a broad range, a narrower range, a period outside the data, and reversed
bounds. No matching data gets an explicit message. The uploaded school snapshot
contained 1,515 numeric readings on 18 October 2026, explaining why a previous
last-seven-days default showed an empty table in November.

## Gateway and network controls

The Source sensor dropdown chooses which sensor's route/history to inspect.
Clicking a drawn node also selects that source. Link to chooses the OTHER endpoint
for link editing; it is deliberately separate from the saved server gateway.

Choose a device under Gateway connected to central server and click Save Gateway.
The selection is persisted in `smartx-mesh.json`. The saved label states which
MAC/location currently represents the uplink. Merely changing the dropdown does
not save a different gateway. A square identifies that device on the canvas;
a black outline identifies the source.

Create Demo creates a partial mesh only when there are no links; it never clears
or overwrites an existing network. The graph layout is a spaced, scrollable grid,
not a map of physical distances. Hover on a node for its full MAC and location.

Estimated link cost refers to ONE direct connection. Existing cost loads when
editing endpoints are selected. Save Link adds or updates that undirected edge;
Remove Link removes only that direct edge, not a sensor or a multihop path.
Self-links, unknown endpoints and nonpositive/nonfinite costs are rejected.

## Status and failure handling

- Available + reachable: a working path exists to the saved gateway.
- Available + unreachable: sensor is not offline/faulty, but has no working path.
- Offline: explicitly disabled by Set Offline.
- Faulty: marked faulty by the existing Part 2 fault service or Mark Faulty.

Blue is available/reachable, gold is online but unreachable, grey is offline,
red is faulty, purple is visited by discovery. Purple temporarily overrides the
reachable colour. Device status provides full MAC/location, state, reachability
and gateway role. The summary reports counts without requiring scrolling.
When there is no saved gateway, reachability is explicitly unknown.

Offline flags, fault flags and gateway survive API restarts. Existing Part 2
`Sensor.IsOnline` reflects effective availability. Set Online removes the manual
offline flag; it does not clear a fault. Clear Fault clears the fault but does not
undo a manual offline setting. Newly registered devices begin available.
This is simulated status, not automatic radio heartbeat detection.

Discover: BFS / DFS always starts from the saved gateway and lists visited,
online-but-unreachable and unavailable nodes. It displays the final visited set,
not an animation. An unavailable gateway cannot reach any devices.

Minimum Tree displays selected green edges, total forest cost and component count.
Route to Server: Tree highlights only the selected tree route in orange and fades
unrelated connections. Fewest Hops uses the full active graph. Results show the
source and gateway MAC/location, hops, selected route cost, and total forest cost
separately. A missing path shows N/A rather than suggesting a free zero-cost route.
An explicit labelled server uplink follows the gateway in the textual report.

After a link/status/gateway edit, any current route is recalculated. Changing the
source clears old route highlights and prompts a new route calculation. To test
failure of an intermediate node, select that node, mark it offline, then select
the original source again and recalculate its server route.

## Small manual demonstration

For predictable numerical results, use a separate test copy with three sensors
and no existing demo links. Do not expect isolated three-node totals in a saved
31-device demo network containing many other connections.

- A–B costs 1, B–gateway costs 2, A–gateway costs 8.
- All three links make a partial mesh with an alternate path.
- Save the gateway and select A as source.
- Kruskal selects costs 1 + 2; total tree cost is 3.
- Tree route is A → B → gateway, two hops, route cost 3.
- Fewest Hops is A → gateway, one hop, route cost 8.
- Take B offline. A still reaches gateway directly; new tree route cost is 8.
- Remove the direct A–gateway link. A is now online but unreachable.
- Restore B. A can again reach the gateway through B.

The drawing does not move three chosen nodes into a triangle; it shows their
three connections wherever those nodes are positioned. In the full demo there
may be other alternative paths, so costs/hops can differ from this isolated test.

## API additions

- `PUT /api/network/gateway/{id}` saves the gateway device.
- `GET /api/network/route-to-server?source=GUID&useTree=true` uses the saved gateway.
  No saved gateway returns HTTP 400. False selects full-graph fewest-hop BFS.
- `GET /api/network/` now includes `gatewayId`, node `status` and `reachable`.

Existing links, availability, BFS/DFS, MST, generic route, history, faulty and
recovery endpoints remain compatible. Existing two-field mesh snapshots load
with no gateway chosen, requiring the user to choose/save one explicitly.

## Validation

```sh
dotnet build Smart-X_v2.slnx
dotnet run --project SmartX.Checks
python scripts/check_api.py
```

The latter two are optional developer checks. The Python script starts an isolated
API using a temporary directory; it does not alter your application data.
Cross-compilation on Linux uses `-p:EnableWindowsTargeting=true`. Windows Forms
requires Windows for interactive visual verification. Existing nullable warnings
in older Parts 1–2 files remain separate from these changes.

Algorithm checks include AVL balancing, duplicate timestamps and range pruning,
history isolation, unavailable/disconnected nodes and 30 independent exhaustive
MST comparisons. API checks include gateway persistence, reachability, fault and
offline consistency after restart, alternate routing, UTC bounds and legacy APIs.
The services use locks and atomic mesh-file replacement, but legacy application
collections are not a transactional database for concurrent production traffic.

## References

- AVL tree: https://xlinux.nist.gov/dads/HTML/AVLtree.html
- Minimum spanning trees: https://algs4.cs.princeton.edu/43mst/
- Graph traversal: https://algs4.cs.princeton.edu/41graph/
- Windows Forms: https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/

Use your institution's citation format and disclose assistance as required.
