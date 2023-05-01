using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.Properties;

namespace WMM.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var vCulture = new CultureInfo(Settings.Default.Language);

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
            var mainWindow = new MainWindow();
            var mainViewModel = new  MainViewModel(new DbRepository(Settings.Default.DbDirectory), new WindowService(mainWindow), new CurrencyService(Settings.Default.Currencies));
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
            await mainViewModel.Initialize();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            
        }
    }
}
