using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDSerialLib.Models.NMEA
{
    public enum EGSVType { Unknown, GPGSV, GNGSV, SBGSV, QZGSV, GLGSV, Multi }
    public class GenericGSV:Base
    {
        public EGSVType GSVType { get; set; } = EGSVType.Unknown;

        public string GetGSVName
        {
            get
            {
                switch (GSVType)
                {
                    case EGSVType.GPGSV:
                        return "GPS";
                    case EGSVType.GNGSV:
                        return "Galileo";
                    case EGSVType.SBGSV:
                        return "BeiDou";
                    case EGSVType.QZGSV:
                        return "QZSS (Quasi-Zenith)";
                    case EGSVType.GLGSV:
                        return "GLONASS (USSR)";
                    default: return "Unknown";
                }
            }
        }

        public GenericGSV(EGSVType eSystemType):base( EGPSSystemType.Unknown)
        {
            GSVType = eSystemType;
            Satellites = new List<GSV>();
        }

        public List<GSV> Satellites { get; private set; }

        public static GenericGSV? FromString(EGSVType gSVType, string gngsvString)
        {
            try
            {
                if (string.IsNullOrEmpty(gngsvString)) return null;

                Debug.WriteLine(gngsvString);
                var result = new GenericGSV(gSVType);
                result.GSVType = gSVType;
                var strings = gngsvString.Split(',');

            // Start reading from index 4, as the first four fields are metadata
            for (int i = 4; i < strings.Length - 1; i += 4)
                {
                    try
                    {
                        // Ensure that there are enough fields for a satellite entry
                        if (i + 3 < strings.Length)
                        {
                            var satellite = new GSV
                            {
                                PRN = !String.IsNullOrEmpty(strings[i]) ? int.Parse(strings[i]) : null,
                                Elevation = !String.IsNullOrEmpty(strings[i + 1]) ? int.Parse(strings[i + 1]) : null,
                                Azimuth = !String.IsNullOrEmpty(strings[i + 2]) ? int.Parse(strings[i + 2]) : null,
                                SNR = !String.IsNullOrEmpty(strings[i + 3]) ? double.Parse(strings[i + 3]) : null,
                            };
                            result.Satellites.Add(satellite);
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine($"Parse Exception: {err.Message} '{gngsvString}'");
                    }
                }

                return result;
            }
            catch (Exception err2)
            {
                Debug.WriteLine($"Parse Exception: {err2.Message} '{gngsvString}'");
                return null;
            }
        }
    }
}
