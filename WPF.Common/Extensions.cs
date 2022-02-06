using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Common
{
    public static class Extensions
    {
        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }

        public static int Upperlimit(this int i, int limit)
        {
            return i > limit ? limit : i;
        }

        public static int LowerLimit(this int i, int limit)
        {
            return i < limit ? limit : i;
        }

        public static string EscapeFileName(this string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = name.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                if (invalidChars.Contains(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }

        public static IActionLog Log(this Task task, string description = "", LogLevel logLevel = LogLevel.Information)
        {
            return new TaskLog(task) { Description = description, LogLevel = logLevel };
        }

        public static IActionLog LogSuccess(this string description, LogLevel logLevel = LogLevel.Information)
        {
            return new SuccessLog(description) { LogLevel = logLevel };
        }

        public static IActionLog Log(this Exception exception, string descrpition = "")
        {
            return new ExceptionLog(exception) { Description = descrpition };
        }

        public static TimeOnly GetTime(this DateTime dateTIme)
        {
            return new TimeOnly(dateTIme.TimeOfDay.Ticks);
        }

        public static void Log(this ICollection<IActionLog> logs, string description, LogLevel logLevel = LogLevel.Information)
        {
            var log = description.LogSuccess(logLevel);
            logs.Add(log);
        }

        public static void Log(this ICollection<IActionLog> logs, Exception exception, string description = "")
        {
            var log = exception.Log(description);
            logs.Add(log);
        }

        public static void Log(this ICollection<IActionLog> logs, Task task, string description = "", LogLevel logLevel = LogLevel.Information)
        {
            var log = task.Log(description, logLevel);
            logs.Add(log);
        }
    }
}
