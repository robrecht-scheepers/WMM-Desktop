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
        }

        private void RefreshList()
        {
            var expenses = Repository.GetExpenses();
            var adapter = new DateGroupedExpenseListAdapter(this, expenses, DateGroupType.Week);
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
    }
}

