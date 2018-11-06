using WMM.WPF.MVVM;

namespace WMM.WPF.Helpers
{
    public interface IWindowService
    {
        void OpenDialogWindow(ObservableObject dataContext);

        void ShowMessage(string message, string caption);
    }
}
