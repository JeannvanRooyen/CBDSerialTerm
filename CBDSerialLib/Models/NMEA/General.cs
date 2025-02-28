using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDSerialLib.Models.NMEA
{
    public static class Helpers
    {
        // Helper Parsing Methods
        public static int ParseInt(string input) => int.TryParse(input, out var value) ? value : 0;
        public static double ParseDouble(string input) => double.TryParse(input, out var value) ? value : 0.0;
        public static byte ParseByte(string input) => byte.TryParse(input, out var value) ? value : (byte)0;

        public static double ParseLatitude(string latString, string hemisphere)
        {
            if (double.TryParse(latString, out double parsed))
            {
                return hemisphere == "S" ? -parsed : parsed;
            }
            else
            {
                return 0;
            }
        }

        public static double ParseLongitude(string lonString, string hemisphere)
        {
            if (double.TryParse(lonString, out double parsed))
            {
                return hemisphere == "W" ? -parsed : parsed;
            }
            else
            {
                return 0;
            }
        }
    }

    public abstract class GPSMessage
    {
       public GPSMessage(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

  

    public class GSV
    {
        public GSV() { }

        // GSV constructor with string PRN for satellite ID
        public GSV(int? pRN, int? elevation, int? azimuth, int? sNR = null)
        {
            PRN = pRN;
            Elevation = elevation;
            Azimuth = azimuth;
            SNR = sNR;
        }

        public static GSV FromString(string[] gsvString)
        {
            if (gsvString == null)
                throw new ArgumentNullException(nameof(gsvString));

            if (gsvString.Length < 8)
                throw new FormatException("GSV string format is invalid.");

            try
            {
                int satelliteId = int.Parse(gsvString[4]);
                int elevation = int.Parse(gsvString[5]);
                int azimuth = int.Parse(gsvString[6]);
                int? snr = !string.IsNullOrWhiteSpace(gsvString[7]) ? int.Parse(gsvString[7]) : (int?)null;

                return new GSV(satelliteId, elevation, azimuth, snr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing GSV: {ex.Message}");
                return null; // Or throw an exception, depending on your needs
            }
        }

        public int? PRN { get; set; } // Satellite ID as string
        public int? Elevation { get; set; } = 0;
        public int? Azimuth { get; set; } = 0;
        public double? SNR { get; set; } = null;
    }


    public class GPSCoordinate
    {
        public GPSCoordinate(double latitude, double longitude, double altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public GPSCoordinate()
        {
            Latitude = 0;
            Longitude = 0;
            Altitude = 0;
        }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double Altitude { get; set; }
    }

    public enum EFixQualityIndicator { No_Fix = 0, GPS_SPS_Fix = 1, DGPS_Fix = 2, PPS_Fix = 3, Real_Time_Kinematic = 4, Float_RTK = 5, Estimated_Dead_Reckoning_Fix = 6, Manual_Input_Mode = 7, Simulation_Mode = 9 }

}
