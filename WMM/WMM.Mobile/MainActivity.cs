using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using WMM.Mobile.Adapters;
using WMM.Mobile.Data;
using Environment = System.Environment;

namespace WMM.Mobile
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private DateGroupType _dateGroupType;
        public static Repository Repository;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (Repository == null)
            {
                Repository = new Repository(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                var initTask = Repository.Initialize();
                initTask.Wait();
            }

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
                await RefreshList();
            };

            _dateGroupType = DateGroupType.Day;

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
            var transactions = (await Repository.GetTransactions()).ToList();
            var adapter = new DateGroupedExpenseListAdapter(this, transactions, _dateGroupType);
            var listView = FindViewById<ExpandableListView>(Resource.Id.listViewTransactions);
            listView.SetAdapter(adapter);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RefreshList();
        }

        #region Menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
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
                    //StartActivity(typeof(UserSettingsActivity));
                    return true;
                case Resource.Id.upload_action:
                    //UploadDb();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
        #endregion

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        private void UpdateGrouping(DateGroupType groupType)
        {
            if (groupType == _dateGroupType) return;

            _dateGroupType = groupType;
            RefreshList();
        }

        //private async Task UploadDb()
        //{
        //    //TODO: status updates (floating stats thingy like in GMail)
        //    var userName = PreferenceManager.GetDefaultSharedPreferences(this).GetString("pref_user_name", "");
        //    if (string.IsNullOrEmpty(userName))
        //        return;

        //    await Repository.UploadAsync(userName);
        //}
    }
}

