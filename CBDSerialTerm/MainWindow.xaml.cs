using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CBDSerialLib;

namespace CBDSerialTerm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialTerminal? serialTerminal;
        private long bytesReceived = 0;
        private long bytesSent = 0;
        private long linesReceived = 0;

        private List<string> sentCommands = new List<string>();

        public MainWindow()
        {
            Initialized += MainWindow_Initialized;
            Closing += MainWindow_Closing;
           
            InitializeComponent();
        }

        private void MainTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var enableMenus = mainTextBox != null && mainTextBox.Document != null && mainTextBox.Document.Blocks.Count > 0;
            menuItemCopy.IsEnabled = enableMenus;
            buttonCopy.IsEnabled = enableMenus;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (serialTerminal != null && serialTerminal.IsOpen)
            {
                MessageBox.Show("Port is open. Please close the port before closing the application.", "Cannot Close", MessageBoxButton.OK);
                e.Cancel = true;
            }
        }

        private void MainWindow_Initialized(object? sender, EventArgs e)
        {
            UpdatePort();
            UpdateElements();
            mainTextBox.SelectionChanged += MainTextBox_SelectionChanged;
        }

        private void UpdateElements()
        {
            if (buttonToggleState != null && labelPort != null && labelPortState != null)
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.PortName))
                {
                    labelPortName.Content = "none";
                    labelPort.Content = string.Empty;
                    labelPortState.Content = "State: Closed, ";
                    buttonToggleState.IsEnabled = false;
                    textBoxTx.IsReadOnly = true;
                    textBoxTx.Text = "Port is not configured";
                }
                else
                {
                    bool isAvailable = CBDSerialLib.SerialTerminal.PortNames.Contains(Properties.Settings.Default.PortName);

                    labelPortName.Content = Properties.Settings.Default.PortName;

                    labelPort.Content = $"Bdr: {Properties.Settings.Default.Baudrate:#,###,###,##0}, " +
                                       $"Par: {(Parity)Properties.Settings.Default.ParityIndex}, " +
                                       $"Dtbts: {Properties.Settings.Default.DataBitsIndex}, " +
                                       $"Stpbts: {(StopBits)Properties.Settings.Default.StopBitsIndex}, " +
                                       $"Hndshk: {(Handshake)Properties.Settings.Default.HandshakeIndex}, " +
                                       $"RTS: {(Properties.Settings.Default.RTSEnable ? "Y" : "N")}, " +
                                       $"DTR: {(Properties.Settings.Default.DTREnable ? "Y" : "N")}, ";

                    buttonToggleState.IsEnabled = isAvailable;

                    if (serialTerminal != null)
                    {
                        labelPortState.Content = "State: " + (serialTerminal.IsOpen ? "Opened, " : isAvailable ? "Ready" : "Not Available") + ", ";
                        iconState.Kind = serialTerminal.IsOpen ? MaterialDesignThemes.Wpf.PackIconKind.Stop : MaterialDesignThemes.Wpf.PackIconKind.Play;
                        textBoxTx.IsReadOnly = !serialTerminal.IsOpen;
                    }
                    else
                    {
                        labelPortState.Content = "State: Not selected, ";
                        iconState.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                        textBoxTx.IsReadOnly = true;
                        textBoxTx.Text = "Port is not configured";
                    }
                }
            }
            else
            {
                if (buttonToggleState != null)
                {
                    buttonToggleState.IsEnabled = false;
                    iconState.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                }

                if (textBoxTx != null)
                {
                    textBoxTx.IsReadOnly = true;
                }

                if (labelPortState != null)
                {
                    labelPortState.Content = "Closed, ";
                }
            }
        }

        private void buttonPortConfig_Click(object sender, RoutedEventArgs e)
        {
            PortSettingsWindow portSettingsWindow = new PortSettingsWindow();
            portSettingsWindow.Owner = this;
            var result = portSettingsWindow.ShowDialog();

            if (result == true)
            {
                UpdatePort();
            }

        }

        private void UpdatePort()
        {
            serialTerminal = new SerialTerminal(Properties.Settings.Default.PortName,
                    baudRate: Properties.Settings.Default.Baudrate,
                    parity: (Parity)Properties.Settings.Default.ParityIndex,
                    dataBits: Properties.Settings.Default.DataBits,
                    stopBits: (StopBits)Properties.Settings.Default.StopBitsIndex,
                    handshake: (Handshake)Properties.Settings.Default.HandshakeIndex,
                    readTimeout: 500,
                    writeTimeout: 500,
                    rtsEnabled: Properties.Settings.Default.RTSEnable,
                    dtrEnabled: Properties.Settings.Default.DTREnable);

            serialTerminal.DataReceived += SerialTerminal_DataReceived;
            serialTerminal.DataSent += SerialTerminal_DataSent;
            serialTerminal.Exceptioned += SerialTerminal_Exceptioned;

            UpdateElements();
        }

        private void SerialTerminal_Exceptioned(object? sender, Exception e)
        {
            MessageBox.Show(GetWindow(this), e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SerialTerminal_DataSent(object? sender, string e)
        {
            sentCommands.Add(e);
            textBoxTx.Text = null;
            textBoxTx.Focus();
            bytesSent += e.Length;
            labelTX.Content = $"Tx: {bytesSent:#,###,###,###} bytes / {sentCommands.Count} commands";

            if (Properties.Settings.Default.ShowSentCommands)
            {
                mainTextBox.Document.Blocks.Add(new Paragraph());
                mainTextBox.AppendText(e.Replace("\n", ""));
                mainTextBox.Document.Blocks.LastBlock.Foreground = Brushes.Orange;
                mainTextBox.Document.Blocks.Add(new Paragraph());
                scrollViewerRx.ScrollToBottom();
            }
        }

        private void SerialTerminal_DataReceived(object? sender, string e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                mainTextBox.AppendText(e);
                mainTextBox.Document.Blocks.LastBlock.Foreground = Brushes.LightGreen;
                scrollViewerRx.ScrollToBottom();
                bytesReceived += e.Length;

                if (e.Contains("\n") ||  e.Contains("\r"))
                {
                    linesReceived++;
                }

                labelRX.Content = $"Rx: {bytesReceived:#,###,###,###} bytes / {linesReceived} lines";
            }
            );
        }

        private void buttonToggleState_Click(object sender, RoutedEventArgs e)
        {
            if (serialTerminal != null)
            {
                if (!serialTerminal.IsOpen)
                {
                    serialTerminal.Open();

                    if (serialTerminal.IsOpen)
                    {
                        textBoxTx.Text = null;
                        textBoxTx.IsReadOnly = false;
                    }
                    else
                    {
                        textBoxTx.IsReadOnly = true;
                        textBoxTx.Text = "Port failed to open";
                        serialTerminal.Close();
                        MessageBox.Show(GetWindow(this), "Failed to open port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    textBoxTx.IsReadOnly = true;
                    textBoxTx.Text = "Port is closed";
                    serialTerminal.Close();
                }

                UpdateElements();
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            if (mainTextBox.Document != null && mainTextBox.Document.Blocks.Count > 0)
            {
                if (Properties.Settings.Default.ConfirmClearHistory)
                {
                    if (MessageBox.Show("Are you sure you want to clear the history?", "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                mainTextBox.Document.Blocks.Clear();
                bytesReceived = 0;
                linesReceived = 0;
                UpdateElements();

            }
        }
        
        private void mainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (buttonClear != null && mainTextBox != null)
            {
                buttonClear.IsEnabled = mainTextBox.Document != null && mainTextBox.Document.Blocks.Count > 0;
            }
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void TextBoxTx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (serialTerminal != null && serialTerminal.IsOpen)
                {
                    string command = textBoxTx.Text;
                    serialTerminal.Write(command + "\r\n");
                }
            }
        }

        private void menuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected text from the RichTextBox
            TextRange selectedTextRange = new TextRange(mainTextBox.Selection.Start, mainTextBox.Selection.End);

            // Copy the selected text to the clipboard
            Clipboard.SetText(selectedTextRange.Text);
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected text from the RichTextBox
            TextRange selectedTextRange = new TextRange(mainTextBox.Selection.Start, mainTextBox.Selection.End);

            // Copy the selected text to the clipboard
            Clipboard.SetText(selectedTextRange.Text);
        }

        private void menuItemExit_Click(object sender, RoutedEventArgs e)
        {
            if (serialTerminal != null && serialTerminal.IsOpen)
            {
                MessageBox.Show("Port is open. Please close the port before closing the application.", "Cannot Close", MessageBoxButton.OK);
            }
            else
            {
                Close();
            }
        }
    }
}