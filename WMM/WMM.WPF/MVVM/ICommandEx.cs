using System.Windows.Input;

namespace WMM.WPF.MVVM
{
    public interface ICommandEx : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
