using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CBDSerialLib.Models.NMEA
{
    public enum EGPSSystemType { Unknown, GPS, Multiple }
    public enum EGAAFixQuality { Invalid = 0, GPS = 1, DGPS = 2 }
    public abstract class Base
    {
        public Base(EGPSSystemType sType)
        {
            GPSSystemType = sType;
            Properties = this.GetType().GetProperties();
            PropertyNamesAndValues = new Dictionary<string, object?>();
        }

        public EGPSSystemType GPSSystemType { get; set; } = EGPSSystemType.Unknown;

        public PropertyInfo[] Properties { get; private set; }

        public Dictionary<string, object?> PropertyNamesAndValues { get; private set; }

        public void InitProps(Type typed)
        {
            Type type = typed;
            Properties = type.GetProperties();
            PropertyNamesAndValues = new Dictionary<string, object?>();

            foreach (var property in Properties)
            {
                object? value = property.GetValue(this);
                PropertyNamesAndValues[property.Name] = value;
            }
        }
    }
}
