using System;
using System.Collections.Generic;
using System.IO;
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

namespace CBDSerialTerm
{
    /// <summary>
    /// Interaction logic for EULAWindow.xaml
    /// </summary>
    public partial class EULAWindow : Window
    {
        public EULAWindow()
        {
            InitializeComponent();

            Loaded += EULAWindow_Loaded;
           

            buttonAccept.Click += ButtonAccept_Click;
            buttonDecline.Click += ButtonDecline_Click;
        }

        private void EULAWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var eulaText = File.ReadAllText("EULA.txt");
            textBoxEULA.Text = eulaText;
        }

        private void ButtonDecline_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.EULAAccepted = false;
            Properties.Settings.Default.Save();
            this.DialogResult = false;
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.EULAAccepted = true;
            Properties.Settings.Default.Save();
            this.DialogResult = true;
        }
    }
}
