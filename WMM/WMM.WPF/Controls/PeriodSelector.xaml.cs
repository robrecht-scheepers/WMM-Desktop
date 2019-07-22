using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WMM.WPF.Helpers;
using WMM.WPF.Resources;

namespace WMM.WPF.Controls
{
    /// <summary>
    /// Interaction logic for PeriodSelector.xaml
    /// </summary>
    public partial class PeriodSelector : UserControl
    {
        private Dictionary<string, Tuple<DateTime, DateTime>> _shortcuts;
        private bool _applyingShortcut;

        public PeriodSelector()
        {
            InitializeComponent();
            InitializeShortcutOptions();
        }

        public static readonly DependencyProperty DateFromProperty = DependencyProperty.Register(
            "DateFrom", typeof(DateTime?), typeof(PeriodSelector), new PropertyMetadata(default(DateTime?),DateFromChanged));

        private static void DateFromChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PeriodSelector me)) return;

            if (me.DateFromSelection.SelectedDate == me.DateFrom)
                return;

            me.DateFromSelection.SelectedDate = me.DateFrom;
        }

        public DateTime? DateFrom
        {
            get => (DateTime?) GetValue(DateFromProperty);
            set => SetValue(DateFromProperty, value);
        }

        public static readonly DependencyProperty DateUntilProperty = DependencyProperty.Register(
            "DateUntil", typeof(DateTime?), typeof(PeriodSelector), new PropertyMetadata(default(DateTime?),DateUntilChanged));

        private static void DateUntilChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PeriodSelector me)) return;

            if(me.DateUntilSelection.SelectedDate == me.DateUntil)
                return;

            me.DateUntilSelection.SelectedDate = me.DateUntil;
        }

        public DateTime? DateUntil
        {
            get => (DateTime?) GetValue(DateUntilProperty);
            set => SetValue(DateUntilProperty, value);
        }

        private void InitializeShortcutOptions()
        {
            _shortcuts = new Dictionary<string, Tuple<DateTime, DateTime>>()
            {
                {Captions.Today, Tuple.Create(DateTime.Now, DateTime.Now) },
                {Captions.Yesterday, Tuple.Create(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1)) },
                {Captions.ThisWeek, Tuple.Create(DateTime.Now.FirstDayOfWeek(), DateTime.Now.LastDayOfWeek()) },
                {Captions.LastWeek,
                    Tuple.Create(DateTime.Now.AddDays(-7).FirstDayOfWeek(), DateTime.Now.AddDays(-7).LastDayOfWeek()) },
                {Captions.ThisMonth, Tuple.Create(DateTime.Now.FirstDayOfMonth(), DateTime.Now.LastDayOfMonth()) },
                {Captions.LastMonth,
                    Tuple.Create(DateTime.Now.PreviousMonth().FirstDayOfMonth(), DateTime.Now.PreviousMonth().LastDayOfMonth()) },
                {Captions.ThisYear, Tuple.Create(DateTime.Now.FirstDayOfYear(), DateTime.Now.LastDayOfYear())  },
                {Captions.LastYear,
                    Tuple.Create(DateTime.Now.AddYears(-1).FirstDayOfYear(), DateTime.Now.AddYears(-1).LastDayOfYear()) }
            };

            //ShortcutSelection.Items.Add("");
            foreach (var shortcutKey in _shortcuts.Keys)
            {
                ShortcutSelection.Items.Add(shortcutKey);    
            }
        }

        private void ShortcutSelection_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ShortcutSelection.SelectedItem == null)
                return;

            var selectedPeriod = _shortcuts[(string)ShortcutSelection.SelectedItem];
            _applyingShortcut = true;
            DateFrom = selectedPeriod.Item1;
            DateUntil = selectedPeriod.Item2;
            _applyingShortcut = false;
        }

        private void DateFromSelection_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateFrom = DateFromSelection.SelectedDate;
            if (!ValidateShortcutStillValid())
                ShortcutSelection.SelectedItem = null;
        }

        private void DateUntilSelection_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateUntil = DateUntilSelection.SelectedDate;
            if (!ValidateShortcutStillValid())
                ShortcutSelection.SelectedItem = null;
        }

        private bool ValidateShortcutStillValid()
        {
            if (ShortcutSelection.SelectedItem == null || _applyingShortcut)
                return true;

            if (DateFrom == null || DateUntil == null)
                return false;
            
            var shortcut = (string)ShortcutSelection.SelectedItem;
            return _shortcuts[shortcut].Item1.Date == DateFrom.Value.Date &&
                   _shortcuts[shortcut].Item2.Date == DateUntil.Value.Date;
        }
    }
}
