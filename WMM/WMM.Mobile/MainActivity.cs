using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using WMM.Data;
using Environment = System.Environment;

namespace WMM.Mobile
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private DateGroupType _dateGroupType;
        public static IRepository Repository;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (Repository == null)
                Repository = new DbRepository(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            var addArea = FindViewById<RelativeLayout>(Resource.Id.addTransactionArea);
            addArea.Visibility = ViewStates.Gone;

            var addButton = FindViewById<Button>(Resource.Id.addTransactionButton);
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

            var categoryEditText = FindViewById<EditText>(Resource.Id.addTransactionCategoryEditText);
            var amountEditText = FindViewById<EditText>(Resource.Id.addTransactionAmountEditText);
            var datePicker = FindViewById<DatePicker>(Resource.Id.addTransactionDatePicker);
            var addContinueButton = FindViewById<Button>(Resource.Id.addTransactionConfirmButton);

            addContinueButton.Click += async (sender, args) =>  
            {
                await AddTransaction(categoryEditText.Text, double.Parse(amountEditText.Text), datePicker.DateTime);
                categoryEditText.Text = "";
                amountEditText.Text = "";
                categoryEditText.RequestFocus();
                RefreshList();
            };

            var listView = FindViewById<ListView>(Resource.Id.listViewTransactions);
            RegisterForContextMenu(listView);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

        private async Task AddTransaction(string category, double amount, DateTime date)
        {
            var typeRadioGroup = FindViewById<RadioGroup>(Resource.Id.addTransactionTypeRadioGroup);
            var selectedType = typeRadioGroup.CheckedRadioButtonId;
            if (selectedType == Resource.Id.addTransactionRadioButtonExpense)
                amount *= -1.0;

            await Repository.AddTransaction(date, category, amount, "");
        }

        private async Task RefreshList()
        {
            var expenses = await Repository.GetTransactions();
            var adapter = new DateGroupedExpenseListAdapter(this, expenses, _dateGroupType);
            var listView = FindViewById<ExpandableListView>(Resource.Id.listViewExpenses);
            listView.SetAdapter(adapter);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
	}
}

