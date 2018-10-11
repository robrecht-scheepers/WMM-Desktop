using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WMM.Data;

namespace WMM.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainViewModel = new MainViewModel(new DummyRepository());
            var mainWindow = new MainWindow {DataContext = mainViewModel};
            mainWindow.Show();
            await mainViewModel.Initialize();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            
        }
    }
}
