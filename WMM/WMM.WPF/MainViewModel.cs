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
        private IRepository _repository;

        public MainViewModel(IRepository repository)
        {
            _repository = repository;
            MonthBalanceViewModels = new ObservableCollection<MonthBalanceViewModel>();
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
        }

        public ObservableCollection<MonthBalanceViewModel> MonthBalanceViewModels { get; }


    }
}
