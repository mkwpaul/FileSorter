using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPF.Common.Converters
{
    public class FileInfoToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                string s => GetImageSourceFromFileInfo(s, Path.GetExtension(s)),
                FileInfo f => GetImageSourceFromFileInfo(f.FullName, f.Extension),
                _ => value,
            };
        }

        private Dictionary<string, object> _cache = new();

        private object GetImageSourceFromFileInfo(string filePath, string extension)
        {
            if (_cache.TryGetValue(filePath, out var result) && File.Exists(filePath))
                return result;

            switch (extension.ToLower())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".gif":
                {
                    var bmi = new BitmapImage();
                    bmi.BeginInit();
                    bmi.UriSource = new Uri(filePath);
                    bmi.CacheOption = BitmapCacheOption.OnLoad;
                    bmi.EndInit();
                    _cache[filePath] = bmi;
                    return bmi;
                }
            }

            try
            {
                using var shellFile = ShellFile.FromFilePath(filePath);
                var source = shellFile?.Thumbnail?.ExtraLargeBitmapSource;
                if (source is not null)
                {
                    _cache[filePath] = source;
                    return source;
                }
            }
            catch (FileNotFoundException ex)
            {

            }
            return filePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
