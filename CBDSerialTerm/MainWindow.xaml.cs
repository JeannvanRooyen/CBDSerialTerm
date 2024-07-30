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
using CBDSerialTerm.Graphing;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;


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
        private Dictionary<int, GraphData> graphs = new Dictionary<int, GraphData>();
        private List<string> sentCommands = new List<string>();

        public MainWindow()
        {
            Initialized += MainWindow_Initialized;
            Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;

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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EULAAccepted == false)
            {
                showEULA();
            }
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
                    buttonPortConfig.IsEnabled = true;
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
                        iconStateB.Kind = serialTerminal.IsOpen ? MaterialDesignThemes.Wpf.PackIconKind.Stop : MaterialDesignThemes.Wpf.PackIconKind.Play;
                        iconStateB.Foreground = serialTerminal.IsOpen ? Brushes.Yellow : Brushes.YellowGreen;
                        iconStateA.Foreground = serialTerminal.IsOpen ? Brushes.Yellow : Brushes.YellowGreen;
                        textBoxTx.IsReadOnly = !serialTerminal.IsOpen;
                        buttonPortConfig.IsEnabled = !serialTerminal.IsOpen;
                    }
                    else
                    {
                        labelPortState.Content = "State: Not selected, ";
                        iconStateB.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                        iconStateB.Foreground = Brushes.GreenYellow;
                        iconStateA.Foreground = Brushes.GreenYellow;
                        textBoxTx.IsReadOnly = true;
                        textBoxTx.Text = "Port is not configured";
                        buttonPortConfig.IsEnabled = true;
                    }
                }
            }
            else
            {
                if (buttonToggleState != null)
                {
                    buttonToggleState.IsEnabled = false;
                    iconStateB.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    iconStateB.Foreground = Brushes.Yellow;
                    iconStateA.Foreground = Brushes.Yellow;
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
                    stopBits: (StopBits)Properties.Settings.Default.StopBits,
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
                AddLine("Tx: " + e, Brushes.DarkOrange,true);
            }
        }

        private void AddLine(string text, Brush foreground, bool paragraphed)
        {
            var lineText = text;
            if (Properties.Settings.Default.ShowTimeStamp)
            {
                lineText = DateTime.Now.ToString("HH:mm:ss.fff") + " " + lineText;
            }

            if (!paragraphed)
            {
                mainTextBox.AppendText(lineText);
                mainTextBox.Document.Blocks.LastBlock.Foreground = Brushes.Silver;
                scrollViewerRx.ScrollToBottom();
            }
            else
            {
                mainTextBox.Document.Blocks.Add(new Paragraph());
                mainTextBox.AppendText(lineText.Replace("\n", ""));
                mainTextBox.Document.Blocks.LastBlock.Foreground = foreground;
                mainTextBox.Document.Blocks.Add(new Paragraph());
                scrollViewerRx.ScrollToBottom();
            }
        }

        private void DisplayGraphs()
        {
            scrollViewerGraph.Content = null;

            try
            {

                GraphComponent graphComponent = new GraphComponent();
                PlotModel plotModel = new PlotModel
                {
                    Title = "Graphs",
                    IsLegendVisible = true,
                    PlotAreaBorderColor = OxyColors.Transparent,
                    Background = OxyColors.LightGray,
                    TextColor = OxyColors.Black,
                    TitleColor = OxyColors.Black
                };

                var xAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColors.Gray,
                    MinorGridlineColor = OxyColors.LightGray,
                    TicklineColor = OxyColors.Black
                };
                plotModel.Axes.Add(xAxis);

                var yAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColors.Gray,
                    MinorGridlineColor = OxyColors.LightGray,
                    TicklineColor = OxyColors.Black
                };
                plotModel.Axes.Add(yAxis);

                var legend = new Legend
                {
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.TopRight,
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendBackground = OxyColors.White,
                    LegendBorder = OxyColors.Black
                };

                plotModel.Legends.Add(legend);

                foreach (var graph in graphs)
                {
                    var tempColor = GraphData.GetRandomColor();
                    var points = graph.Value.Values.Select((v, i) => new DataPoint(i, v));
                    var lineSeries = new LineSeries
                    {
                        Title = graph.Value.Title,
                        Color = graph.Value.Color,
                        MarkerType = MarkerType.Circle,
                        MarkerFill = OxyColors.White,
                        MarkerStroke = graph.Value.MarkerStroke,
                        MarkerStrokeThickness = 1.5
                    };
                    lineSeries.Points.AddRange(points);
                    plotModel.Series.Add(lineSeries);
                }

                graphComponent.SetView(plotModel);

                scrollViewerGraph.Content = graphComponent;
            }
            catch (Exception ex)
            {
                // Inform the user of the specific syntax error
                mainTextBox.Document.Blocks.Add(new Paragraph());
                mainTextBox.AppendText(ex.GetType().ToString() + ": " + ex.Message);
                mainTextBox.Document.Blocks.LastBlock.Foreground = Brushes.OrangeRed;
                mainTextBox.Document.Blocks.Add(new Paragraph());
                scrollViewerRx.ScrollToBottom();
            }
        }


        private void SerialTerminal_DataReceived(object? sender, string e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (e.StartsWith("GRAPH:SERIES:"))
                {
                    try
                    {
                        var parts = e.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        // Check if the command has the minimum required parts
                        if (parts.Length < 5)
                        {
                            throw new FormatException("Invalid command format: not enough parts.");
                        }

                        // Parse the series index
                        if (!int.TryParse(parts[2], out int seriesIndex))
                        {
                            throw new FormatException($"Invalid series index: {parts[2]} is not an integer.");
                        }

                        if (parts.Length >= 5)
                        {
                            string seriesTitle = parts[3];
                            var data = new List<double>();
                            for (int i = 5; i < parts.Length; i++)
                            {
                                if (!double.TryParse(parts[i], out double value))
                                {
                                    throw new FormatException($"Invalid data value: {parts[i]} is not a valid number.");
                                }
                                data.Add(value);
                            }

                            if (!graphs.ContainsKey(seriesIndex))
                            {
                                // Create new series if it doesn't exist
                                graphs[seriesIndex] = new GraphData(seriesTitle);
                            }
                            else if (graphs[seriesIndex].Title != seriesTitle)
                            {
                                // Update the title if it has changed
                                graphs[seriesIndex].Title = seriesTitle;
                            }

                            graphs[seriesIndex].Values.AddRange(data);

                            DisplayGraphs();
                        }
                    }
                    catch (FormatException ex)
                    {
                        // Inform the user of the specific syntax error
                        mainTextBox.Document.Blocks.Add(new Paragraph());
                        mainTextBox.AppendText(ex.GetType().ToString() + ": " + ex.Message);
                        mainTextBox.Document.Blocks.LastBlock.Foreground = Brushes.OrangeRed;
                        mainTextBox.Document.Blocks.Add(new Paragraph());
                        scrollViewerRx.ScrollToBottom();
                    }
                    catch (Exception ex)
                    {
                        // Inform the user of the specific syntax error
                        mainTextBox.Document.Blocks.Add(new Paragraph());
                        mainTextBox.AppendText(ex.GetType().ToString() + ": " + ex.Message);
                        mainTextBox.Document.Blocks.LastBlock.Foreground = Brushes.OrangeRed;
                        mainTextBox.Document.Blocks.Add(new Paragraph());
                        scrollViewerRx.ScrollToBottom();
                    }
                }

                AddLine("Rx: " + e, Brushes.Silver, false);
                bytesReceived += e.Length;

                if (e.Contains("\n") || e.Contains("\r"))
                {
                    linesReceived++;
                }

                labelRX.Content = $"Rx: {bytesReceived:#,###,###,###} bytes / {linesReceived} lines";
            });
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

                scrollViewerGraph.Content = null;

                graphs = new Dictionary<int, GraphData>();

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

        private void buttonTx_Click(object sender, RoutedEventArgs e)
        {
            if (serialTerminal != null && serialTerminal.IsOpen && !string.IsNullOrEmpty(textBoxTx.Text))
            {
                string command = textBoxTx.Text;
                serialTerminal.Write(command + "\r\n");
            }
        }

        private void menuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            var result = settingsWindow.ShowDialog();
        }

        private void menuItemPortSettings_Click(object sender, RoutedEventArgs e)
        {
            PortSettingsWindow portSettingsWindow = new PortSettingsWindow();
            portSettingsWindow.Owner = this;
            var result = portSettingsWindow.ShowDialog();

            if (result == true)
            {
                UpdatePort();
            }
        }

        private void menuItemEULA_Click(object sender, RoutedEventArgs e)
        {
            showEULA();
        }

        private void showEULA()
        {
            try
            {
                EULAWindow eULAWindow = new EULAWindow();
                eULAWindow.Owner = this;
                var result = eULAWindow.ShowDialog();

                if (result == false)
                {
                    try
                    {
                        this.Close();
                    }
                    catch (Exception ex1)
                    {
                        string msg1 = ex1.Message;
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex2)
            {
                var msg2 = ex2.Message;
                Application.Current.Shutdown();
            }
        }
    }
}