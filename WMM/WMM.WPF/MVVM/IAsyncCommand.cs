using System.Threading.Tasks;

namespace WMM.WPF.MVVM
{
    public interface IAsyncCommand : ICommandEx
    {
        Task ExecuteAsync(object parameter);
    }
}
