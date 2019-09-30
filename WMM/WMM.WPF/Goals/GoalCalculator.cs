using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;

namespace WMM.WPF.Goals
{
    public class GoalCalculator
    {
        private readonly DateTime _month;
        private readonly IRepository _repository;
        private List<Transaction> _transactions;
        private List<Category> _categories;

        public GoalCalculator(DateTime month, IRepository repository)
        {
            _month = month;
            _repository = repository;
            _categories = _repository.GetCategories();
        }

        public async Task Initialize()
        {
            _transactions = (await _repository.GetTransactions(_month.FirstDayOfMonth(), _month.LastDayOfMonth(), null))
                .ToList();
        }

        public double CalculateCurrentGoalAmount(Goal goal)
        {
            var goalCategories = new List<Category>();

            goalCategories.AddRange(goal.CategoryCriteria);
            goalCategories.AddRange(_categories.Where(x => goal.AreaCriteria.Contains(x.Area)));
            goalCategories.AddRange(_categories.Where(x => goal.CategoryTypeCriteria.Contains(x.CategoryType)));

            return _transactions.Where(x => goalCategories.Contains(x.Category)).Select(x => x.Amount).Sum();
        }
    }
}
