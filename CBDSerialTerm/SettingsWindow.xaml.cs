using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Printing;
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
using CBDSerialLib;

namespace CBDSerialTerm
{
    /// <summary>
    /// Interaction logic for PortSettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            Initialized += SettingsWindow_Initialized;
            InitializeComponent();
        }

        private void SettingsWindow_Initialized(object? sender, EventArgs e)
        {
            checkBoxShowTimestamp.IsChecked = Properties.Settings.Default.ShowTimeStamp;
            checkBoxShowGraph.IsChecked = Properties.Settings.Default.ShowGraph;
            checkBoxShowSentCommands.IsChecked = Properties.Settings.Default.ShowSentCommands;
        }

        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
          
            Properties.Settings.Default.ShowTimeStamp = checkBoxShowTimestamp.IsChecked == true;
            Properties.Settings.Default.ShowGraph = checkBoxShowGraph.IsChecked == true;
            Properties.Settings.Default.ShowSentCommands = checkBoxShowSentCommands.IsChecked == true;

            Properties.Settings.Default.Save();

            DialogResult = true;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
