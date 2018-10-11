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
    public class MonthBalanceViewModel: ObservableObject
    {
        public struct CategoryBalance
        {
            public string Category { get; }
            public Balance Balance { get; }
            public CategoryBalance(string category, Balance balance)
            {
                Category = category;
                Balance = balance;
            }
        }

        private readonly DateTime _month;
        private readonly IRepository _repository;

        public Balance TotalBalance { get; private set; }
        public string Name => _month.ToString("Y");

        public ObservableCollection<CategoryBalance> CategoryBalances { get; }

        public MonthBalanceViewModel(DateTime month, IRepository repository)
        {
            _month = month;
            _repository = repository;
            CategoryBalances = new ObservableCollection<CategoryBalance>();
        }

        public async Task Initialize()
        {
            TotalBalance = await _repository.GetBalance(_month.FirstDayOfMonth(), _month.LastDayOfMonth());
            var balanceDictionary =
                await _repository.GetAreaBalances(_month.FirstDayOfMonth(), _month.LastDayOfMonth());
            foreach (var item in balanceDictionary)
            {
                CategoryBalances.Add(new CategoryBalance(item.Key, item.Value));
            }
        }
    }
}
