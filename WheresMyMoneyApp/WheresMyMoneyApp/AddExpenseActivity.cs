using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WheresMyMoneyApp.Data;

namespace WheresMyMoneyApp
{
    [Activity(Label = "AddExpenseActivity")]
    public class AddExpenseActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.add_expense_layout);

            var categoryEditText = FindViewById<EditText>(Resource.Id.newExpenseCategoryEditText);
            var amountEditText = FindViewById<EditText>(Resource.Id.newExpenseAmountEditText);
            var datePicker = FindViewById<DatePicker>(Resource.Id.newExpenseDatePicker);
            var addContinueButon = FindViewById<Button>(Resource.Id.newExpenseAddContinueButton);
            var addBackButton = FindViewById<Button>(Resource.Id.newExpenseAddBackButton);

            addContinueButon.Click += delegate
            {
                AddExpense(categoryEditText.Text, double.Parse(amountEditText.Text), datePicker.DateTime);
                categoryEditText.Text = "";
                amountEditText.Text = "";
                categoryEditText.RequestFocus();
            };

            addBackButton.Click += delegate
            {
                AddExpense(categoryEditText.Text, double.Parse(amountEditText.Text), datePicker.DateTime);
                Finish();
            };

        }

        private void AddExpense(string category, double amount, DateTime date)
        {
            var typeRadioGroup = FindViewById<RadioGroup>(Resource.Id.newExpenseTypeRadioGroup);
            var selectedType = typeRadioGroup.CheckedRadioButtonId;
            if (selectedType == Resource.Id.newExpenseRadioButtonIncome)
                amount *= -1.0;

            MainActivity.Repository.SaveExpense(new Expense(category, amount, date));
        }
    }
}