using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WMM.WPF.Controls
{
    public class ImageToggleButton : ToggleButton
    {
        static ImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageToggleButton), new FrameworkPropertyMetadata(typeof(ImageToggleButton)));
        }

        public string CheckedImage
        {
            get { return (string)GetValue(CheckedImageProperty); }
            set { SetValue(CheckedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.Register("CheckedImage", typeof(string), typeof(ImageToggleButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));

        public string UncheckedImage
        {
            get { return (string)GetValue(UncheckedImageProperty); }
            set { SetValue(UncheckedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UncheckedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UncheckedImageProperty =
            DependencyProperty.Register("UncheckedImage", typeof(string), typeof(ImageToggleButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));

        public string NullImage
        {
            get { return (string)GetValue(NullImageProperty); }
            set { SetValue(NullImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NullImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NullImageProperty =
            DependencyProperty.Register("NullImage", typeof(string), typeof(ImageToggleButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));

        public SolidColorBrush HoverBackground
        {
            get { return (SolidColorBrush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(SolidColorBrush), typeof(ImageToggleButton), new PropertyMetadata(Application.Current.FindResource("SCB1")));



        private static void ImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Application.GetResourceStream(new Uri("pack://application:,,," + (string)e.NewValue));
        }

        public static readonly DependencyProperty CheckedToolTipProperty = DependencyProperty.Register(
            "CheckedToolTip", typeof(string), typeof(ImageToggleButton), new PropertyMetadata(default(string)));

        public string CheckedToolTip
        {
            get { return (string) GetValue(CheckedToolTipProperty); }
            set { SetValue(CheckedToolTipProperty, value); }
        }

        public static readonly DependencyProperty UncheckedToolTipProperty = DependencyProperty.Register(
            "UncheckedToolTip", typeof(string), typeof(ImageToggleButton), new PropertyMetadata(default(string)));

        public string UncheckedToolTip
        {
            get { return (string) GetValue(UncheckedToolTipProperty); }
            set { SetValue(UncheckedToolTipProperty, value); }
        }
    }
}
