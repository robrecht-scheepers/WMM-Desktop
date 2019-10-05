using System.Windows;
using System.Windows.Input;
using WMM.WPF.Categories;
using WMM.WPF.Forecast;
using WMM.WPF.Goals;
using WMM.WPF.MVVM;
using WMM.WPF.Recurring;
using WMM.WPF.Resources;
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
                var window = new RecurringTransactionsWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
            else if(dataContext is EditTransactionViewModel)
            {
                var window = new EditTransactionWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
            else if (dataContext is ManageCategoriesViewModel)
            {
                var window = new ManageCategoriesWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
            else if (dataContext is ForecastViewModel)
            {
                var window = new ForecastWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
            else if (dataContext is SelectDeleteCategoryFallbackViewModel)
            {
                var window = new SelectDeleteCategoryFallbackWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
            else if (dataContext is ManageGoalsViewModel)
            {
                var window = new ManageGoalsWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
            else if (dataContext is MonthGoalListViewModel)
            {
                var window = new MonthGoalDetailsWindow
                {
                    DataContext = dataContext,
                    Owner = _ownerWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                window.ShowDialog();
            }
        }

        public void ShowMessage(string message, string caption)
        {
            MessageBox.Show(message, caption);
        }

        public bool AskConfirmation(string message)
        {
            var result = MessageBox.Show(message, Captions.Warning, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            return result == MessageBoxResult.Yes;
        }
    }
}
