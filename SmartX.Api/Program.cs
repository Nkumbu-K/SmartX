using PROG7312_SmartX.Endpoints;
using PROG7312_SmartX.Data;
using PROG7312_SmartX.DataTransfer;
using PROG7312_SmartX.Models;
using PROG7312_SmartX.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DataClass>();
builder.Services.AddSingleton<AnomalyDetection>();
builder.Services.AddSingleton<TelemetryQueueService>();
builder.Services.AddSingleton<AlertPriorityQueueService>();
builder.Services.AddSingleton<DeviceLookupService>();
builder.Services.AddSingleton<TelemetryHistoryService>();
builder.Services.AddSingleton<FaultyDeviceService>();
builder.Services.AddSingleton<CommandHistoryService>();
builder.Services.AddSingleton<RecommendationService>();

var app = builder.Build();

// Initialise dictionary with sensors
var dataStore = app.Services.GetRequiredService<DataClass>();
var deviceLookup = app.Services.GetRequiredService<DeviceLookupService>();
var historyService = app.Services.GetRequiredService<TelemetryHistoryService>();

foreach (var existingSensor in dataStore.sensor)
{
    deviceLookup.AddDevice(existingSensor);
}

// Load saved and seeded telemetry into the history service.
foreach (var packet in dataStore.NumericTelemetry)
{
    historyService.AddReading(packet);
}
app.UseHttpsRedirection();
app.MapHistoryQueries();

//Get all registered devices
app.MapGet("/api/devices", (DeviceLookupService deviceLookup) =>
{
    return Results.Ok(deviceLookup.GetAllDevices());
});


//Retrieve a specific sensor 

app.MapGet("/api/devices/{id:guid}",
    (Guid id, DeviceLookupService deviceLookup) =>
    {
        var sensor = deviceLookup.GetDevice(id);

        if (sensor == null)
        {
            return Results.NotFound(new
            {
                message = "Sensor not found."
            });
        }

        return Results.Ok(sensor);
    });


//Register a new sensor
app.MapPost("/api/devices/register",(registerSensor request, DataClass data,DeviceLookupService deviceLookup) =>
  {
      // Validations
      if (string.IsNullOrWhiteSpace(request.MacAddress))
      {
          return Results.BadRequest(new
          {
              message = "MAC address is required."
          });
      }

      if (string.IsNullOrWhiteSpace(request.Location))
      {
          return Results.BadRequest(new
          {
              message = "Sensor location is required."
          });
      }

      // Create new sensor
      var sensor = new Sensor
      {
          MacAddress = request.MacAddress,
          Location = request.Location,
          Category = request.Category
      };

      data.sensor.Add(sensor);
      deviceLookup.AddDevice(sensor);
      data.SaveToDisk();

      // Return 201 Created
      return Results.Created($"/api/devices/{sensor.Id}", sensor);
  });

//Upload files associated with a specific sensor (Microsoft, 2026)
app.MapPost("/api/devices/{id:guid}/attachments",
  async (Guid id, IFormFile file,DataClass data) =>
  {
      // Find sensor
      var sensor = data.sensor.FirstOrDefault(s => s.Id == id);

      if (sensor == null)
      {
          return Results.NotFound(new
          {
              message = "Sensor not found."
          });
      }

      // Validate file
      if (file == null || file.Length == 0)
      {
          return Results.BadRequest(new
          {
              message = "A file is required."
          });
      }

      // Create uploads folder
      var uploadsDirectory = Path.Combine( Directory.GetCurrentDirectory(),"uploads"); //(Microsoft, 2026)

      Directory.CreateDirectory(uploadsDirectory);

      // Generate filename
      var extension = Path.GetExtension(file.FileName);

      var generatedFileName = $"{Guid.NewGuid()}{extension}";

      var filePath = Path.Combine(uploadsDirectory,generatedFileName);

      // Save file
      await using var stream = new FileStream(filePath,FileMode.Create);
      await file.CopyToAsync(stream);

      var attachment = new SensorAttachments
      {
          attachmentname = file.FileName,
          Path = filePath
      };

      // Add attachment to sensor
      sensor.attachment.Add(attachment);

      return Results.Ok(new
      {
          message = "Attachment uploaded successfully.",
          sensorId = sensor.Id,
          attachment
      });
  })
.DisableAntiforgery();

app.MapGet("/api/devices/{id:guid}/attachments",
  (Guid id, DataClass data) =>
  {
      var sensor = data.sensor .FirstOrDefault(s => s.Id == id);

      if (sensor == null)
      {
          return Results.NotFound(new
          {
              message = "Sensor not found."
          });
      }

      return Results.Ok(sensor.attachment);
  });



app.MapPost("/api/telemetry", (TelemetryRequest request, DataClass data,TelemetryHistoryService historyService) =>
{
    // Checking that the sensor is registered.
    var sensor = data.sensor .FirstOrDefault(s => s.Id == request.SensorId);

    if (sensor == null)
    {
        return Results.NotFound(new
        {
            message = "Sensor is not registered."
        });
    }

    // Validating the telemetry unit.
    if (string.IsNullOrWhiteSpace(request.Unit))
    {
        return Results.BadRequest(new
        {
            message = "Telemetry unit is required."
        });
    }

    // Create the telemetry packet.
    var packet = new TelemetryPacket<double>( request.SensorId, request.Number, request.Unit );

    // Store the reading in the main data store.
    data.NumericTelemetry.Add(packet);

    // Add the same reading to telemetry history.
    historyService.AddReading(packet);
    data.SaveToDisk();

    return Results.Ok(new
    {
        message = "Telemetry received successfully.",
        packet.PacketId,
        packet.SensorId,
        packet.Number,
        packet.Unit,
        packet.Timestamp
    });
});



app.MapGet("/api/telemetry/device/{id:guid}",
  (Guid id, DataClass data) =>
  {
      // Check that sensor exists
      var sensor = data.sensor .FirstOrDefault(s => s.Id == id);

      if (sensor == null)
      {
          return Results.NotFound(new
          {
              message = "Sensor not found."
          });
      }

      // Get telemetry belonging to sensor
      var telemetry = data.NumericTelemetry.Where(t => t.SensorId == id).ToList();

      return Results.Ok(telemetry);
  });

app.MapPost("/api/telemetry/queue",
    (TelemetryRequest request, DataClass data,
     TelemetryQueueService queueService) =>
    {
        // Check whether the sensor exists
        var sensor = data.sensor
            .FirstOrDefault(s => s.Id == request.SensorId);

        if (sensor == null)
        {
            return Results.NotFound(new
            {
                message = "Sensor is not registered."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Unit) ||
            double.IsNaN(request.Number) ||
            double.IsInfinity(request.Number))
        {
            return Results.BadRequest(new
            {
                message = "A valid reading and unit are required."
            });
        }

        // Create a telemetry packet
        var packet = new TelemetryPacket<double>(
            request.SensorId,
            request.Number,
            request.Unit
        );

        // Add packet to queue
        queueService.EnqueueTelemetry(packet);

        return Results.Ok(new
        {
            message = "Telemetry added to queue.",
            packet.PacketId,
            packet.SensorId,
            queueCount = queueService.GetQueueCount()
        });
    });



app.MapPost(
    "/api/telemetry/queue/process", (TelemetryQueueService queueService, DataClass data, TelemetryHistoryService historyService ) =>
    {
        // Get the oldest packet from the FIFO queue.
        var packet = queueService.DequeueTelemetry();

        // Stop if the queue is empty.
        if (packet == null)
        {
            return Results.NotFound(new
            {
                message = "Telemetry queue is empty."
            });
        }

        data.NumericTelemetry.Add(packet);

        // Add the packet to the sorted telemetry history.
        historyService.AddReading(packet);
        data.SaveToDisk();

        return Results.Ok(new
        {
            message = "Telemetry processed successfully.",
            packet,
            remainingCount = queueService.GetQueueCount()
        });
    });


app.MapGet("/api/telemetry/queue/status",
    (TelemetryQueueService queueService) =>
    {
        return Results.Ok(new
        {
            pendingPackets = queueService.GetQueueCount()
        });
    });


app.MapPost("/api/alerts/queue",
    (AlertRequest request, AlertPriorityQueueService alertService) =>
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new
            {
                message = "Alert message is required."
            });
        }

        if (request.Priority < 1 || request.Priority > 3)
        {
            return Results.BadRequest(new
            {
                message = "Priority must be between 1 and 3."
            });
        }

        alertService.EnqueueAlert(request.Message, request.Priority);

        return Results.Ok(new
        {
            message = "Alert added to priority queue.",
            alert = request.Message,
            priority = request.Priority,
            pendingAlerts = alertService.GetAlertCount()
        });
    });


app.MapPost("/api/devices/{id:guid}/faulty",
    (Guid id,
     DeviceLookupService deviceLookup,
     FaultyDeviceService faultyService) =>
    {
        // Making sure the sensor exists first
        if (!deviceLookup.DeviceExists(id))
        {
            return Results.NotFound(new
            {
                message = "Sensor not found."
            });
        }

        bool added = faultyService.MarkAsFaulty(id);

        return Results.Ok(new
        {
            message = added
                ? "Sensor marked as faulty."
                : "Sensor is already marked as faulty.",
            sensorId = id,
            added,
            totalFaultyDevices = faultyService.GetFaultyCount()
        });
    });

//recovered sensors
app.MapPost("/api/devices/{id:guid}/recover",
    (Guid id,
     DeviceLookupService deviceLookup,
     FaultyDeviceService faultyService) =>
    {
        if (!deviceLookup.DeviceExists(id))
        {
            return Results.NotFound(new
            {
                message = "Sensor not found."
            });
        }

        bool removed = faultyService.MarkAsRecovered(id);

        return Results.Ok(new
        {
            message = removed
                ? "Sensor marked as recovered."
                : "Sensor was not marked as faulty.",
            sensorId = id,
            removed,
            totalFaultyDevices = faultyService.GetFaultyCount()
        });
    });

//get all faulty sensors

app.MapGet("/api/devices/faulty",
    (FaultyDeviceService faultyService) =>
    {
        return Results.Ok(new
        {
            totalFaultyDevices = faultyService.GetFaultyCount(),
            sensorIds = faultyService.GetFaultyDevices()
        });
    });

app.MapPost("/api/alerts/queue/process",
    (AlertPriorityQueueService alertService) =>
    {
        var alert = alertService.DequeueAlert();

        if (alert == null)
        {
            return Results.NotFound(new
            {
                message = "Alert queue is empty."
            });
        }

        return Results.Ok(new
        {
            message = "Alert processed successfully.",
            processedAlert = alert,
            remainingAlerts = alertService.GetAlertCount()
        });
    });


app.MapGet("/api/alerts/queue/status",
    (AlertPriorityQueueService alertService) =>
    {
        return Results.Ok(new
        {
            pendingAlerts = alertService.GetAlertCount()
        });
    });


app.MapGet("/api/telemetry/history", (TelemetryHistoryService historyService) =>
    {
        return Results.Ok(historyService.GetAllReadings());
    });


app.MapGet("/api/telemetry/history/{sensorId:guid}",(Guid sensorId, TelemetryHistoryService historyService) =>
    {
        //var readings = historyService.GetSensorHistory(sensorId);

        return Results.Ok(historyService.GetSensorHistory(sensorId));
    });


app.MapGet("/api/telemetry/history/count",(TelemetryHistoryService historyService) =>
    {
        return Results.Ok(new
        {
            totalReadings = historyService.GetReadingCount()
        });
    });

// Analyse a sensor's telemetry readings for anomalies.
app.MapPost("/api/telemetry/analyze",  (TelemetryAnalysisRequest request,DeviceLookupService deviceLookup,TelemetryHistoryService historyService, AnomalyDetection anomalyDetection, AlertPriorityQueueService alertService, FaultyDeviceService faultyService) =>
    {
        // Validate the supplied thresholds.
        if (double.IsNaN(request.Min) || double.IsNaN(request.Max) || double.IsInfinity(request.Min) || double.IsInfinity(request.Max) || request.Min > request.Max)
        {
            return Results.BadRequest(new
            {
                message = "Valid thresholds are required. Min must not exceed Max."
            });
        }

        // Checking that the sensor exists.
        if (!deviceLookup.DeviceExists(request.SensorId))
        {
            return Results.NotFound(new
            {
                message = "Sensor not found."
            });
        }

        // Retrieving the sensor's stored telemetry.
        var readings = historyService .GetSensorHistory(request.SensorId) .ToList();

        if (readings.Count == 0)
        {
            return Results.NotFound(new
            {
                message = "No telemetry history is available for this sensor."
            });
        }

        // Extract numeric readings for recursive anomaly counting.
        double[] values = readings .Select(reading => reading.Number) .ToArray();

        int anomalyCount = anomalyDetection.CountAnomaliesRecursive(values, request.Min, request.Max  );

        bool latestReadingIsAnomaly = anomalyDetection.IsAnomaly( values[^1], request.Min, request.Max );

        // Generate an alert and mark the sensor faulty when anomalies exist.
        if (anomalyCount > 0)
        {
            alertService.EnqueueAlert( $"Sensor {request.SensorId} has {anomalyCount} anomalous reading(s).", 1 );
            faultyService.MarkAsFaulty(request.SensorId);
        }

        return Results.Ok(new
        {
            sensorId = request.SensorId,
            minimumThreshold = request.Min,
            maximumThreshold = request.Max,
            totalReadingsAnalysed = values.Length,
            anomalyCount,
            latestReading = values[^1],
            latestReadingIsAnomaly,
            sensorMarkedFaulty = anomalyCount > 0,
            pendingAlerts = alertService.GetAlertCount()
        });
    });

app.MapPost("/api/commands/send",
    (CommandRequest request,
     DeviceLookupService deviceLookup,
     CommandHistoryService commandHistory) =>
    {
        if (!deviceLookup.DeviceExists(request.SensorId))
        {
            return Results.NotFound(new
            {
                message = "Sensor not found."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CommandName))
        {
            return Results.BadRequest(new
            {
                message = "Command name is required."
            });
        }

        var allowedCommands = new[]
        {
            "Enable",
            "Disable",
            "Restart",
            "Triggering a water pump",
            "Toggling a smart relay"
        };

        var commandName = request.CommandName.Trim();

        if (!allowedCommands.Contains(
            commandName, StringComparer.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new
            {
                message = "Unsupported command."
            });
        }

        var command = new DeviceCommand
        {
            SensorId = request.SensorId,
            CommandName = commandName
        };

        commandHistory.PushCommand(command);

        return Results.Ok(new
        {
            message = "Command recorded successfully.",
            command,
            totalCommands = commandHistory.GetCommandCount()
        });
    });


app.MapPost("/api/commands/undo",
    (CommandHistoryService commandHistory) =>
    {
        var undoneCommand = commandHistory.UndoLastCommand();

        if (undoneCommand == null)
        {
            return Results.NotFound(new
            {
                message = "No commands available to undo."
            });
        }

        return Results.Ok(new
        {
            message = "Latest command removed from history.",
            undoneCommand,
            remainingCommands = commandHistory.GetCommandCount()
        });
    });


app.MapGet("/api/commands/history",
    (CommandHistoryService commandHistory) =>
    {
        return Results.Ok(commandHistory.GetCommandHistory());
    });


app.MapGet("/api/commands/count",
    (CommandHistoryService commandHistory) =>
    {
        return Results.Ok(new
        {
            totalCommands = commandHistory.GetCommandCount()
        });
    });


app.MapGet("/api/recommendations",
    (RecommendationService recommendationService) =>
    {
        return Results.Ok(
            recommendationService.GetAllRecommendations());
    });

app.MapGet("/api/recommendations/{sensorId:guid}", (Guid sensorId, RecommendationService recommendationService) =>
    {
        var recommendations =  recommendationService.GetRecommendations(sensorId);

        if (recommendations == null)
        {
            return Results.NotFound(new
            {
                message = "Sensor not found."
            });
        }

        return Results.Ok(recommendations);
    });
app.Run();

/*
 Reference List

Microsoft, 2026. Overview of Minimal APIs in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis 

Microsoft, 2026. File uploads in ASP.NET Core. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads 
 */