namespace PROG7312_SmartX.Models
{
    public class TelemetryReading
    {
        public double Number { get; set; }

        public TelemetryReading(double num)
        {
            Number = num;
        }

        //compares a Telemetry Reading against a numeric threshold and checks if its greater than the threshold
        public static bool operator >(TelemetryReading reading, double threshold) //(Microsoft, 2026)
        { 
             return reading.Number > threshold;
        }

        //compares a Telemetry Reading against a numeric threshold and checks if its less than the threshold
        public static bool operator <(TelemetryReading reading, double threshold) //(Microsoft, 2026)
        {
            return reading.Number < threshold;
        }
    }
}

/*Reference List
 
 Microsoft, 2026. Operator overloading - Define unary, arithmetic, equality, and comparison operators. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading*/
