using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF.Goals
{
    public class  GoalYearViewModel : ObservableObject
    {
        private readonly Goal _goal;
        private readonly IRepository _repository;


        public GoalYearViewModel(Goal goal, IRepository repository)
        {
            _goal = goal;
            _repository = repository;
        }


    }
}
