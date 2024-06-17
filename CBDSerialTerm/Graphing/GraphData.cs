using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBDSerialTerm.Graphing
{
    public class GraphData
    {
        public string Title { get; set; }
        public List<double> Values { get; set; }

        public GraphData(string title)
        {
            Title = title;
            Values = new List<double>();
            Color = GetRandomColor();
            MarkerStroke = Color;
        }

        public OxyColor Color { get; set; }
        public OxyColor MarkerStroke { get; set; }

        public static OxyColor GetRandomColor()
        {
            Random r = new Random();
            return Colors[r.Next(Colors.Count)];
        }

        public static readonly List<OxyColor> Colors = new List<OxyColor>
        {
            OxyColors.Red,
            OxyColors.Green,
            OxyColors.Blue,
            OxyColors.Orange,
            OxyColors.Purple,
            OxyColors.Teal,
            OxyColors.Brown,
            OxyColors.Magenta,
            OxyColors.Cyan,
            OxyColors.Lime
        };
    }

    
}
