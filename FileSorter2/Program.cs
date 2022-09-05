using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Serilog;
using WPF.Common.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WPF.Common;
using AdonisUI;
using System.Windows;

namespace FileSorter;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

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
#if DEBUG
            .WriteTo.Debug(LogEventLevel.Verbose, outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext} {Level}] {Message:lj}{NewLine}{Exception}")
#endif
            .WriteTo.Sink(inMemoryLogSink, LogEventLevel.Information)
            .CreateLogger();

        builder.ConfigureLogging(logbuilder => logbuilder.AddSerilog(logger));

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

        var app = new App();

        // setup adonis-ui
        ResourceLocator.SetColorScheme(app.Resources, ResourceLocator.DarkColorScheme);

        // resources 
        app.Resources.Add("fileInfoToImage", host.Services.GetService<FileInfoToImageSourceConverter>());

        app.MainWindow = host.Services.GetService<MainWindow>()!;
        app.MainWindow.ShowDialog();
    }
}