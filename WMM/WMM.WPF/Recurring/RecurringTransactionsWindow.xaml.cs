using System.Windows;

namespace WMM.WPF.Recurring
{
    /// <summary>
    /// Interaction logic for RecurringTransactionsWindow.xaml
    /// </summary>
    public partial class RecurringTransactionsWindow : Window
    {
        public RecurringTransactionsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
