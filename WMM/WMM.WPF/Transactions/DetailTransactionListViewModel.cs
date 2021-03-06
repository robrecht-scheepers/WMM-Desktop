﻿//using System;
//using System.Collections.ObjectModel;
//using System.Threading.Tasks;
//using WMM.Data;
//using WMM.WPF.Helpers;

//namespace WMM.WPF.Transactions
//{
//    public class DetailTransactionListViewModel : TransactionListViewModelBase
//    {
//        private Category _category;
//        private DateTime _dateFrom;
//        private DateTime _dateTo;
//        private bool _loaded = false;

//        public DetailTransactionListViewModel(IRepository repository, IWindowService windowService) 
//            : base(repository, windowService, true)
//        {
//        }

//        public async Task LoadTransactions(DateTime dateFrom, DateTime dateTo, Category category)
//        {
//            _dateFrom = dateFrom;
//            _dateTo = dateTo;
//            _category = category;

//            await Load();
//        }

//        public async Task ReloadTransactions()
//        {
//            if(!_loaded)
//                return;
//            await Load();
//        }

//        public void Clear()
//        {
//            Transactions.Clear();
//            _loaded = false;
//        }

//        private async Task Load()
//        {
//            Transactions = new ObservableCollection<Transaction>(
//                await Repository.GetTransactions(_dateFrom, _dateTo, _category));
//            _loaded = true;
//        }
//    }
//}
