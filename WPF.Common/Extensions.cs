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
    }
}
