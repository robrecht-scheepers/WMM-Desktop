using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    public class SearchConfiguration
    {
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private string _area;
        private string _categoryName;
        private string _comments;
        private double _amount;
        private bool _transactionDirectionPositive;

        public SearchConfiguration()
        {
            Parameters = SearchParameter.None;
        }

        public SearchParameter Parameters { get; set; }

        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                Parameters |= ; SearchParameter.Date
            }
        }

        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                Parameters |= SearchParameter.Date;
            }
        }

        public string Area
        {
            get => _area;
            set
            {
                _area = value;
                Parameters |= SearchParameter.Area;
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                Parameters |= SearchParameter.Category;
            }
        }

        public string Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                Parameters |= SearchParameter.Comments;
            }
        }

        public double Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                Parameters |= SearchParameter.Amount;
            }
        }

        public bool TransactionDirectionPositive
        {
            get => _transactionDirectionPositive;
            set
            {
                _transactionDirectionPositive = value;
                Parameters |= SearchParameter.Direction;
            }
        }
    }
}
