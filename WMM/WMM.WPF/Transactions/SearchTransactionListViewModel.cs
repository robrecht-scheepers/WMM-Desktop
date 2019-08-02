using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Resources;

namespace WMM.WPF.Transactions
{
    public class SearchTransactionListViewModel : TransactionListViewModelBase
    {
        public enum AreaCategorySelectionType { Area, Category, CategoryType}

        public class AreaCategorySelectionItem
        {
            //public bool IsArea { get; set; }
            public AreaCategorySelectionType SelectionType { get; set; }
            public string Name { get; set; }
            public bool IsSelectable { get; set; }

            public AreaCategorySelectionItem(string name, AreaCategorySelectionType selectionType, bool isSelectable = true)
            {
                SelectionType = selectionType;
                Name = name;
                IsSelectable = isSelectable;
            }
        }

        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private double? _amount;
        private string _comments;
        private string _selectedSign;
        private ObservableCollection<AreaCategorySelectionItem> _areaCategoryList;
        private AreaCategorySelectionItem _selectedAreaCategoryItem;
        private AsyncRelayCommand _searchCommand;
        private RelayCommand _resetCommand;
        private Balance _balance;
        private RelayCommand _excelExportCommand;
        private string _selectedRecurringOption;

        public SearchTransactionListViewModel(IRepository repository, IWindowService windowService) : base(repository, windowService, true)
        {
            InitializeRecurringOptionList();
            Repository.CategoriesUpdated += (s, a) => InitializeAreaCategoryList();
        }

        public void Initialize()
        {
            InitializeAreaCategoryList();
            SelectedSign = "";
        }

        public Balance Balance
        {
            get => _balance;
            set => SetValue(ref _balance, value);
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set => SetValue(ref _dateFrom, value);
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set => SetValue(ref _dateTo, value);
        }

        public double? Amount
        {
            get => _amount;
            set => SetValue(ref _amount, value);
        }

        public AreaCategorySelectionItem SelectedAreaCategoryItem
        {
            get => _selectedAreaCategoryItem;
            set => SetValue(ref _selectedAreaCategoryItem, value);
        }

        public string Comments
        {
            get => _comments;
            set => SetValue(ref _comments, value);
        }

        public List<string> Signs => new List<string> {"", "+", "-"};

        public string SelectedSign
        {
            get => _selectedSign;
            set => SetValue(ref _selectedSign, value);
        }

        public ObservableCollection<AreaCategorySelectionItem> AreaCategoryList
        {
            get => _areaCategoryList;
            set => SetValue(ref _areaCategoryList, value);
        }

        public Dictionary<string, bool?> RecurringOptionList { get; set; }

        public string SelectedRecurringOption
        {
            get => _selectedRecurringOption;
            set => SetValue(ref _selectedRecurringOption, value);
        }

        public AsyncRelayCommand SearchCommand => _searchCommand ?? (_searchCommand = new AsyncRelayCommand(Search));

        private async Task Search()
        {
            var searchConfiguration = new SearchConfiguration();

            if (DateFrom.HasValue)
            {
                searchConfiguration.DateFrom = DateFrom.Value;
                searchConfiguration.DateTo = DateTo ?? DateFrom.Value;
            }

            if (!string.IsNullOrWhiteSpace(SelectedAreaCategoryItem?.Name))
            {
                switch (SelectedAreaCategoryItem.SelectionType)
                {
                    case AreaCategorySelectionType.Area:
                        searchConfiguration.Area = SelectedAreaCategoryItem.Name;
                        break;
                    case AreaCategorySelectionType.Category:
                        searchConfiguration.CategoryName = SelectedAreaCategoryItem.Name;
                        break;
                    case AreaCategorySelectionType.CategoryType:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!string.IsNullOrWhiteSpace(Comments))
            {
                searchConfiguration.Comments = Comments;
            }

            if (!string.IsNullOrEmpty(SelectedSign))
            {
                if (Amount.HasValue)
                {
                    searchConfiguration.Amount = SelectedSign == "+"
                        ? Amount.Value
                        : -1.0 * Amount.Value;
                }
                else
                {
                    searchConfiguration.TransactionDirectionPositive = (SelectedSign == "+");
                }
            }

            if (RecurringOptionList[SelectedRecurringOption].HasValue)
            {
                searchConfiguration.Recurring = RecurringOptionList[SelectedRecurringOption].Value;
            }

            Transactions = new ObservableCollection<Transaction>((await Repository.GetTransactions(searchConfiguration)).OrderByDescending(x => x.Date));
            CalculateBalance();
        }

        public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));
        public void Reset()
        {
            DateFrom = null;
            DateTo = null;
            SelectedAreaCategoryItem = null;
            SelectedSign = "";
            Amount = null;
            Comments = "";
            ResetRecurringOption();

            Transactions.Clear();
            CalculateBalance();
        }

        private void InitializeAreaCategoryList()
        {
            AreaCategoryList = new ObservableCollection<AreaCategorySelectionItem>
            {
                new AreaCategorySelectionItem("", AreaCategorySelectionType.Area),
                new AreaCategorySelectionItem($"--- {Captions.Area} ---", AreaCategorySelectionType.Area, false)
            };
            foreach (var area in Repository.GetAreas().OrderBy(x => x))
            {
                AreaCategoryList.Add(new AreaCategorySelectionItem(area, AreaCategorySelectionType.Area));
            }
            AreaCategoryList.Add(new AreaCategorySelectionItem($"--- {Captions.Category} ---", AreaCategorySelectionType.Category,false));
            foreach (var category in Repository.GetCategoryNames().OrderBy(x => x))
            {
                AreaCategoryList.Add(new AreaCategorySelectionItem(category,AreaCategorySelectionType.Category));
            }
        }

        private void InitializeRecurringOptionList()
        {
            RecurringOptionList = new Dictionary<string, bool?>
            {
                {Captions.All, null},
                {Captions.OnlyRecurring, true},
                {Captions.NoRecurring, false}
            };
            ResetRecurringOption();
        }

        private void ResetRecurringOption()
        {
            SelectedRecurringOption = RecurringOptionList.First(x => x.Value == null).Key;
        }

        public async Task SearchForDatesAndCategory(DateTime dateFrom, DateTime dateTo, string category)
        {
            Reset();

            DateFrom = dateFrom;
            DateTo = dateTo;
            SelectedAreaCategoryItem = AreaCategoryList.FirstOrDefault(
                x => x.SelectionType == AreaCategorySelectionType.Category && x.IsSelectable && x.Name == category);

            await Search();
        }

        public void CalculateBalance()
        {
            Balance = new Balance(
                Transactions.Select(x => x.Amount).Where(x => x > 0).Sum(),
                Transactions.Select(x => x.Amount).Where(x => x < 0).Sum());
        }

        protected override void RepositoryOnTransactionUpdated(object sender, TransactionUpdateEventArgs args)
        {
            base.RepositoryOnTransactionUpdated(sender, args);
            CalculateBalance();
        }

        protected override void RepositoryOnTransactionDeleted(object sender, TransactionEventArgs args)
        {
            base.RepositoryOnTransactionDeleted(sender, args);
            CalculateBalance();
        }

        public RelayCommand ExcelExportCommand =>
            _excelExportCommand ?? (_excelExportCommand = new RelayCommand(ExcelExport, CanExecuteExcelExport));

        private bool CanExecuteExcelExport()
        {
            return Transactions.Any();
        }

        private void ExcelExport()
        {
            try
            {
                ExcelHelper.OpenInExcel(Transactions);
            }
            catch (Exception e)
            {
                WindowService.ShowMessage(string.Format(Captions.ExcelError, e.Message),Captions.Error);
            }
        }
    }
}
