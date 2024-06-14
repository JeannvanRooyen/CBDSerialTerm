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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CBDSerialTerm
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        DispatcherTimer timer;
        public SplashWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer(TimeSpan.FromSeconds(3), DispatcherPriority.Normal, Timer_Tick , Dispatcher);
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            DialogResult = true;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
