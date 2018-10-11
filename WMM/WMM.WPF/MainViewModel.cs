using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF
{
    public class MainViewModel : ObservableObject
    {
        private IRepository _reposistory;

        public MainViewModel(IRepository repository)
        {
            _repository = repository;
        }
    }
}
