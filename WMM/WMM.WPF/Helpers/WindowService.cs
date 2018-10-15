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
        }
    }
}
