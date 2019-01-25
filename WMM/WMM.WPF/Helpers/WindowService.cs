using System.Windows;
using WMM.WPF.Categories;
using WMM.WPF.MVVM;
using WMM.WPF.Recurring;
using WMM.WPF.Transactions;

namespace WMM.WPF.Helpers
{
    public class WindowService : IWindowService
    {
        private readonly Window _ownerWindow;

        public WindowService(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }

        public void OpenDialogWindow(ObservableObject dataContext)
        {
            if (dataContext is RecurringTransactionsViewModel)
            {
                var window = new Recurring.RecurringTransactionsWindow {DataContext = dataContext, Owner = _ownerWindow};
                window.Show();
            }
            else if(dataContext is EditTransactionViewModel)
            {
                var window = new EditTransactionWindow{DataContext = dataContext, Owner = _ownerWindow };
                window.Show();
            }
            else if (dataContext is ManageCategoriesViewModel)
            {
                var window = new ManageCategoriesWindow{DataContext = dataContext, Owner = _ownerWindow };
                window.Show();
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
