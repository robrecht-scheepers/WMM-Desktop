using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace WMM.WPF.Transactions
{
    public class ScrollTransactionListToDate : Behavior<TransactionListView>
    {
        public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
            "Date", typeof(DateTime), typeof(ScrollTransactionListToDate), new PropertyMetadata(default(DateTime), DateChanged));
        public DateTime Date
        {
            get => (DateTime) GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }
        private static void DateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (ScrollTransactionListToDate) d;

            var transaction = ((TransactionListViewModelBase) me.AssociatedObject.DataContext).Transactions.
                FirstOrDefault(x => x.Date == me.Date);
            if(transaction == null)
                return;

            me.AssociatedObject.DataGrid.ScrollIntoView(transaction);
            me.AssociatedObject.DataGrid.SelectedItem = transaction;
        }
    }
}
