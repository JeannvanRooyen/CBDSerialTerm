using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDSerialLib.Models.NMEA
{
    public class GenericRMC(): Base(EGPSSystemType.Unknown)
    {
        public string? Time { get; set; }
        public char Status { get; set; }
        public GPSCoordinate? GPSCoordinate { get; set; }
        public double Speed { get; set; }
        public double Course { get; set; }
        public string? Date { get; set; }

        public static GenericRMC? FromString(string rmcString)
        {
            if (string.IsNullOrEmpty(rmcString)) return null;

            try
            {
                var strings = rmcString.Split(',');
                var result = new GenericRMC
                {
                    Time = strings[1],
                    Status = strings[2][0],
                    GPSCoordinate = new GPSCoordinate(Helpers.ParseLatitude(strings[3], strings[4]), Helpers.ParseLongitude(strings[5], strings[6]),0),
                    Speed = Helpers.ParseDouble(strings[7]),
                    Course = Helpers.ParseDouble(strings[8]),
                    Date = strings[9]
                };
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing GPRMC: {ex.Message}");
                return null;
            }
        }
    }
}
