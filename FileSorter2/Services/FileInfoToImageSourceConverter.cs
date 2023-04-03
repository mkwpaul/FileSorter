using Microsoft.WindowsAPICodePack.Shell;
using Serilog;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FileSorter.Services;

public class FileInfoToImageSourceConverter : IValueConverter
{
    readonly ILogger? _log;
    readonly ImageCache _cache;

    public FileInfoToImageSourceConverter(ILogger? logger, ImageCache cache)
    {
        _log = logger?.ForContext<FileInfoToImageSourceConverter>();
        _cache = cache;
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

    object? GetImageSourceFromFileInfo(string filePath, string extension)
    {
        if (_cache.TryGetImageFromCache(filePath, out var image))
        {
            _log?.Verbose("Retrieved Image {filePath} from Cache", filePath);
            return image;
        }

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

        // the following requirements must be met:

        // - the file must not be blocked for the sake of moving and deleting
        // - the file must not be cached for too, otherwise we quickly run out of memory.
        //   (the build in caching is too long)
    
        using var fileStream = File.OpenRead(filePath);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = fileStream;
        bitmap.EndInit();
        _cache.Add(filePath, bitmap);

        return bitmap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
