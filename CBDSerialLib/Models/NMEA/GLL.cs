using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDSerialLib.Models.NMEA
{
    public class GLL(): Base(EGPSSystemType.Unknown)
    {
        public GPSCoordinate? GPSCoordinate { get; set; }
        public int? Hour { get; set; }
        public int? Minute { get; set; }
        public int? Second { get; set; }

        public bool Valid { get; set; }

        public static GLL? FromString(string gllString)
        {
            if (string.IsNullOrEmpty(gllString))
                throw new ArgumentNullException(nameof(gllString));

            var strings = gllString.Split(',');

            if (strings.Length < 7)
                throw new FormatException("GLL string format is invalid.");

            try
            {
                var result = new GLL
                {
                    GPSCoordinate = new GPSCoordinate(Helpers.ParseLatitude(strings[1], strings[2]), Helpers.ParseLongitude(strings[3], strings[4]),0),
                    Hour = int.Parse(strings[5].Substring(0,2)),
                    Minute = int.Parse(strings[5].Substring(2,2)),
                    Second = int.Parse(strings[5].Substring(4, 2)),
                    Valid = strings[6] == "A"
                };
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing GLL: {ex.Message} '{gllString}'");
                return null;
            }
        }
    }
}
