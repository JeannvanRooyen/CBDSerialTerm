using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CBDSerialLib.Models.NMEA
{
    public class GGA : GPSMessage
    {
        public GGA(string name) : base(name)
        {
            
        }

        public static GGA? FromString(string gaaString)
        {
            if (string.IsNullOrEmpty(gaaString))
                throw new ArgumentNullException(nameof(gaaString));

            var strings = gaaString.Split(',');

            if (strings.Length < 15)
                throw new FormatException("GGA string format is invalid.");

            try
            {
                var stringAlt = gaaString.Replace(strings.First(), "").Replace(strings.Last(), "");

                if (stringAlt.Contains("GN") || stringAlt.Contains("GP") || stringAlt.Contains("GS"))
                {
                    throw new Exception($"String Format Exception: '{gaaString}'");
                }

                var result = new GGA("GAA")
                {
                    Coordinate = new GPSCoordinate(Helpers.ParseLatitude(strings[2], strings[3]), Helpers.ParseLongitude(strings[3], strings[4]), double.Parse(strings[9])),
                    Time = int.Parse(strings[1].Split('.')[0]),
                    GAAFixQuality = (EGAAFixQuality)byte.Parse(strings[6]),
                    NumberOfSatellites = int.Parse(strings[7])
                };
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing GLL: {ex.Message}");
                return null;
            }
        }

        public int? Time { get; set; }

        public GPSCoordinate? Coordinate { get; set; }

        public EGAAFixQuality? GAAFixQuality { get; set; }

        public int? NumberOfSatellites { get; set; }
    }
}
