using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services
{
    public class TelemetryQueueService
    {
        private readonly Queue<TelemetryPacket<double>> telemetryQueue = new Queue<TelemetryPacket<double>>(); //(Microsoft, 2026)

        private readonly object queueLock = new object();

        // Add a new sensor reading 
        public void EnqueueTelemetry(TelemetryPacket<double> packet)
        {
            lock (queueLock)
            {
                telemetryQueue.Enqueue(packet); // Jamro(2018, p. 96)
            }
        }

        // Process the oldest sensor reading
        public TelemetryPacket<double>? DequeueTelemetry()
        {
            lock (queueLock)
            {
                if (telemetryQueue.Count == 0)
                    return null;

                return telemetryQueue.Dequeue(); //Jamro (2018, p. 96)
            }
        }

        // Check how many readings are waiting.
        public int GetQueueCount()
        {
            lock (queueLock)
            {
                return telemetryQueue.Count;
            }
        }
    }
}
/*
 Reference List 
  
Jamro, M. (2018) C# Data Structures and Algorithms. Birmingham: Packt Publishing.

Microsoft (2026) Queue Class. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1
*/