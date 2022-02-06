using Microsoft.WindowsAPICodePack.Shell;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPF.Common.Converters
{
    public class FileInfoToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                string s => GetImageSourceFromFileInfo(s, Path.GetExtension(s)),
                FileInfo f => GetImageSourceFromFileInfo(f.FullName, f.Extension),
                _ => value,
            };
        }

        private readonly Dictionary<string, object> _cache = new();

        private object? GetImageSourceFromFileInfo(string filePath, string extension)
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
                    return ReadImageToCache(filePath);
            }

            try
            {
                using var shellFile = ShellFile.FromFilePath(filePath);
                var source = shellFile?.Thumbnail?.ExtraLargeBitmapSource;
                if (source is not null)
                    _cache[filePath] = source;
                return source;
            }
            catch (FileNotFoundException ex)
            {
                return ex;
            }
        }

        private BitmapImage ReadImageToCache(string filePath)
        {
            // Image Controls can load images directly by giving them the file path directly as source
            // However when doing that they block the files from being nmoved or deleted
            // Hence why we manually load them
            var result = ReadImage(filePath);
            _cache[filePath] = result;
            return result;
        }

        private static BitmapImage ReadImage(string filePath)
        {
            var bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri(filePath);
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.EndInit();
            return bmi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
