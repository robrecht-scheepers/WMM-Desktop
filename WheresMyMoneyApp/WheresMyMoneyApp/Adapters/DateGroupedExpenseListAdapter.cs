using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WheresMyMoneyApp.Data;
using Object = Java.Lang.Object;

namespace WheresMyMoneyApp.Adapters
{
    public class DateGroupedExpenseListAdapter : BaseExpandableListAdapter
    {
        Context _context;
        private List<ExpenseDateGroup> _groups;
        private DateGroupType _groupType;

        public DateGroupedExpenseListAdapter(Context context, List<Expense> expenses, DateGroupType groupType)
        {
            _context = context;
            _groupType = groupType;
            CreateGroups(expenses);
        }

        private void CreateGroups(List<Expense> expenseList)
        {
            _groups = new List<ExpenseDateGroup>();
            ExpenseDateGroup currentGroup = null;

            foreach (var expense in expenseList.OrderByDescending(x => x.Date))
            {
                if ( currentGroup == null || expense.Date < currentGroup.StartDate)
                {
                    currentGroup = CreateGroup(expense.Date, _groupType);
                    _groups.Add(currentGroup);
                }
                currentGroup.Expenses.Add(expense);
            }
        }

        private ExpenseDateGroup CreateGroup(DateTime date, DateGroupType groupType)
        {
            // create the group for the defined type that contains the date
            switch (groupType)
            {
                case DateGroupType.Day:
                    return new ExpenseDateGroup
                    {
                        StartDate =  date.Date,
                        EndDate = date.Date
                    };
                case DateGroupType.Week:
                    int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var startDay = date.AddDays(-1 * diff).Date;
                    return new ExpenseDateGroup
                    {
                        StartDate = startDay,
                        EndDate = startDay.AddDays(6).Date
                    };
                case DateGroupType.Month:
                    return new ExpenseDateGroup
                    {
                        StartDate = new DateTime(date.Year, date.Month, 1),
                        EndDate = date.AddMonths(1).AddDays(-1).Date
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(groupType), groupType, null);
            }
        }

        public override Object GetChild(int groupPosition, int childPosition)
        {
            return _groups[groupPosition].Expenses[childPosition];
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return _groups[groupPosition].Expenses[childPosition].Id;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return _groups[groupPosition].Expenses.Count;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            var view = convertView;
            DateGroupedExpenseListAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as DateGroupedExpenseListAdapterViewHolder;

            if (holder == null)
            {
                holder = new DateGroupedExpenseListAdapterViewHolder();
                var inflater = _context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.expense_list_item_layout, parent, false);
                holder.Amount = view.FindViewById<TextView>(Resource.Id.expenseItemAmount);
                holder.Category = view.FindViewById<TextView>(Resource.Id.expenseItemCategory);
                holder.Date = view.FindViewById<TextView>(Resource.Id.expenseItemDate);

                view.Tag = holder;
            }

            var expense = (Expense)GetChild(groupPosition, childPosition);

            holder.Amount.Text = expense.Amount.ToString("#.00");
            holder.Category.Text = expense.Category;
            holder.Date.Text = expense.Date.ToString("dd.MM.yy");

            view.SetBackgroundColor(expense.Amount > 0 ? new Color(200, 215, 200) : new Color(215, 215, 215));

            return view;
        }

        public override Object GetGroup(int groupPosition)
        {
            return _groups[groupPosition];
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            var view = convertView;
            DateGroupedExpenseListAdapterGroupViewHolder holder = null;

            if (view != null)
                holder = view.Tag as DateGroupedExpenseListAdapterGroupViewHolder;

            if (holder == null)
            {
                holder = new DateGroupedExpenseListAdapterGroupViewHolder();
                var inflater = _context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.date_group_item_layout, parent, false);
                holder.Start = view.FindViewById<TextView>(Resource.Id.dateGroupHeaderStart);
                holder.End = view.FindViewById<TextView>(Resource.Id.dateGroupHeaderEnd);
                holder.Balance = view.FindViewById<TextView>(Resource.Id.dateGroupHeaderBalance);

                view.Tag = holder;
            }

            var group = (ExpenseDateGroup)GetGroup(groupPosition);

            holder.Start.Text = group.StartDate.ToString("dd.MM.yy");
            holder.End.Text = group.EndDate.ToString("dd.MM.yy");
            holder.Balance.Text = group.Balance.ToString("0.##");

            view.SetBackgroundColor(group.Balance > 0 ? new Color(240, 255, 240) : new Color(255, 255, 255));

            return view;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }

        public override int GroupCount => _groups.Count;
        public override bool HasStableIds { get; }
    }

    class DateGroupedExpenseListAdapterViewHolder : Java.Lang.Object
    {
        public TextView Amount { get; set; }
        public TextView Category { get; set; }
        public TextView Date { get; set; }
    }
    

    class DateGroupedExpenseListAdapterGroupViewHolder : Java.Lang.Object
    {
        public TextView Start { get; set; }
        public TextView End { get; set; }
        public TextView Balance { get; set; }
    }

    public enum DateGroupType { Day, Week, Month}

    class ExpenseDateGroup : Java.Lang.Object
    {
        public ExpenseDateGroup()
        {
            Expenses = new List<Expense>();
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double Balance => Expenses.Select(x => x.Amount).Sum();
        public List<Expense> Expenses { get; set; }
    }
}