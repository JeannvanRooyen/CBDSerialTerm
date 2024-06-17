using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace CBDSerialTerm.Graphing
{
    /// <summary>
    /// Interaction logic for GraphComponent.xaml
    /// </summary>
    public partial class GraphComponent : UserControl
    {
        public GraphComponent()
        {
            InitializeComponent();

            
        }

        public void UpdateGraph()
        {
            //plotViewMain.InvalidateVisual();
        }

        public void SetView(PlotModel plotModel)
        {
            plotViewMain.Model = plotModel;
        }

        //public PlotModel GraphPlotModel
        //{
        //    get { return plotViewMain.Model; }
        //    set { plotViewMain.Model = value; }
        //}
    }
}
