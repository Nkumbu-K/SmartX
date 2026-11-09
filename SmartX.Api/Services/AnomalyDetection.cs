namespace PROG7312_SmartX.Services
{
    public class AnomalyDetection
    {
        // Checking whether a single reading exceeds the safety thresholds.
        public bool IsAnomaly(double reading, double min, double max)
        {
            return reading < min || reading > max;
        }

        // Counting anomalies recursively.
        public int CountAnomaliesRecursive( double[] readings,double min, double max,  int index = 0)
        {
            ArgumentNullException.ThrowIfNull(readings);

            // Checking if all readings have been checked.
            if (index >= readings.Length)
            {
                return 0;
            }

            // Counting the current reading and checking if it exceeds the thresholds.
            int currentAnomaly = IsAnomaly(readings[index], min, max) ? 1 : 0;

            // Continue checking the remaining readings.
            return currentAnomaly +
                   CountAnomaliesRecursive(readings, min, max, index + 1);
        }
    }
}

/*Reference List
 
 Microsoft, 2026. Conditional operator ?: - C# reference. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/conditional-operator
*/

