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

    public BitmapSource Convert(string filePath)
    {
        _log?.Verbose("Load Image {filePath}", filePath);
        var extension = Path.GetExtension(filePath).ToLower();
        switch (extension)
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".bmp":
            case ".gif":
                _log?.Verbose("Is known file extension (extension = {extension}", extension);
                return ReadImage(filePath);
        }

        _log?.Verbose("File Extension {extension] is not known image type. Get thumbnail Image from Shell...", extension);

        using var shellFile = ShellFile.FromFilePath(filePath);
        var source = shellFile?.Thumbnail?.ExtraLargeBitmapSource;
        if (source is not null)
        {
            return source;
        }

        _log?.Verbose("Thumbnail source for {filepath} was null", filePath);
        return new BitmapImage();
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

        // the following requirements must be met:

        // - the file must not be blocked for the sake of moving and deleting
        // - the file must not be cached for too, otherwise we quickly run out of memory.
        //   (the build in caching is too long)
    
        using var stream = File.OpenRead(filePath);
        var memoryStream = new MemoryStream();

        stream.CopyTo(memoryStream);

        stream.Seek(0, SeekOrigin.Begin);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        return bitmap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
