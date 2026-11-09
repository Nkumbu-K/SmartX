namespace PROG7312_SmartX.Services
{
    public class AlertPriorityQueueService
    {
        private readonly PriorityQueue<string, int> alertQueue = new();
        private readonly object queueLock = new();
        public void EnqueueAlert(string message, int priority)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(  "Alert message is required.",nameof(message));
            }

            if (priority < 1 || priority > 3)
            {
                throw new ArgumentOutOfRangeException( nameof(priority), "Priority must be between 1 and 3.");
            }

            lock (queueLock)
            {
                alertQueue.Enqueue(message, priority);
            }
        }

        // Process the highest-priority alert.
        public string? DequeueAlert()
        {
            lock (queueLock)
            {
                if (alertQueue.Count == 0) //(Microsoft, 2026b) 
                {
                    return null;
                }

                return alertQueue.Dequeue();
            }
        }

        // Return the number of pending alerts.
        public int GetAlertCount()
        {
            lock (queueLock)
            {
                return alertQueue.Count;
            }
        }
    }
}
/*Reference List

Microsoft, 2026b. PriorityQueue<TElement,TPriority> Class. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2 
*/