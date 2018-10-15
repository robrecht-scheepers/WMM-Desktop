using WMM.WPF.MVVM;

namespace WMM.WPF.Helpers
{
    public interface IWindowService
    {
        void OpenDialogWindow(ObservableObject dataContext);
    }
}
