using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WPF.Common;
using FileSorter.Services;
using Serilog.Core;
using FileSorter;

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
var logger = SetupLogger();
var settings = SettingsReader.GetSettingsFromFile() ?? new Settings();
var host = SetupHost(args);
StartWpfApp(host);

Logger SetupLogger()
{
    var logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Debug(LogEventLevel.Verbose, outputTemplate: "[{Timestamp:HH:mm:ss} {SourceContext} {Level}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("log.txt")
        .WriteTo.Seq("http://localhost:5341", LogEventLevel.Verbose)
        .CreateLogger();

    Log.Logger = logger;
    return logger;
}

IHost SetupHost(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((host, config) =>
        {
            config.Sources.Clear();
            var env = host.HostingEnvironment;
            config.AddJsonFile("settings.json", optional: true, reloadOnChange: true);
        })
        .ConfigureLogging(logbuilder => logbuilder.AddSerilog(logger))
        .ConfigureServices(ConfigureServices)
        .Build();
}

void ConfigureServices(HostBuilderContext host, IServiceCollection services)
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
}

void StartWpfApp(IHost host)
{
    var app = new App();

    // setup adonis-ui
    AdonisUI.ResourceLocator.SetColorScheme(app.Resources, AdonisUI.ResourceLocator.DarkColorScheme);

    // resources 
    app.Resources.Add("fileInfoToImage", host.Services.GetService<FileInfoToImageSourceConverter>());

    app.MainWindow = host.Services.GetService<MainWindow>()!;
    app.MainWindow.ShowDialog();
}