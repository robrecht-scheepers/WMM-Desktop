using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Recurring;
using WMM.WPF.Transactions;

namespace WMM.WPF.Balances
{
    public class MonthBalanceViewModel: ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private Balance _totalBalance;
        private RelayCommand _showRecurringTransactionsCommand;
        private RelayCommand<string> _showDetailTransactionsCommand;

        public DateTime Month { get; }

        public Balance TotalBalance
        {
            get => _totalBalance;
            private set => SetValue(ref _totalBalance, value);
        }

        public string Name => Month.ToString("Y");

        public ObservableCollection<AreaBalanceViewModel> AreaBalances { get; }

        public RecurringTransactionsViewModel RecurringTransactionsViewModel { get; }

        public MonthBalanceViewModel(DateTime date, IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            Month = date.FirstDayOfMonth();
            AreaBalances = new ObservableCollection<AreaBalanceViewModel>();
            RecurringTransactionsViewModel = new RecurringTransactionsViewModel(_repository, _windowService, Month);
        }

        public async Task Initialize()
        {
            await LoadAllBalances();

            await RecurringTransactionsViewModel.Initialize();
            RecurringTransactionsViewModel.TransactionModified += 
                async (sender, args) => await RecalculateBalances(args.Transaction.Date, args.Transaction.Category);
            RecurringTransactionsViewModel.MultipleTransactionsModified += 
                async (sender, args) => await LoadAllBalances();
        }

        private async Task LoadAllBalances()
        {
            TotalBalance = await _repository.GetBalance(Month.FirstDayOfMonth(), Month.LastDayOfMonth());
            await LoadAreaBalances();
        }

        private async Task LoadAreaBalances()
        {
            AreaBalances.Clear();
            var balanceDictionary =
                await _repository.GetAreaBalances(Month.FirstDayOfMonth(), Month.LastDayOfMonth());
            foreach (var item in balanceDictionary)
            {
                var areaBalance = new AreaBalanceViewModel(item.Key, item.Value);
                AreaBalances.Add(areaBalance);
                await LoadCategoryBalances(areaBalance);
            }
        }

        private async Task LoadCategoryBalances(AreaBalanceViewModel areaBalanceViewModel)
        {
            areaBalanceViewModel.CategoryBalances.Clear();

            var balanceDictionary =
                await _repository.GetCategoryBalances(Month.FirstDayOfMonth(), Month.LastDayOfMonth(), areaBalanceViewModel.Area);

            foreach (var item in balanceDictionary)
            {
                areaBalanceViewModel.CategoryBalances.Add(new CategoryBalanceViewModel(item.Key, item.Value));
            }
        }

        public async Task RecalculateBalances(DateTime date, string category)
        {
            if(date < Month.FirstDayOfMonth() || date > Month.LastDayOfMonth()) return;

            // recalculate total balance for this month
            TotalBalance = await _repository.GetBalance(Month.FirstDayOfMonth(), Month.LastDayOfMonth());

            var area = _repository.GetAreaForCategory(category);
            var areaBalance = await _repository.GetBalanceForArea(Month.FirstDayOfMonth(), Month.LastDayOfMonth(), area);
            var categoryBalance = await _repository.GetBalanceForCategory(Month.FirstDayOfMonth(),
                Month.LastDayOfMonth(), category);

            var areaBalanceViewModel = AreaBalances.FirstOrDefault(x => x.Area == area);
            if (areaBalanceViewModel == null)
            {
                areaBalanceViewModel = new AreaBalanceViewModel(area, areaBalance);
                AreaBalances.Add(areaBalanceViewModel);
                await LoadCategoryBalances(areaBalanceViewModel);
            }
            else
            {
                areaBalanceViewModel.Balance = areaBalance;
                var categoryBalanceViewModel =
                    areaBalanceViewModel.CategoryBalances.FirstOrDefault(x => x.Name == category);
                if (categoryBalanceViewModel == null)
                {
                    areaBalanceViewModel.CategoryBalances.Add(new CategoryBalanceViewModel(category, categoryBalance));
                }
                else
                {
                    categoryBalanceViewModel.Balance = categoryBalance;
                }
            }
        }

        public RelayCommand ShowRecurringTransactionsCommand =>
            _showRecurringTransactionsCommand ?? (_showRecurringTransactionsCommand = new RelayCommand(ShowRecurringTransactions));
        private void ShowRecurringTransactions()
        {
            _windowService.OpenDialogWindow(RecurringTransactionsViewModel);
        }

        public RelayCommand<string> ShowDetailTransactionsCommand =>
            _showDetailTransactionsCommand ?? (_showDetailTransactionsCommand = new RelayCommand<string>(ShowDetailTransactions));
        private void ShowDetailTransactions(string category)
        {
            RaiseDetailTransactionsRequested(Month.FirstDayOfMonth(), Month.LastDayOfMonth(), category);
        }

        public event DetailTransactionsRequestEventHandler DetailTransactionsRequested;

        private void RaiseDetailTransactionsRequested(DateTime dateFrom, DateTime dateTo, string category)
        {
            DetailTransactionsRequested?.Invoke(this, new DetailTransactionsRequestEventArgs(dateFrom, dateTo, category));
        }

    }
}
