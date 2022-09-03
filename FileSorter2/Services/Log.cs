using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;
using WPF.Common;

namespace FileSorter;

public class Log : ObservableCollection<IActionLog>, ILogEventSink
{

    public void Emit(LogEvent log)
    {
        var entry = ToActionLog(log);
        Add(entry);

        if (Count > 100)
        {
            for (int i = 0; i < 50; i++)
                RemoveAt(0);
        }
    }

    static IActionLog ToActionLog(LogEvent log)
    {
        var message = log.RenderMessage();
        if (log.Exception != null)
            return new ExceptionLog(log.Exception) { Description = message };

        var logLevel = log.Level switch
        {
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Verbose => LogLevel.Debug,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => throw new NotImplementedException(),
        };

        if (log.Properties.TryGetValue("task", out var value))
        {
            if (value is ScalarValue scalar)
            {
                if (scalar.Value is Task task)
                {
                    return new TaskLog(task) { Description = message, LogLevel = logLevel };
                }
            }
        }

        return new SuccessLog(message) { LogLevel = logLevel };
    }
}
