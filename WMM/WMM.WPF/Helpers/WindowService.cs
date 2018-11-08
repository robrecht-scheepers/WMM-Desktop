using System.Windows;
using WMM.WPF.Categories;
using WMM.WPF.MVVM;
using WMM.WPF.Recurring;

namespace WMM.WPF.Helpers
{
    public class WindowService : IWindowService
    {
        public void OpenDialogWindow(ObservableObject dataContext)
        {
            if (dataContext is RecurringTransactionsViewModel)
            {
                var window = new Recurring.RecurringTransactionsWindow() {DataContext = dataContext};
                window.ShowDialog();
            }
            else if(dataContext is EditTransactionViewModel)
            {
                var window = new EditTransactionWindow{DataContext = dataContext};
                window.ShowDialog();
            }
            else if (dataContext is ManageCategoriesViewModel)
            {
                var window = new ManageCategoriesWindow{DataContext = dataContext};
                window.ShowDialog();
            }
        }

        public void ShowMessage(string message, string caption)
        {
            MessageBox.Show(message, caption);
        }

        public bool AskConfirmation(string message)
        {
            var result = MessageBox.Show(message, "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            return result == MessageBoxResult.Yes;
        }
    }
}
