using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF
{
    public class MainViewModel : ObservableObject
    {
        private readonly IRepository _repository;

        public MainViewModel(IRepository repository)
        {
            _repository = repository;
            MonthBalanceViewModels = new ObservableCollection<MonthBalanceViewModel>();
            AddTransactionsViewModel = new AddTransactionsViewModel(_repository);
            AddTransactionsViewModel.TransactionAdded += (s, a) =>
            {
                var newTransaction = a.Transaction;
                var month = newTransaction.Date.FirstDayOfMonth();
                var monthViewModel = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == month);
                // calling without await, because we are in a synchronous event handler
                monthViewModel?.RecalculateForTransaction(newTransaction); 
            };
        }

        public async Task Initialize()
        {
            await _repository.Initialize();

            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now, _repository));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth(), _repository));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth().PreviousMonth(), _repository));
            foreach (var monthBalanceViewModel in MonthBalanceViewModels)
            {
                await monthBalanceViewModel.Initialize();
            }

            await AddTransactionsViewModel.Initialize();
        }

        public ObservableCollection<MonthBalanceViewModel> MonthBalanceViewModels { get; }

        public AddTransactionsViewModel AddTransactionsViewModel { get; }


    }
}
