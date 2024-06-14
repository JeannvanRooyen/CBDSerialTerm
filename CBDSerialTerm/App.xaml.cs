using System.Configuration;
using System.Data;
using System.Windows;

namespace CBDSerialTerm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var splashScreen = new SplashWindow();
            splashScreen.Show();
        }
    }

}
