using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Balances;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Recurring;

namespace WMM.WPF
{
    public class MainViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private RelayCommand _showRecurringTransactionsCommand;

        public MainViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            _windowService = windowService;
            MonthBalanceViewModels = new ObservableCollection<MonthBalanceViewModel>();

            AddTransactionsViewModel = new AddTransactionsViewModel(_repository,_windowService);
            AddTransactionsViewModel.TransactionModified+= OnTransactionModified;

            DetailTransactions = new TransactionListViewModelBase(_repository, _windowService,true);
            DetailTransactions.TransactionModified += OnTransactionModified;

            RecurringTransactionsViewModel = new RecurringTransactionsViewModel(_repository, _windowService);
        }
        
        public async Task Initialize()
        {
            await _repository.Initialize();
            await AddTransactionsViewModel.Initialize();
            await RecurringTransactionsViewModel.Initialize();

            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now, _repository, _windowService));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth(), _repository, _windowService));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth().PreviousMonth(), _repository, _windowService));
            foreach (var monthBalanceViewModel in MonthBalanceViewModels)
            {
                await monthBalanceViewModel.Initialize();
                monthBalanceViewModel.DetailTransactionsRequested += async (sender, args) =>
                {
                    await LoadDetailTransactions(args.DateFrom, args.DateTo, args.Category);
                };
            }
        }

        public ObservableCollection<MonthBalanceViewModel> MonthBalanceViewModels { get; }

        public AddTransactionsViewModel AddTransactionsViewModel { get; }

        public RecurringTransactionsViewModel RecurringTransactionsViewModel { get; }

        public TransactionListViewModelBase DetailTransactions { get; }

        private async Task LoadDetailTransactions(DateTime dateFrom, DateTime dateTo, string category)
        {
            DetailTransactions.Show(await _repository.GetTransactions(dateFrom, dateTo, category));
        }

        public RelayCommand ShowRecurringTransactionsCommand => 
            _showRecurringTransactionsCommand ?? (_showRecurringTransactionsCommand = new RelayCommand(ShowRecurringTransactions));

        private void ShowRecurringTransactions()
        {
            _windowService.OpenDialogWindow(RecurringTransactionsViewModel);
        }

        private void OnTransactionModified(object sender, TransactionEventArgs args)
        {
            var newTransaction = args.Transaction;
            var month = newTransaction.Date.FirstDayOfMonth();
            var monthViewModel = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == month);
            monthViewModel?.RecalculateBalances(newTransaction.Date, newTransaction.Category);
        }

    }
}
