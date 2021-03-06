﻿using System;
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
using System.Windows.Shapes;

namespace WMM.WPF.Categories
{
    /// <summary>
    /// Interaction logic for SelectDeleteCategoryFallbackWindow.xaml
    /// </summary>
    public partial class SelectDeleteCategoryFallbackWindow : Window
    {
        public SelectDeleteCategoryFallbackWindow()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            
            btn?.Command.Execute(null);
            this.Close();
        }


    }
}
