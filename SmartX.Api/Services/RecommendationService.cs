 using PROG7312_SmartX.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PROG7312_SmartX.Services
{
    public class RecommendationService
    {
        private readonly DeviceLookupService _deviceLookup;
        private readonly TelemetryHistoryService _historyService;
        private readonly FaultyDeviceService _faultyService;
        private readonly CommandHistoryService _commandHistory;

        public RecommendationService( DeviceLookupService deviceLookup, TelemetryHistoryService historyService,  FaultyDeviceService faultyService, CommandHistoryService commandHistory)
        {
            _deviceLookup = deviceLookup;
            _historyService = historyService;
            _faultyService = faultyService;
            _commandHistory = commandHistory;
        }

        public IEnumerable<object> GetAllRecommendations()
        {
            return _deviceLookup .GetAllDevices().Select(CreateRecommendation).ToList(); //Jamro (2018, pp. 64-67)
        }

        public object? GetRecommendations(Guid sensorId)
        {
            var device = _deviceLookup.GetDevice(sensorId);

            if (device == null)
            {
                return null;
            }

            return CreateRecommendation(device);
        }

        private object CreateRecommendation(Sensor device)
        {
            var recommendations = new List<string>();

            var readings = _historyService //Jamro (2018, pp. 64-67)
                .GetSensorHistory(device.Id)
                .OrderBy(reading => reading.Timestamp)
                .ToList();

            var faultyDevices = _faultyService.GetFaultyDevices();

            bool isFaulty = faultyDevices.Contains(device.Id);

            var commands = _commandHistory //Microsoft (2026b)
                .GetCommandHistory()
                .Where(command => command.SensorId == device.Id)
                .ToList();

            // Recommend an inspection for devices already marked faulty.
            if (isFaulty)
            {
                recommendations.Add("Inspect this device because it is marked as faulty.");
            }

            // Check whether there is enough telemetry to analyse.
            if (readings.Count == 0)
            {
                recommendations.Add( "No telemetry history is available. Check the device connection and telemetry reporting.");
            }
            else
            {
                AnalyseReadingPatterns(readings, recommendations);
            }

            //  Identify repeated manual commands Microsoft (2026c)
            var repeatedCommands = commands
                .GroupBy( command => command.CommandName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() >= 3);

            foreach (var group in repeatedCommands)
            {
                recommendations.Add( $"The '{group.Key}' command has been recorded {group.Count()} times. Investigate whether the device is behaving unexpectedly or needs attention.");
            }

            // Provide a default suggestion 
            bool hasActionableRecommendation = recommendations.Any(
                recommendation => recommendation.StartsWith(
                        "Inspect", StringComparison.OrdinalIgnoreCase) ||
                    recommendation.StartsWith( "No telemetry", StringComparison.OrdinalIgnoreCase) ||
                    recommendation.StartsWith("Potential", StringComparison.OrdinalIgnoreCase) ||
                    recommendation.StartsWith("The '", StringComparison.OrdinalIgnoreCase));

            if (!hasActionableRecommendation)
            {
                recommendations.Add(  "No immediate issue was detected by these checks. Continue monitoring telemetry and command activity.");
            }

            return new
            {
                sensorId = device.Id,
                macAddress = device.MacAddress,
                location = device.Location,
                category = device.Category,
                totalReadings = readings.Count,
                totalCommands = commands.Count,
                isFaulty,
                recommendations
            };
        }
        private static void AnalyseReadingPatterns( List<TelemetryPacket<double>> readings,List<string> recommendations)
        {
            // Need at least six readings to compare two groups of recent and earlier measurements.
            if (readings.Count < 6)
            {
                recommendations.Add( "Collect more telemetry readings to improve historical trend analysis.");

                return;
            }

            double[] values = readings
                .Select(reading => reading.Number)
                .Where(value => !double.IsNaN(value) &&
                                !double.IsInfinity(value))
                .ToArray();

            if (values.Length < 6)
            {
                recommendations.Add("There are not enough valid numeric readings for reliable trend analysis.");

                return;
            }

            // Compare the five latest readings with the five readings immediately before them.
            int recentCount = Math.Min(5, values.Length / 2);

            double[] previousValues = values .Skip(values.Length - (recentCount * 2)) .Take(recentCount) .ToArray();

            double[] recentValues = values .TakeLast(recentCount) .ToArray();

            double previousAverage = previousValues.Average();
            double recentAverage = recentValues.Average();

            // Use relative change where possible. .
            double changePercent = Math.Abs(previousAverage) > 0.000001
                ? ((recentAverage - previousAverage) /
                   Math.Abs(previousAverage)) * 100.0
                : 0.0;
            // Recommend a review if the recent average is more than 20% higher than the preceding readings.
            if (changePercent >= 20.0)
            {
                recommendations.Add( $"Potential rising trend detected: the recent average is {changePercent:F1}% higher than the preceding readings. Review the device and its expected operating range.");
            }

            // Check for unusually high readings compared to the overall historical average.
            double overallAverage = values.Average();

            double variance = values
                .Select(value => Math.Pow(value - overallAverage, 2)) .Average();

            double standardDeviation = Math.Sqrt(variance);
            double latestValue = values[^1]; //Microsoft (2026c)

            if (standardDeviation > 0 && latestValue > overallAverage + (2 * standardDeviation))
            {
                recommendations.Add(  $"Potential unusually high reading detected: the latest value ({latestValue:F2}) is more than two standard deviations above the historical average ({overallAverage:F2}). Investigate whether this is expected.");
            }
        }
    }
}

/*Reference List
 
Jamro, M. (2018) C# Data Structures and Algorithms. Birmingham: Packt Publishing.

Microsoft (2026b) Enumerable.Where Method. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.where 

Microsoft (2026c) Enumerable.GroupBy Method. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby 
 */