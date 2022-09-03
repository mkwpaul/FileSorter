using Microsoft.WindowsAPICodePack.Shell;
using Serilog;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPF.Common.Converters;

public class FileInfoToImageSourceConverter : IValueConverter
{
    readonly ILogger? _log;

    public FileInfoToImageSourceConverter(ILogger? logger)
    {
        _log = logger?.ForContext<FileInfoToImageSourceConverter>();
    }

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
            _log?.Verbose("Load Image {filePath}", filePath);
            switch (Path.GetExtension(filePath).ToLower())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".gif":
                    _log?.Verbose("Is known image type");
                    return ReadImage(filePath);
            }

            _log?.Verbose("Is not known image type. Get thumbnail Image from Shell...");
            using var shellFile = ShellFile.FromFilePath(filePath);
            var source = shellFile?.Thumbnail?.ExtraLargeBitmapSource;
            if (source is not null)
            {
                return source;
            }

            return new BitmapImage();
        }, token);
    }

    object? GetImageSourceFromFileInfo(string filePath, string extension)
    {
        _log?.Verbose("Reading Image: {filePath}", filePath);
        try
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

            _log?.Verbose("reading Thumbnail from File via Shell");
            using var shellFile = ShellFile.FromFilePath(filePath);
            var source = shellFile?.Thumbnail?.ExtraLargeBitmapSource;
            return source;
        }
        catch (Exception ex)
        {
            _log?.Error(ex, "Error occured loading Image or File thumbnail: {filePath}", filePath);
            return ex;
        }
    }

    BitmapImage ReadImage(string filePath)
    {
        _log?.Verbose("reading Image from known Image type");
        // loading of file could crash if file is corrupted

        using var stream = File.OpenRead(filePath);
        using var memoryStream = new MemoryStream();

        stream.CopyToAsync(memoryStream);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.None;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        return bitmap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
