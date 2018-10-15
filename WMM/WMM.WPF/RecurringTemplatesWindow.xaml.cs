using System.Windows;

namespace WMM.WPF
{
    /// <summary>
    /// Interaction logic for RecurringTransactionsWindow.xaml
    /// </summary>
    public partial class RecurringTemplatesWindow : Window
    {
        public RecurringTemplatesWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
