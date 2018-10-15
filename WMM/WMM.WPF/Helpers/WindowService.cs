﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.WPF.MVVM;

namespace WMM.WPF.Helpers
{
    public class WindowService : IWindowService
    {
        public void OpenDialogWindow(ObservableObject dataContext)
        {
            if (dataContext is RecurringTransactionsViewModel)
            {
                var window = new RecurringTransactionsWindow() {DataContext = dataContext};
                window.ShowDialog();
            }
        }
    }
}
