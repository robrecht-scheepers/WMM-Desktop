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
    public struct CategoryBalance
    {
        public string Name { get; }
        public Balance Balance { get; }

        public CategoryBalance(string name, Balance balance)
        {
            Name = name;
            Balance = balance;
        }
    }

    public class AreaBalanceViewModel : ObservableObject
    {
        public string Area { get; }
        public Balance Balance { get; }

        public ObservableCollection<CategoryBalance> CategoryBalances { get; }
        public AreaBalanceViewModel(string area, Balance balance)
        {
            Area = area;
            Balance = balance;
            CategoryBalances = new ObservableCollection<CategoryBalance>();
        }
    }

    public class MonthBalanceViewModel: ObservableObject
    {
        private readonly DateTime _month;
        private readonly IRepository _repository;

        public Balance TotalBalance { get; private set; }
        public string Name => _month.ToString("Y");

        public ObservableCollection<AreaBalanceViewModel> AreaBalances { get; }

        public MonthBalanceViewModel(DateTime month, IRepository repository)
        {
            _month = month;
            _repository = repository;
            AreaBalances = new ObservableCollection<AreaBalanceViewModel>();
        }

        public async Task Initialize()
        {
            TotalBalance = await _repository.GetBalance(_month.FirstDayOfMonth(), _month.LastDayOfMonth());
            await LoadAreaBalances();
        }
        private async Task LoadAreaBalances()
        {
            var balanceDictionary =
                await _repository.GetAreaBalances(_month.FirstDayOfMonth(), _month.LastDayOfMonth());
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
                await _repository.GetCategoryBalances(_month.FirstDayOfMonth(), _month.LastDayOfMonth(), areaBalanceViewModel.Area);

            foreach (var item in balanceDictionary)
            {
                areaBalanceViewModel.CategoryBalances.Add(new CategoryBalance(item.Key, item.Value));
            }
        }
    }
}
