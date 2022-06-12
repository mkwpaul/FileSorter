using AdonisUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FileSorter
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            InitializeComponent();
            ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var host = CreateHostBuilder(e.Args).Build();
            this.MainWindow = new MainWindow();
            this.MainWindow.Show();
        }


        static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureServices(ConfigureServices);
            return builder;
        }

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
        }
    }
}
