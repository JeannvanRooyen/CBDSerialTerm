using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CBDSerialLib.Models.NMEA
{
    public class GPGLL(): Base(EGPSSystemType.Unknown)
    {
        public GPSCoordinate? GPSCoordinate { get; set; }
        public string? Time { get; set; }
        public char Status { get; set; }

        public static GPGLL? FromString(string gllString)
        {

            if (string.IsNullOrEmpty(gllString)) return null;

            try
            {
                var strings = gllString.Split(',');
                var result = new GPGLL
                {
                    GPSCoordinate = new GPSCoordinate(Helpers.ParseLatitude(strings[1], strings[2]), Helpers.ParseLongitude(strings[3], strings[4]),0),
                    Time = strings[5],
                    Status = strings[6][0]
                };
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing GPGLL: {ex.Message}");
                return null;
            }
        }
    }
}
