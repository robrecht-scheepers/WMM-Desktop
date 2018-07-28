using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using WheresMyMoneyApp.Data;

namespace WheresMyMoneyApp
{
    [Activity(Label = "EditExpenseActivity")]
    public class EditExpenseActivity : Activity
    {
        private Expense _expense;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.edit_expense_layout);

            var id = this.Intent?.GetIntExtra("ExpenseId", -1);
            if (!id.HasValue || id.Value < 0)
            {
                SetResult(Result.Canceled);
                Finish();
            }
            else
            {
                _expense = MainActivity.Repository.GetExpense(id.Value);
                if (_expense == null)
                {
                    Log.Debug("EDIT", $"Expense with id {id} not found.");
                    SetResult(Result.Canceled);
                    Finish();
                }

                var typeRadioGroup = FindViewById<RadioGroup>(Resource.Id.editExpenseTypeRadioGroup);
                var categoryEditText = FindViewById<EditText>(Resource.Id.editExpenseCategoryEditText);
                var amountEditText = FindViewById<EditText>(Resource.Id.editExpenseAmountEditText);
                var datePicker = FindViewById<DatePicker>(Resource.Id.editExpenseDatePicker);
                var saveButon = FindViewById<Button>(Resource.Id.editExpenseSaveButton);

                typeRadioGroup.Check(_expense.Amount < 0
                    ? Resource.Id.editExpenseRadioButtonExpense
                    : Resource.Id.editExpenseRadioButtonIncome);
                categoryEditText.Text = _expense.Category;
                amountEditText.Text = Math.Abs(_expense.Amount).ToString("0.00");
                datePicker.DateTime = _expense.Date;

                saveButon.Click += delegate
                {
                    _expense.Category = categoryEditText.Text;
                    _expense.Date = datePicker.DateTime;

                    var amount = double.Parse(amountEditText.Text);
                    if (typeRadioGroup.CheckedRadioButtonId == Resource.Id.editExpenseRadioButtonExpense)
                        amount *= -1.0;
                    _expense.Amount = amount;

                    MainActivity.Repository.SaveExpense(_expense);

                    SetResult(Result.Ok);
                    Finish();
                };
            }

        }
    }
}