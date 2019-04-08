using System.Collections.ObjectModel;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF.Balances
{
    public class AreaBalanceViewModel : ObservableObject
    {
        private Balance _balance;
        public string Area { get; }

        public Balance Balance
        {
            get => _balance;
            set => SetValue(ref _balance, value);
        }

        public ObservableCollection<CategoryBalanceViewModel> CategoryBalances { get; }
        public AreaBalanceViewModel(string area, Balance balance)
        {
            Area = area;
            Balance = balance;
            CategoryBalances = new ObservableCollection<CategoryBalanceViewModel>();
        }
    }
}