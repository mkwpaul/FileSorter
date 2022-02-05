using AdonisUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FileSorter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);
        }
    }
}
