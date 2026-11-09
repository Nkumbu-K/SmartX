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
