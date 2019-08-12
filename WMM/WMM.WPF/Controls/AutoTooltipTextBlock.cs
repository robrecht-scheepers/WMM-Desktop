using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WMM.WPF.Controls
{
    public class AutoTooltipTextBlock : TextBlock
    {
        protected override void OnInitialized(EventArgs e)
        {
            // we want a tooltip that resizes to the contents -- a textblock with TextWrapping.Wrap will do that
            var toolTipTextBlock = new TextBlock {TextWrapping = System.Windows.TextWrapping.Wrap};
            // bind the tooltip text to the current textblock Text binding
            var binding = GetBindingExpression(TextProperty);
            if (binding != null)
            {
                toolTipTextBlock.SetBinding(TextProperty, binding.ParentBinding);
            }

            var toolTipPanel = new StackPanel();
            toolTipPanel.Children.Add(toolTipTextBlock);
            ToolTip = toolTipPanel;

            base.OnInitialized(e);
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            if (TextTrimming != System.Windows.TextTrimming.None)
            {
                e.Handled = !IsTextTrimmed();
            }
        }

        private bool IsTextTrimmed()
        {
            Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            return ActualWidth < DesiredSize.Width;
        }
    }
}
