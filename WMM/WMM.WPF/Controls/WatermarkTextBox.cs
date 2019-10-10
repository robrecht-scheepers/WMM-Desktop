using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WMM.WPF.Controls
{
    public class WatermarkTextBox : TextBox
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark", typeof(string), typeof(WatermarkTextBox), new PropertyMetadata(default(string)));

        public string Watermark
        {
            get => (string) GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }
    }
}
