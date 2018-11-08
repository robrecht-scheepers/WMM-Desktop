using System.Windows;

namespace WMM.WPF.Transactions
{
    /// <summary>
    /// Interaction logic for EditTransactionWindow.xaml
    /// </summary>
    public partial class EditTransactionWindow : Window
    {
        public EditTransactionWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
