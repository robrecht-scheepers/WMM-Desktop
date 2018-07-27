using System.IO;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using SQLite;
using WheresMyMoneyApp.Data;

namespace WheresMyMoneyApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
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

        }

        protected override void OnResume()
        {
            base.OnResume();

            var expenses = Repository.GetExpenses();
            var adapter = new ArrayAdapter<Expense>(this, Android.Resource.Layout.SimpleListItem1, expenses);
            var listView = FindViewById<ListView>(Resource.Id.listViewExpenses);
            listView.Adapter = adapter;
        }
    }
}

