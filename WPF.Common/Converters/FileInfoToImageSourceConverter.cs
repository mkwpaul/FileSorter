using Microsoft.WindowsAPICodePack.Shell;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPF.Common.Converters
{
    public class FileInfoToImageSourceConverter : IValueConverter, IHasStaticInstance<FileInfoToImageSourceConverter>
    {
        public static FileInfoToImageSourceConverter Instance { get; } = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                string s => GetImageSourceFromFileInfo(s, Path.GetExtension(s)),
                FileInfo f => GetImageSourceFromFileInfo(f.FullName, f.Extension),
                _ => value,
            };
        }

        public Task<BitmapSource> ConvertAsync(string filePath, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                switch (Path.GetExtension(filePath).ToLower())
                {
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".bmp":
                    case ".gif":
                        return ReadImage(filePath);
                }

                using var shellFile = ShellFile.FromFilePath(filePath);
                var source = shellFile?.Thumbnail?.ExtraLargeBitmapSource;
                if (source is not null)
                {
                    return source;
                }

                return new BitmapImage();
            }, token);
        }
        private object GetImageSourceFromFileInfo(string filePath, string extension)
        {
            switch (extension.ToLower())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".gif":
                    return ReadImage(filePath);
            }

            try
            {
                using var shellFile = ShellFile.FromFilePath(filePath);
                var source = shellFile?.Thumbnail?.ExtraLargeBitmapSource;
                return source;
            }
            catch (FileNotFoundException ex)
            {
                return ex;
            }
        }

        private static BitmapImage ReadImage(string filePath)
        {
            // loading of file could crash if file is corrupted
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
