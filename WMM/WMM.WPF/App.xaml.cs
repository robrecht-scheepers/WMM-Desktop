using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using WMM.Data;
using WMM.WPF.Helpers;

namespace WMM.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var vCulture = new CultureInfo("de-DE");

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            base.OnStartup(e);
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainViewModel = new MainViewModel(new DbRepository(@"c:\TEMP\WMM\DB\"), new WindowService());
            var mainWindow = new MainWindow {DataContext = mainViewModel};
            mainWindow.Show();
            await mainViewModel.Initialize();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            
        }
    }
}
