using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WMM.WPF.Controls
{
    public class MultiSelectCombo : Control
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(List<ISelectableItem>), typeof(MultiSelectCombo), new PropertyMetadata(default(List<ISelectableItem>), ItemsChanged));

        private static void ItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (MultiSelectCombo) d;
            foreach (var item in me.Items)
            {
                item.SelectionChanged += (s, a) => me.CreateDisplayText();
            }
            me.CreateDisplayText();
        }

        private void CreateDisplayText()
        {
            if (Items == null || !Items.Any(x => x.IsSelected))
            {
                DisplayText = DefaultText;
                return;
            }

            DisplayText = string.Join(", ", Items.Where(x => x.IsSelected).Select(x => x.Caption));
        }

        public List<ISelectableItem> Items
        {
            get => (List<ISelectableItem>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty DefaultTextProperty = DependencyProperty.Register(
            "DefaultText", typeof(string), typeof(MultiSelectCombo), new PropertyMetadata(default(string)));

        public string DefaultText
        {
            get => (string) GetValue(DefaultTextProperty);
            set => SetValue(DefaultTextProperty, value);
        }

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
            "DisplayText", typeof(string), typeof(MultiSelectCombo), new PropertyMetadata(default(string)));

        public string DisplayText
        {
            get { return (string) GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

    }
}
