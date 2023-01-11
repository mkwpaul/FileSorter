using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WPF.Common;
using AdonisUI;
using FileSorter.Services;

namespace FileSorter;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Debug(LogEventLevel.Verbose, outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext} {Level}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("log.txt")
            .WriteTo.Seq("http://localhost:5341", LogEventLevel.Verbose)
            .CreateLogger();

        SettingsReader._log = logger.ForContext<SettingsReader>();

        var settings = SettingsReader.GetSettingsFromFile() ?? new Settings();
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureAppConfiguration((host, config) =>
        {
            config.Sources.Clear();
            var env = host.HostingEnvironment;
            config.AddJsonFile("settings.json", optional: true, reloadOnChange: true);
        });



        builder.ConfigureLogging(logbuilder => logbuilder.AddSerilog(logger));

        var host = builder.ConfigureServices((host, services) =>
        {
            services
            .AddSingleton<ILogger>(logger)
            .AddSingleton<IUserInteraction, UserInteraction>()
            .AddSingleton(settings)
            .AddSingleton<ImageCache>()
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
        app.Resources.Add("fileInfoToImage", host.Services.GetService<Services.FileInfoToImageSourceConverter>());

        app.MainWindow = host.Services.GetService<MainWindow>()!;
        app.MainWindow.ShowDialog();
    }
}