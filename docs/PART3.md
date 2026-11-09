# Part 3 implementation

Extend the existing API and Windows Forms application with timestamp-indexed
telemetry queries and a simulated weighted IoT mesh. No live ESP32 discovery is
claimed. Sensor GUIDs and MAC addresses identify nodes; costs are relative power
units, not measured electricity consumption.

## Delivery sequence

25 implementation commits cover: plan; AVL insertion; AVL range search; tree
checks; history indexing; history API; history UI; graph contracts; adjacency
lists; saved topology; BFS; DFS; disjoint sets; Kruskal; routes; availability;
read API; edit API; demo topology; canvas; network controls; menu integration;
network checks; API integration checks; final usage guide.

## Acceptance criteria

- Historical queries use a balanced timestamp tree and inclusive UTC bounds.
- Equal timestamps preserve all readings; existing ingestion indexes readings.
- Registered sensors form vertices; positive finite weighted links form edges.
- Traversals and routing skip unavailable nodes; disconnected meshes are explicit.
- Kruskal produces a minimum spanning forest; a connected mesh produces a tree.
- A selected sensor can display its route to a selected gateway along the forest.
- The desktop menu opens telemetry, commands and network topology.
- Automated algorithm/API checks complement manual Windows UI verification.

## Run on Windows

1. Install .NET 10 SDK and the Visual Studio .NET desktop/ASP.NET workloads.
2. Open `Smart-X_v2.slnx`, restore and build.
3. Trust the development certificate using `dotnet dev-certs https --trust`.
4. Run `dotnet run --project SmartX.Api` and keep it running.
5. Run `dotnet run --project SmartX.App` in another terminal.
6. Open Part 3 from the main menu. Click Create Demo once for an empty mesh,
   or select two sensors, enter a positive power cost and click Save Link.

The desktop uses `https://localhost:7154/`, matching existing launch settings.
The mesh uses simulated availability, initially online; it does not infer radio
connectivity from legacy `Sensor.IsOnline`. Marking a device faulty in Part 2
also makes it unavailable to the mesh. Set Online does not clear that fault;
use the existing recovery action to clear it.

## Demonstration checklist

- Select a sensor and gateway by MAC address/location. Click a node on the canvas
  to select the source. Nodes show the final five characters of their MAC.
- Run BFS and DFS; inspect visited, unreachable online and unavailable lists.
- Click Minimum Tree: green edges show Kruskal's selected links and the report
  shows total cost and component count. Isolated online sensors count as components.
- Click Tree Route: orange edges show hops from source to gateway in the tree.
- Click Fewest Hops to compare a BFS path on the complete available mesh.
- Take an intermediate device offline, then recalculate the route. An alternative
  is used if one exists; otherwise the report explicitly says no route exists.
- Open Sensor History from the network screen, or Search History from telemetry.
  Select local date/time bounds covering saved data, then search. The API converts
  offset timestamps to UTC and includes both endpoints. Results are chronological.
- Restart the API and refresh: links and simulated offline states persist. Existing
  Part 2 fault flags remain in memory, as in the original implementation.
- Verify all three main menu entries and existing telemetry/command workflows on
  Windows before submission. The new screens are created in code, without designer files.

## Algorithms and limitations

AVL insertion and balancing use O(log n) time for n distinct timestamps. Duplicate
keys retain all packets in a bucket. Per-sensor range queries take O(log n + k)
time for k matching readings; only intersecting subtrees are visited. Global and
per-sensor indexes use O(N) storage for N readings, and ingestion updates both.
The packet timestamp and sensor ID must not be mutated after indexing.

BFS/DFS take O(V + E); Kruskal sorts edges in O(E log E) and uses union by rank
with path compression to avoid cycles. An undirected edge can be updated in either
direction; self-loops, unknown endpoints and nonpositive/nonfinite costs are rejected.
Routes recompute the current spanning forest before searching it.

An MST minimises the sum of connection costs over the active network. It does
not minimise every individual sensor-to-server weighted route. Tree Route follows
that tree; Fewest Hops minimises hops on the original graph. The selected gateway
represents the central server connection; no external server vertex is invented.
Costs are user-entered relative power estimates. This is an educational mesh
simulation, not a live ESP32 integration. Demo generation is explicit and refuses
to overwrite existing links. Large dense networks can make the circular canvas
crowded; the textual results remain available.

The history and mesh services lock their own state. Legacy application collections
and ingestion/persistence are retained from Parts 1–2; this is not a replacement
for a transactional database under concurrent production traffic.

## API reference

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/telemetry/history/{id}/range?from=...&to=...` | Inclusive sensor history with ISO 8601 bounds |
| GET | `/api/network/` | Nodes, availability and links |
| PUT | `/api/network/links` | Upsert `{ "from": "GUID", "to": "GUID", "cost": 2.5 }` |
| DELETE | `/api/network/links/{from}/{to}` | Remove an undirected edge |
| PUT | `/api/network/nodes/{id}/availability` | Set `{ "available": false }` |
| GET | `/api/network/traverse/{id}?algorithm=bfs` | BFS (`dfs` also supported) |
| GET | `/api/network/mst` | Minimum spanning forest, cost and component count |
| GET | `/api/network/route?source=GUID&gateway=GUID&useTree=true` | Tree route; false selects fewest-hop BFS |
| POST | `/api/network/demo` | Generate links only when the mesh is empty |

Data lives in `SmartX.Api/App_Data/`. The mesh is stored separately in
`smartx-mesh.json`; telemetry remains in `smartx-data.json`. Do not delete your
data to run automated checks: the integration script uses a temporary directory.

## Verification

```sh
dotnet build Smart-X_v2.slnx
dotnet run --project SmartX.Checks
python scripts/check_api.py
```

On Linux, cross-compile with `dotnet build Smart-X_v2.slnx -p:EnableWindowsTargeting=true`.
Windows Forms requires Windows to run; cross-compilation does not verify visual layout.
The Python integration script requires Python 3 and a built Debug API. Optionally
pass a dotnet executable path as its first argument.

Checks completed during implementation:

- Full solution cross-compilation: passed; existing nullable warnings remain.
- AVL checks: all four rotation shapes, inclusive and reversed bounds, duplicates,
  ordered insertion of 100,000 keys. Final height 17; exact query visits 17 nodes.
- History isolation and graph checks: passed, including disconnected/empty graphs,
  invalid links, offline endpoints, alternate routes and 30 exhaustive MST comparisons.
- Live API checks: passed for validation, fault integration, timestamp bounds,
  save/restart/reload, deletion and demo generation.
- Windows interactive UI and live ESP32 hardware: not tested in the Linux environment.

## References to study and acknowledge

These describe the underlying algorithms; follow your institution's required
citation format and AI assistance disclosure rules. Understand the implementation
before presenting it.

- NIST Dictionary of Algorithms and Data Structures: AVL tree,
  https://xlinux.nist.gov/dads/HTML/AVLtree.html
- Princeton Algorithms: minimum spanning trees,
  https://algs4.cs.princeton.edu/43mst/
- Princeton Algorithms: undirected graphs (BFS and DFS),
  https://algs4.cs.princeton.edu/41graph/
- Microsoft Learn: Windows Forms overview,
  https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/
