using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Media.Capture;

namespace CBDSerialLib.Models.NMEA
{

    public class GenericGGA:Base
    {
        public GenericGGA():base(EGPSSystemType.Unknown)
        {
            
            InitProps(GetType());
        }

       
        

        public GenericGGA(int raw_time, GPSCoordinate? gPSCoordinate, EFixQualityIndicator fixQuality, int numberOfSatellites,
                     double horizontalDilutionOfPrecision, double altitude, double geoidalSeparation):base(EGPSSystemType.Unknown)
        {
            this.raw_time = raw_time;
            GPSCoordinate = gPSCoordinate;
            FixQuality = fixQuality;
            NumberOfSatellites = numberOfSatellites;
            HorizontalDilutionOfPrecision = horizontalDilutionOfPrecision;
            Altitude = altitude;
            GeoidalSeparation = geoidalSeparation;
            InitProps(GetType());
        }

        

        // Factory method to parse from a string
        public static GenericGGA? FromString(string ggaString)
        {
            if (string.IsNullOrEmpty(ggaString))
                throw new ArgumentNullException(nameof(ggaString));

            var strings = ggaString.Split(',');

            if (strings.Length < 15)
                throw new FormatException("GGA string format is invalid.");

            try
            {
                var result = new GenericGGA();
                result.raw_time = Helpers.ParseInt(strings[1]); // Time of fix in hhmmss.ss
                result.GPSCoordinate = new GPSCoordinate(
                    Helpers.ParseLatitude(strings[2], strings[3]),
                    Helpers.ParseLongitude(strings[4], strings[5]),
                    0
                );
                result.FixQuality = (EFixQualityIndicator)Helpers.ParseByte(strings[6]);
                result.NumberOfSatellites = Helpers.ParseInt(strings[7]);
                result.HorizontalDilutionOfPrecision = Helpers.ParseDouble(strings[8]);
                result.Altitude = Helpers.ParseDouble(strings[9]);
                result.GeoidalSeparation = Helpers.ParseDouble(strings[11]);

                result.InitProps(result.GetType());
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing GGA: {ex.Message}");
                return null; // Optionally handle the failure case better
            }
        }

      

        // Properties matching the GPGGA format
        public const string Name = "GGA - Global Positioning System Fixed Data";

        public int? raw_time { get; set; }
        public GPSCoordinate? GPSCoordinate { get; set; }
        public EFixQualityIndicator? FixQuality { get; set; }
        public int? NumberOfSatellites { get; set; }
        public double? HorizontalDilutionOfPrecision { get; set; }
        public double? Altitude { get; set; }
        public double? GeoidalSeparation { get; set; }
    }
}
