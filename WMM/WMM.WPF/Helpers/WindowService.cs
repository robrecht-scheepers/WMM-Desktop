using WMM.WPF.MVVM;

namespace WMM.WPF.Helpers
{
    public class WindowService : IWindowService
    {
        public void OpenDialogWindow(ObservableObject dataContext)
        {
            if (dataContext is RecurringTemplatesViewModel)
            {
                var window = new RecurringTemplatesWindow() {DataContext = dataContext};
                window.ShowDialog();
            }
        }
    }
}
