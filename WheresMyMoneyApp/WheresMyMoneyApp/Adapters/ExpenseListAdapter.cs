using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WheresMyMoneyApp.Data;

namespace WheresMyMoneyApp.Adapters
{
    public class ExpenseListAdapter : BaseAdapter
    {
        private readonly List<Expense> _expenseList;
        readonly Context _context;

        public ExpenseListAdapter(Context context, List<Expense> expenseList)
        {
            _context = context;
            _expenseList = expenseList.OrderByDescending(x => x.Date).ToList();
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return _expenseList[position];
        }

        public override long GetItemId(int position)
        {
            return _expenseList[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            ExpenseListAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as ExpenseListAdapterViewHolder;

            if (holder == null)
            {
                holder = new ExpenseListAdapterViewHolder();
                var inflater = _context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.expense_list_item_layout, parent, false);
                holder.Amount = view.FindViewById<TextView>(Resource.Id.expenseItemAmount);
                holder.Category = view.FindViewById<TextView>(Resource.Id.expenseItemCategory);
                holder.Date = view.FindViewById<TextView>(Resource.Id.expenseItemDate);

                view.Tag = holder;
            }

            var expense = _expenseList[position]; 

            holder.Amount.Text = expense.Amount.ToString("#.00");
            holder.Category.Text = expense.Category;
            holder.Date.Text = expense.Date.ToString("dd.MM.yy");

            view.SetBackgroundColor(expense.Amount > 0 ? new Color(200, 215, 200) : new Color(215, 215, 215));

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count => _expenseList.Count;
    }

    public class ExpenseListAdapterViewHolder : Java.Lang.Object
    {
        public TextView Amount { get; set; }
        public TextView Category { get; set; }
        public TextView Date { get; set; }
    }
}