using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WMM.WPF.Controls
{
    public class ImageButton : Button
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton),
                new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));


        private static void ImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Application.GetResourceStream(new Uri("pack://application:,,," + (string)e.NewValue));
        }



        public SolidColorBrush HoverBackground
        {
            get { return (SolidColorBrush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(SolidColorBrush), typeof(ImageButton), new PropertyMetadata(Application.Current.FindResource("AppBrightColor")));



    }
}
