using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.WPF.MVVM;

namespace WMM.WPF.Helpers
{
    interface IWindowService
    {
        void OpenDialogWindow(ObservableObject dataContext);
    }
}
