using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services
{
    public class SensorService
    {
        private readonly List<Sensor> sensors = new List<Sensor>(); //tore sensor instances

        public List<Sensor> GetSensors() //returns a  list of all sensors
        {
            return sensors;
        }

        public Sensor? GetSensor(Guid id)
        {
            return sensors.FirstOrDefault(s =>  s.Id == id); //(Microsoft, 2026)
        }

        //Add sesnors
        public Sensor AddSensor(Sensor sen)
        {
            sensors.Add(sen);
            return sen;
        }
    }
}

/*Reference List
 Microsoft, 2026. Enumerable.FirstOrDefault Method. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.firstordefault
 */