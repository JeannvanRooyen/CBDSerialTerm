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
    public partial class PortSettingsWindow : Window
    {
        public PortSettingsWindow()
        {
            Initialized += PortSettingsWindow_Initialized;
            InitializeComponent();
        }

        private void PortSettingsWindow_Initialized(object? sender, EventArgs e)
        {
            comboBoxBaudrate.ItemsSource = CBDSerialLib.SerialTerminal.BaudRates;
          
            comboBoxParity.ItemsSource = Enum.GetValues(typeof(Parity)).Cast<Parity>();
            comboBoxStopBits.ItemsSource = Enum.GetValues(typeof(StopBits)).Cast<StopBits>();
            comboBoxHandshake.ItemsSource = Enum.GetValues(typeof(Handshake)).Cast<Handshake>();
            comboBoxPort.ItemsSource = CBDSerialLib.SerialTerminal.PortNames;

            comboBoxBaudrate.SelectedIndex = Properties.Settings.Default.BaudrateIndex >= 0 ? Properties.Settings.Default.BaudrateIndex : 0;
            comboBoxDataBits.SelectedIndex = Properties.Settings.Default.DataBits == 0 ? comboBoxDataBits.Items.Count - 1 : Properties.Settings.Default.DataBitsIndex;
            comboBoxParity.SelectedIndex = Properties.Settings.Default.ParityIndex >= 0 ? Properties.Settings.Default.ParityIndex : 0;
            comboBoxStopBits.SelectedIndex = Properties.Settings.Default.StopBitsIndex >= 0 ? Properties.Settings.Default.StopBitsIndex : 0;
            comboBoxHandshake.SelectedIndex = Properties.Settings.Default.HandshakeIndex >= 0 ? Properties.Settings.Default.HandshakeIndex : 0;
            comboBoxPort.SelectedIndex = Properties.Settings.Default.PortNameIndex >= 0 && Properties.Settings.Default.PortNameIndex < comboBoxPort.Items.Count ? Properties.Settings.Default.PortNameIndex : 0;
            checkBoxRTCEnabled.IsChecked = Properties.Settings.Default.RTSEnable;
            checkBoxDTREnabled.IsChecked = Properties.Settings.Default.DTREnable;
        }

        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BaudrateIndex = comboBoxBaudrate.SelectedIndex;
            Properties.Settings.Default.DataBitsIndex = comboBoxDataBits.SelectedIndex;
            Properties.Settings.Default.DataBits = comboBoxDataBits.SelectedIndex >= 0 ? int.Parse(comboBoxDataBits.Text) : 0;
            Properties.Settings.Default.ParityIndex = comboBoxParity.SelectedIndex;
            Properties.Settings.Default.StopBitsIndex = comboBoxStopBits.SelectedIndex;
            Properties.Settings.Default.HandshakeIndex = comboBoxHandshake.SelectedIndex;
            Properties.Settings.Default.PortNameIndex = comboBoxPort.SelectedIndex;
            Properties.Settings.Default.RTSEnable = checkBoxRTCEnabled.IsChecked == true;
            Properties.Settings.Default.DTREnable = checkBoxDTREnabled.IsChecked == true;
            Properties.Settings.Default.PortName = comboBoxPort.Text;

            Properties.Settings.Default.Baudrate = (int)comboBoxBaudrate.SelectedItem;

            Properties.Settings.Default.Save();

            DialogResult = true;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
