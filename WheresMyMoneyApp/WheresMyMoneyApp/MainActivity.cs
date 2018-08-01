using System.IO;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using SQLite;
using WheresMyMoneyApp.Adapters;
using WheresMyMoneyApp.Data;

namespace WheresMyMoneyApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const int EditRequestCode = 100;

        private DateGroupType _dateGroupType;

        public static Repository Repository;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // Create dtaabse if not exists
            if(Repository == null)
                Repository = new Repository();
            
            var addButton = FindViewById<Button>(Resource.Id.addNewExpenseButton);
            addButton.Click += delegate { StartActivity(typeof(AddExpenseActivity)); };

            var listView = FindViewById<ListView>(Resource.Id.listViewExpenses);
            RegisterForContextMenu(listView);

            _dateGroupType = DateGroupType.Day;
        }

        private void RefreshList()
        {
            var expenses = Repository.GetExpenses();
            var adapter = new DateGroupedExpenseListAdapter(this, expenses, _dateGroupType);
            var listView = FindViewById<ExpandableListView>(Resource.Id.listViewExpenses);
            listView.SetAdapter(adapter);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RefreshList();
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.listViewExpenses)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                //menu.SetHeaderTitle(_countries[info.Position]);
                var menuItems = Resources.GetStringArray(Resource.Array.listItemContextMenu);
                for (var i = 0; i < menuItems.Length; i++)
                    menu.Add(Menu.None, i, i, menuItems[i]);
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var listView = FindViewById<ListView>(Resource.Id.listViewExpenses);
            var expense = (Expense)listView.Adapter.GetItem(info.Position);

            var menuItemIndex = item.ItemId;
            if (menuItemIndex == 0) // edit
            {
                var intent = new Intent(this, typeof(EditExpenseActivity));
                intent.PutExtra("ExpenseId", expense.Id);

                StartActivityForResult(intent, EditRequestCode);
                return true;
            }

            if (menuItemIndex == 1) // delete
            {
                Repository.DeleteExpense(expense);
                RefreshList();
            }

            return true;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == EditRequestCode && resultCode == Result.Ok)
            {
                RefreshList();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_grouping, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            DateGroupType newGroupType;
            switch (item.ItemId)
            {
                case Resource.Id.group_menu_day:
                    newGroupType = DateGroupType.Day;
                    break;
                case Resource.Id.group_menu_week:
                    newGroupType = DateGroupType.Week;
                    break;
                case Resource.Id.group_menu_month:
                    newGroupType = DateGroupType.Month;
                    break;
                default:
                    return base.OnOptionsItemSelected(item);
            }

            if (newGroupType != _dateGroupType)
            {
                _dateGroupType = newGroupType;
                RefreshList();
            }

            return true;
        }
    }
}



