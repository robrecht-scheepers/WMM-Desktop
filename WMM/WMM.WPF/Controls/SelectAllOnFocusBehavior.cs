using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace WMM.WPF.Controls
{
    public class SelectAllOnFocusBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotKeyboardFocus += AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture += AssociatedObjectGotMouseCapture;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObjectPreviewMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture -= AssociatedObjectGotMouseCapture;
            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObjectPreviewMouseLeftButtonDown;
        }

        private void AssociatedObjectGotKeyboardFocus(object sender,
            System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            AssociatedObject.SelectAll();
        }

        private void AssociatedObjectGotMouseCapture(object sender,
            System.Windows.Input.MouseEventArgs e)
        {
            AssociatedObject.SelectAll();
        }

        private void AssociatedObjectPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AssociatedObject.IsKeyboardFocusWithin)
            {
                AssociatedObject.Focus();
                e.Handled = true;
            }
        }
    }
}
