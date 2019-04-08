using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF.Balances
{
    public class CategoryBalanceViewModel : ObservableObject
    {
        private Balance _balance;
        public string Name { get; }

        public Balance Balance
        {
            get => _balance;
            set => SetValue(ref _balance, value);
        }

        public CategoryBalanceViewModel(string name, Balance balance)
        {
            Name = name;
            Balance = balance;
        }
    }
}