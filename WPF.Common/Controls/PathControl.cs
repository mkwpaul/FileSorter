using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPF.Common.Commands;

namespace WPF.Common.Controls;

public enum FileType
{
    Directory,
    File,
}

public class PathControl : Control
{
    static PathControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PathControl), new FrameworkPropertyMetadata(typeof(PathControl)));
    }

    public RelayCommand OpenFileExplorerCommand { get; }

    public PathControl()
    {
        OpenFileExplorerCommand = new RelayCommand(OpenFileExplorer);
    }

    public static readonly DependencyProperty PathProperty = DependencyProperty.Register
    (
        nameof(Path),
        typeof(string),
        typeof(PathControl),
        new FrameworkPropertyMetadata
        {
            DefaultValue = "",
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        }
    );

    public static readonly DependencyProperty FileTypeProperty = DependencyProperty.Register
    (
        nameof(FileType),
        typeof(FileType),
        typeof (PathControl),
        new FrameworkPropertyMetadata
        {
            DefaultValue =  FileType.Directory,
            BindsTwoWayByDefault = false,
        }
    );

    public string Path
    {
        get => GetValue(PathProperty)?.ToString() ?? "";
        set => SetValue(PathProperty, value);
    }

    public FileType FileType
    {
        get => (FileType)GetValue(FileTypeProperty);
        set => SetValue(FileTypeProperty, value);
    }

    private void OpenFileExplorer()
    {
        var dlg = new CommonOpenFileDialog
        {
            IsFolderPicker = FileType == FileType.Directory
        };

        if (!string.IsNullOrWhiteSpace(Path))
            dlg.DefaultFileName = Path;

        switch (dlg.ShowDialog())
        {
            case CommonFileDialogResult.None:
                Path = "";
                break;
            case CommonFileDialogResult.Ok:
                Path = dlg.FileName;
                break;
            case CommonFileDialogResult.Cancel:
                break;
        }
    }
}
