using AdonisUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPF.Common;
using Serilog;
using Serilog.Events;
using WPF.Common.Converters;
using System.Windows.Input;

namespace FileSorter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var builder = Host.CreateDefaultBuilder(e.Args);

        builder.ConfigureAppConfiguration((host, config) =>
        {
            config.Sources.Clear();
            var env = host.HostingEnvironment;
            config.AddJsonFile("settings.json", optional: true, reloadOnChange: true);

            var root = config.Build();
            var settings = new Settings();
            root.GetSection(nameof(Settings)).Bind(settings);
        });

        var inMemoryLogSink = new Log();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Debug(LogEventLevel.Verbose, outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext} {Level}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Sink(inMemoryLogSink, LogEventLevel.Information)
            //.WriteTo.File("log.txt", shared: true, outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext} {Level}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();


        builder.ConfigureLogging(logbuilder =>
        {
            logbuilder.AddSerilog(logger);
        });

        var host = builder.ConfigureServices((host, services) =>
            {
                var settings = SettingsReader.GetSettingsFromFile() ?? new Settings();
                services
                .AddSingleton<ILogger>(logger)
                .AddSingleton(inMemoryLogSink)
                .AddSingleton<IUserInteraction, UserInteraction>()
                .AddSingleton(settings)
                .AddSingleton<MainModule>()
                .AddSingleton<MainViewModel>()
                .AddSingleton<MainWindow>()
                .AddSingleton<FileInfoToImageSourceConverter>()
                ;
            })
            .Build();

        // resources 
        Resources.Add("fileInfoToImage", host.Services.GetService<FileInfoToImageSourceConverter>());

        MainWindow = host.Services.GetService<MainWindow>()!;
        MainWindow.Show();
    }
}
