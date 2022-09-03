using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WPF.Common.Converters;

namespace WPF.Common.Controls;

public class FilePresenter : Control
{
    private readonly FileInfoToImageSourceConverter thumbConverter = new();

    public static readonly DependencyProperty FileProperty = DependencyProperty.Register
    (
        nameof(File),
        typeof(object),
        typeof(FilePresenter),
        new FrameworkPropertyMetadata
        {
            DefaultValue = null,
            PropertyChangedCallback = (s, e) => (s as FilePresenter)!.OnFileChanged(e),
        }
    );

    public static readonly DependencyProperty FileInfoProperty = DependencyProperty.Register
    (
        nameof(FileInfo),
        typeof(FileInfo),
        typeof(FilePresenter),
        new FrameworkPropertyMetadata
        {
            DefaultValue = null,
            PropertyChangedCallback = (s, e) => (s as FilePresenter)!.OnFileInfoChanged(e),
        }
    );

    private void OnFileInfoChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is FileInfo info)
        {
            var gethumbnail = thumbConverter.ConvertAsync(info.FullName);
            ThumbnailSource = new TaskCompletionNotifier<BitmapSource>(gethumbnail);
        }
    }

    private readonly BitmapSource timeout = new BitmapImage();
    private async Task<BitmapSource> getTimeOut()
    {
        await Task.Delay(TimeSpan.FromSeconds(30));
        return timeout;
    }

    public static readonly DependencyPropertyKey ThumbnailSourcePropertyKey = DependencyProperty.RegisterReadOnly
    (
        nameof(ThumbnailSource),
        typeof(TaskCompletionNotifier<BitmapSource>),
        typeof(FilePresenter),
        new FrameworkPropertyMetadata(null)
    );

    private static readonly DependencyProperty ThumbnailSourceProperty = ThumbnailSourcePropertyKey.DependencyProperty;

    public TaskCompletionNotifier<BitmapSource> ThumbnailSource
    {
        get => (TaskCompletionNotifier<BitmapSource>)GetValue(ThumbnailSourceProperty);
        private set => SetValue(ThumbnailSourceProperty, value);
    }

    public object File
    {
        get => GetValue(FileProperty);
        set => SetValue(FileProperty, value);
    }

    private void OnFileChanged(DependencyPropertyChangedEventArgs e)
    {
        switch (e.NewValue)
        {
            case string s:
                if (System.IO.File.Exists(s))
                    ThumbnailSource = new TaskCompletionNotifier<BitmapSource>(thumbConverter.ConvertAsync(s));
                break;
            case FileInfo fileInfo:
                FileInfo = fileInfo;
                break;
        }
    }

    public FileInfo FileInfo
    {
        get => (FileInfo)GetValue(FileInfoProperty);
        set => SetValue(FileInfoProperty, value);
    }

    static FilePresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FilePresenter), new FrameworkPropertyMetadata(typeof(FilePresenter)));
    }
}
