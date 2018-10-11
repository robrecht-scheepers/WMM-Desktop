﻿using System;
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
    public class CategoryBalance : ObservableObject
    {
        private Balance _balance;
        public string Name { get; }

        public Balance Balance
        {
            get => _balance;
            set => SetValue(ref _balance, value);
        }

        public CategoryBalance(string name, Balance balance)
        {
            Name = name;
            Balance = balance;
        }
    }

    public class AreaBalanceViewModel : ObservableObject
    {
        private Balance _balance;
        public string Area { get; }

        public Balance Balance
        {
            get => _balance;
            set => SetValue(ref _balance, value);
        }

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
        private readonly IRepository _repository;
        private Balance _totalBalance;

        public DateTime Month { get; }

        public Balance TotalBalance
        {
            get => _totalBalance;
            private set => SetValue(ref _totalBalance, value);
        }

        public string Name => Month.ToString("Y");

        public ObservableCollection<AreaBalanceViewModel> AreaBalances { get; }

        public MonthBalanceViewModel(DateTime date, IRepository repository)
        {
            Month = date.FirstDayOfMonth();
            _repository = repository;
            AreaBalances = new ObservableCollection<AreaBalanceViewModel>();
        }

        public async Task Initialize()
        {
            TotalBalance = await _repository.GetBalance(Month.FirstDayOfMonth(), Month.LastDayOfMonth());
            await LoadAreaBalances();
        }
        private async Task LoadAreaBalances()
        {
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
                areaBalanceViewModel.CategoryBalances.Add(new CategoryBalance(item.Key, item.Value));
            }
        }

        public async Task RecalculateForTransaction(Transaction transaction)
        {
            if(transaction.Date < Month.FirstDayOfMonth() || transaction.Date > Month.LastDayOfMonth()) return;

            // recalculate total balance for this month
            TotalBalance = await _repository.GetBalance(Month.FirstDayOfMonth(), Month.LastDayOfMonth());

            var area = await _repository.GetArea(transaction.Category);
            var areaBalance = await _repository.GetBalanceForArea(Month.FirstDayOfMonth(), Month.LastDayOfMonth(), area);
            var categoryBalance = await _repository.GetBalanceForCategory(Month.FirstDayOfMonth(),
                Month.LastDayOfMonth(), transaction.Category);

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
                    areaBalanceViewModel.CategoryBalances.FirstOrDefault(x => x.Name == transaction.Category);
                if (categoryBalanceViewModel == null)
                {
                    areaBalanceViewModel.CategoryBalances.Add(new CategoryBalance(transaction.Category, categoryBalance));
                }
                else
                {
                    categoryBalanceViewModel.Balance = categoryBalance;
                }
            }
        }
    }
}
