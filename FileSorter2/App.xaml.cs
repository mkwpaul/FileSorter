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
using WPF.Common;

namespace FileSorter;

public partial class App : Application
{
    IHost host;
    IServiceProvider services;
    public App()
    {
        InitializeComponent();
        ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        host = 
            Host.CreateDefaultBuilder(e.Args)
            .ConfigureServices((host, services) =>
                {
                    services.AddSingleton<IUserInteraction, UserInteraction>();

                    var settings = SettingsReader.GetSettingsFromFile() ?? new Settings();
                    services.AddSingleton(settings);
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
            .Build();

        services = host.Services;

        MainWindow = services.GetService<MainWindow>()!;
        MainWindow.Show();
    }
}
