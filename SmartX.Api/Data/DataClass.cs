
using PROG7312_SmartX.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PROG7312_SmartX.Data
{
    public class DataClass
    {
        private readonly string dataFilePath;
        private readonly object dataLock = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public List<Sensor> sensor { get; } = new();
        public List<TelemetryPacket<double>> NumericTelemetry { get; } = new();
        public List<TelemetryPacket<bool>> BooleanTelemetry { get; } = new();
        public double[][] RawTelemetryBatches { get; set; } = new double[0][];

        private class DataSnapshot
        {
            public List<Sensor> Sensors { get; set; } = new();
            public List<TelemetryPacket<double>> NumericTelemetry { get; set; } = new();
            public List<TelemetryPacket<bool>> BooleanTelemetry { get; set; } = new();
            public double[][] RawTelemetryBatches { get; set; } = new double[0][];
        }

        public DataClass(IWebHostEnvironment environment)
        {
            string dataDirectory = Path.Combine( environment.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dataDirectory);

            dataFilePath = Path.Combine( dataDirectory, "smartx-data.json");

            if (File.Exists(dataFilePath))
            {
                LoadFromDisk();
            }
            else
            {
                SeedData();
                SaveToDisk();
            }
        }

        private void LoadFromDisk()
        {
            lock (dataLock)
            {
                string json = File.ReadAllText(dataFilePath);

                DataSnapshot? savedData = JsonSerializer.Deserialize<DataSnapshot>( json,  JsonOptions);

                if (savedData == null)
                {
                    throw new InvalidOperationException( "The saved SmartX data file could not be read.");
                }

                sensor.Clear();
                sensor.AddRange(savedData.Sensors ?? new List<Sensor>());

                NumericTelemetry.Clear();
                NumericTelemetry.AddRange( savedData.NumericTelemetry ?? new List<TelemetryPacket<double>>());

                BooleanTelemetry.Clear();
                BooleanTelemetry.AddRange( savedData.BooleanTelemetry?? new List<TelemetryPacket<bool>>());

                RawTelemetryBatches = savedData.RawTelemetryBatches ?? new double[0][];
            }
        }

        public void SaveToDisk()
        {
            lock (dataLock)
            {
                DataSnapshot snapshot = new()
                {
                    Sensors = sensor,
                    NumericTelemetry = NumericTelemetry,
                    BooleanTelemetry = BooleanTelemetry,
                    RawTelemetryBatches = RawTelemetryBatches
                };

                string json = JsonSerializer.Serialize( snapshot,JsonOptions);
                string temporaryPath = dataFilePath + ".tmp";
                File.WriteAllText(temporaryPath, json);

                File.Move( temporaryPath,  dataFilePath, overwrite: true);
            }
        }

        private void SeedData()
        {
            var sensor1 = new Sensor
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                MacAddress = "AA:BB:CC:DD:EE:01",
                Location = "Pretoria Office",
                Category = SensorType.Environmental
            };

            var sensor2 = new Sensor
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                MacAddress = "AA:BB:CC:DD:EE:02",
                Location = "Rustenburg Office",
                Category = SensorType.PowerConsumption
            };

            var sensor3 = new Sensor 
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                MacAddress = "AA:BB:CC:DD:EE:03",
                Location = "Server Room",
                Category = SensorType.Actuator
            };

            sensor.Add(sensor1);
            sensor.Add(sensor2);
            sensor.Add(sensor3);

            string[] locations =
            {
                "Pretoria Office",
                "Rustenburg Office",
                "Server Room",
                "Johannesburg Office",
                "Cape Town Office",
                "Durban Office",
                "Centurion Warehouse",
                "Midrand Data Centre",
                "Polokwane Branch",
                "Bloemfontein Office"
            };

            SensorType[] categories =
            {
                SensorType.Environmental,
                SensorType.PowerConsumption,
                SensorType.Actuator
            };

            for (int i = 4; i <= 30; i++)
            {
                string idSuffix = i.ToString("D12");

                sensor.Add(new Sensor
                {
                    Id = Guid.Parse( $"00000000-0000-0000-0000-{idSuffix}"),
                    MacAddress = $"AA:BB:CC:DD:{i / 100:X2}:{i % 100:X2}",
                    Location = locations[(i - 1) % locations.Length],
                    Category = categories[(i - 1) % categories.Length]
                });
            }
            var batches = new List<double[]>
            {
                new double[] { 24.5, 25.1, 25.8, 26.2, 26.0 }, //(Microsoft, 2026)
                new double[] { 22.8, 23.4, 24.0 },
                new double[] { 31.7, 32.4, 33.1, 32.8, 33.5, 34.0 }
            };

            TransferRawTelemetryToCollections(sensor1.Id, batches[0], "°C");
            TransferRawTelemetryToCollections( sensor2.Id, batches[1], "kWh");
            TransferRawTelemetryToCollections( sensor3.Id, batches[2], "°C");

            var random = new Random(7312);

            foreach (Sensor currentSensor in sensor)
            {
                var batch = new double[50];

                for (int i = 0; i < batch.Length; i++)
                {
                    double reading = Math.Round(20 + random.NextDouble() * 20,  2);

                    if (currentSensor.Id != sensor1.Id && currentSensor.Id != sensor2.Id && currentSensor.Id != sensor3.Id && i % 17 == 0)
                    {
                        reading = 90 + random.Next(0, 15);
                    }

                    batch[i] = reading;
                }

                batches.Add(batch);

                TransferRawTelemetryToCollections(  currentSensor.Id, batch,  currentSensor.Category == SensorType.PowerConsumption ? "kWh": "°C");
            }

            RawTelemetryBatches = batches.ToArray();
        }

        private void TransferRawTelemetryToCollections( Guid sensorId, double[] rawBatch,string unit)
        {
            foreach (double reading in rawBatch)
            {
                var packet = new TelemetryPacket<double>( sensorId, reading, unit);
                NumericTelemetry.Add(packet);
            }
        }

        public void ProcessRawTelemetryBatch( Guid sensorId,  double[] rawBatch, string unit)
        {
            if (rawBatch == null || rawBatch.Length == 0)
            {
                return;
            }

            lock (dataLock)
            {
                var updatedBatches =  new double[RawTelemetryBatches.Length + 1][];

                Array.Copy( RawTelemetryBatches,  updatedBatches,  RawTelemetryBatches.Length);
                updatedBatches[^1] = rawBatch;
                RawTelemetryBatches = updatedBatches;

                TransferRawTelemetryToCollections(  sensorId, rawBatch, unit);

                SaveToDisk();
            }
        }
    }
}


/*
 Reference List

Microsoft, 2026. Arrays (C# Programming Guide). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/ 

 */