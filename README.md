# Smart-X  — IoT Sensor Management System

## 1. Project Overview

Smart-X  is an IoT sensor management application developed using **C#, .NET 10, ASP.NET Core Minimal APIs, and Windows Forms**.

The application allows users to register and manage sensors, collect and view telemetry readings, monitor device faults, send commands, review command history, undo the most recent command, and receive recommendations based on telemetry and device activity.

The solution consists of two main projects:

- **SmartX.Api:** The backend responsible for API endpoints, application logic, device data, telemetry processing, command handling, and recommendations.
- **SmartX.App:** The Windows Forms desktop application that provides the user interface.

Youtube Video Demo: https://www.youtube.com/watch?v=mWwiKEO6ieA

## 2. Features

### Sensor Management
- Register new sensors.
- View registered devices.
- Access individual sensor profiles.
- View device information and associated attachments.

### Telemetry Collection and Monitoring
- Collect numeric telemetry readings.
- View telemetry history.
- Retrieve readings for individual devices.
- Process queued telemetry packets.
- Analyse readings for anomalies.
- Identify and monitor faulty devices.

### Command Management
- Send supported commands to registered devices.
- View command history.
- Undo the most recent command using command history.
- Track command activity for analysis and recommendations.

### Data Structures and Algorithms
The application uses data structures suited to different operations:

| Data structure | Purpose |
|---|---|
| `Queue<T>` | Processes telemetry packets in first-in, first-out order. |
| `PriorityQueue<TElement, TPriority>` | Prioritises alerts so higher-priority alerts can be processed first. Lower numeric priorities are processed before higher ones. |
| `Stack<T>` | Maintains command history and supports undoing the most recent command. |
| `Dictionary<TKey, TValue>` | Provides efficient device lookup using unique identifiers. |
| `AvlTree<DateTime, TelemetryPacket<double>>` | Indexes global and per-sensor history for timestamp range queries. |
| `HashSet<T>` | Tracks unique faulty devices and avoids duplicate entries. |

### Recommendations and Pattern Analysis
- Analyse historical telemetry readings for unusual patterns.
- Identify devices marked as faulty.
- Generate recommendations based on device status, telemetry, and command activity.
- Record user searches and commands for activity analysis, where the activity-tracking functionality is enabled.
- Identify recurring search-and-command sequences and suggest potentially relevant actions.

Recommendations are intended to assist users in identifying potential issues. They do not automatically execute suggested device commands.

## 3. Technologies Used

- C#
- .NET 10
- ASP.NET Core Minimal APIs
- Windows Forms
- HTTP APIs
- Visual Studio
- GitHub

## 4. Prerequisites

Before running the application, ensure that the following are installed:

- Windows 10 or Windows 11
- .NET 10 SDK
- Visual Studio with the required .NET desktop and ASP.NET development workloads
- Git

## 5. Installation and Setup

### Step 1: Clone the repository

Clone the repository from GitHub and open the project directory.

The solution contains the following main components:

```text
Smart-X_v2/
├── Smart-X_v2.slnx
├── SmartX.Api/
└── SmartX.App/
```

### Step 2: Restore dependencies

Open a terminal in the directory containing `Smart-X_v2.slnx` and run:

```bash
dotnet restore Smart-X_v2.slnx
```

### Step 3: Build the solution

Run:

```bash
dotnet build Smart-X_v2.slnx
```

Resolve any build errors before running the application.

### Step 4: Start the API

Run the backend project:

```bash
dotnet run --project SmartX.Api
```

Alternatively, start the API through Visual Studio.

The configured API addresses are:

- HTTPS: `https://localhost:7154`
- HTTP: `http://localhost:5165`

These addresses assume that the project's launch settings have not been changed.

### Step 5: Start the desktop application

After the API is running, start `SmartX.App` through Visual Studio.

If necessary, configure the solution to start both projects, ensuring that the API starts before the desktop application makes requests.

The desktop application communicates with the backend using HTTP requests.

## 6. Project Structure

```text
Smart-X_v2/
│
├── Smart-X_v2.slnx
│
├── SmartX.Api/
│   ├── Program.cs
│   ├── Data/
│   ├── DataTransfer/
│   ├── Models/
│   ├── Services/
│   ├── Uploads/
│   ├── Properties/
│   └── SmartX.Api.csproj
│
└── SmartX.App/
    ├── Forms/
    │   ├── Form1.cs
    │   ├── Register.cs
    │   ├── Sensors.cs
    │   ├── SensorProfile.cs
    │   ├── Telemetry.cs
    │   └── RealTimeDashboard.cs
    ├── Properties/
    └── SmartX.App.csproj
```

### SmartX.Api

The backend project provides the API and application logic.

- **Program.cs:** Configures the API, registers application services, and defines HTTP endpoints.
- **Data:** Manages application data.
- **DataTransfer:** Contains objects used to transfer information between API requests and responses.
- **Models:** Contains the application's main data entities, such as sensors, telemetry packets, and commands.
- **Services:** Implements application logic for device lookup, telemetry history, command history, fault tracking, alert processing, and recommendations.
- **Uploads:** Stores uploaded sensor media.

### SmartX.App

The desktop project provides the Windows Forms interface.

- **Forms:** Contains screens used to register sensors, browse devices, view sensor profiles, inspect telemetry, and access the dashboard.

## 7. Data Structure Testing Guide

The API can be tested using Thunder Client in Visual Studio Code or Postman.

### 7.1 Preparing the Application

1. Start the `SmartX.Api` project.
2. Confirm that the API is running at `https://localhost:7154`.
3. Open API testing tool.
4. Use the base URL `https://localhost:7154` for the requests below.
5. Register a sensor if you do not already have one. Retrieve its ID using `GET /api/devices`.

### 7.2 Testing the Telemetry Queue (`Queue<T>`)

Verify that telemetry packets are queued and processed in FIFO order.

**Step 1: Add the first telemetry packet**

- Method: `POST`
- Endpoint: `/api/telemetry/queue`
- Request body (JSON):

```json
{
  "sensorId": "SENSOR-GUID",
  "number": 15.5,
  "unit": "°C"
}
```

**Step 2: Add a second telemetry packet**

Submit another request to the same endpoint, using the same sensor ID and a different reading, such as `22.5`.

**Step 3: Check the queue status**

- Method: `GET`
- Endpoint: `/api/telemetry/queue/status`

Assess the response for the number of pending packets.

**Step 4: Process the queue**

- Method: `POST`
- Endpoint: `/api/telemetry/queue/process`

Both packets should be accepted and processed in FIFO order, with the first queued packet processed before the second. The pending queue count should decrease as packets are processed.

### 7.3 Testing the Alert Priority Queue (`PriorityQueue<TElement, TPriority>`)

The implementation uses numeric priorities, with lower numbers representing higher priority.

**Step 1: Queue a low-priority alert**

- Method: `POST`
- Endpoint: `/api/alerts/queue`
- Request body:

```json
{
  "message": "Routine sensor status check",
  "priority": 5
}
```

**Step 2: Queue a high-priority alert**

Submit another request to the same endpoint:

```json
{
  "message": "Critical sensor fault detected",
  "priority": 1
}
```

**Step 3: Queue another alert**

Submit a third request:

```json
{
  "message": "Warning: unusual sensor reading",
  "priority": 3
}
```

**Step 4: Check the queue status**

- Method: `GET`
- Endpoint: `/api/alerts/queue/status`

**Step 5: Process the alerts**

- Method: `POST`
- Endpoint: `/api/alerts/queue/process`

The critical alert with priority `1` should be processed before the warning with priority `3`

### 7.4 Testing Device Lookup (`Dictionary<TKey, TValue>`)

**Step 1: Retrieve registered devices**

- Method: `GET`
- Endpoint: `/api/devices`

Identify a registered device and copy its ID.

**Step 2: Retrieve an individual device**

- Method: `GET`
- Endpoint: `/api/devices/{sensorId}`

Replace `{sensorId}` with the copied device ID.

### 7.5 Testing Command History and Undo (`Stack<T>`)

**Step 1:** Send a command to a registered device.

- Method: `POST`
- Endpoint: `/api/commands/send`

Use the request body expected by the API, for example:

```json
{
  "sensorId": "SENSOR-GUID",
  "commandName": "Disable"
}
```

**Step 2:** Send a second command to the device, or to another registered device.

**Step 3:** Retrieve command history:

- Method: `GET`
- Endpoint: `/api/commands/history`

**Step 4:** Undo the most recent command:

- Method: `POST`
- Endpoint: `/api/commands/undo`

**Step 5:** Retrieve command history again and inspect the result.

## 8. Conclusion

Smart-X  demonstrates the integration of a .NET desktop application with an ASP.NET Core Minimal API backend for IoT sensor management.

The project combines sensor registration, telemetry collection, anomaly detection, command management, data structures, and recommendations in a single application. Its modular design separates the user interface, data models, API communication, and business logic, making the system easier to maintain and extend.




## Part 3 — Historical Queries and Mesh Routing

The network dashboard now separates source/link editing from a saved server-connected
gateway. It displays a spaced scrollable graph, full MAC/location tooltips, availability
and reachability, BFS/DFS discovery from the gateway, Kruskal minimum spanning forests,
and highlighted routes to that gateway. Fault and offline states survive restarts.

History initially loads ALL saved readings for the selected sensor and resets its date
controls to cover them. Users can narrow the range using the per-sensor AVL timestamp
index. MAC address, location, timestamp, reading and unit are visible in the log.

Costs are relative link-energy estimates. Reports distinguish the total network-tree
cost from the selected route's cost. An MST minimises total selected connection cost,
not every individual route. The selected gateway represents the central-server uplink,
which is outside this cost model. No live ESP32 discovery or hardware forwarding is
claimed. Offline status is simulated and uses existing fault flags consistently.

See [full replacement-file list, setup and test walkthrough](docs/PART3.md).

Optional verification: `dotnet run --project SmartX.Checks`; after building the API,
`python scripts/check_api.py`. If the Checks project is absent from your school copy,
remove its project entry from the solution; it is not needed to run the application.
