using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Preferences;
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

            if(Repository == null)
                Repository = new Repository();

            var addArea = FindViewById<RelativeLayout>(Resource.Id.addExpenseArea);
            addArea.Visibility = ViewStates.Gone;

            var addButton = FindViewById<Button>(Resource.Id.addNewExpenseButton);
            addButton.Click += delegate
            {
                switch (addArea.Visibility)
                {
                    case ViewStates.Gone:
                    case ViewStates.Invisible:
                        addArea.Visibility = ViewStates.Visible;
                        break;
                    case ViewStates.Visible:
                        addArea.Visibility = ViewStates.Gone;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            var categoryEditText = FindViewById<EditText>(Resource.Id.newExpenseCategoryEditText);
            var amountEditText = FindViewById<EditText>(Resource.Id.newExpenseAmountEditText);
            var datePicker = FindViewById<DatePicker>(Resource.Id.newExpenseDatePicker);
            var addContinueButon = FindViewById<Button>(Resource.Id.newExpenseAddContinueButton);
            
            addContinueButon.Click += delegate
            {
                AddExpense(categoryEditText.Text, double.Parse(amountEditText.Text), datePicker.DateTime);
                categoryEditText.Text = "";
                amountEditText.Text = "";
                categoryEditText.RequestFocus();
                RefreshList();
            };

            var listView = FindViewById<ListView>(Resource.Id.listViewExpenses);
            RegisterForContextMenu(listView);

            _dateGroupType = DateGroupType.Day;
        }

        private void AddExpense(string category, double amount, DateTime date)
        {
            var typeRadioGroup = FindViewById<RadioGroup>(Resource.Id.newExpenseTypeRadioGroup);
            var selectedType = typeRadioGroup.CheckedRadioButtonId;
            if (selectedType == Resource.Id.newExpenseRadioButtonExpense)
                amount *= -1.0;

            Repository.SaveExpense(new Expense(category, amount, date));
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

        #region context menu

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

        #endregion
        
        #region Menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.group_menu_day:
                    UpdateGrouping(DateGroupType.Day);
                    return true;
                case Resource.Id.group_menu_week:
                    UpdateGrouping(DateGroupType.Week);
                    return true;
                case Resource.Id.group_menu_month:
                    UpdateGrouping(DateGroupType.Month);
                    return true;
                case Resource.Id.user_action:
                    StartActivity(typeof(UserSettingsActivity));
                    return true;
                case Resource.Id.upload_action:
                    UploadDb();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
        #endregion

        private void UpdateGrouping(DateGroupType groupType)
        {
            if (groupType == _dateGroupType) return;

            _dateGroupType = groupType;
            RefreshList();
        }

        private async Task UploadDb()
        {
            //TODO: status updates (floating stats thingy like in GMail)
            var userName = PreferenceManager.GetDefaultSharedPreferences(this).GetString("pref_user_name", "");
            if(string.IsNullOrEmpty(userName))
                return;

            await Repository.UploadAsync(userName);
        }

    }
}



