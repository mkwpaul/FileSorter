using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using WPF.Common;
using System.Windows.Data;
using System.Collections.Specialized;
using Serilog;
using WPF.Common.Commands;

namespace FileSorter;

public class TargetFolderSource : PropertyChangedNotifier
{
    string folder = ""; FolderSourceType folderType = FolderSourceType.SubFolders;

    public string Folder
    {
        get => folder;
        set => SetProperty(ref folder, value);
    }

    public FolderSourceType FolderType
    {
        get => folderType;
        set => SetProperty(ref folderType, value);
    }
}

public class Settings : PropertyChangedNotifier
{
    string _sourceFolder = "";
    string _targetFoldersFolder = "";
    bool _askBeforeFileDeletion = true;

    public string SourceFolder { get => _sourceFolder; set => SetProperty(ref _sourceFolder, value); }

    public string TargetFoldersFolder { get => _targetFoldersFolder; set => SetProperty(ref _targetFoldersFolder, value); }

    public bool AskBeforeFileDeletion { get => _askBeforeFileDeletion; set => SetProperty(ref _askBeforeFileDeletion, value); }

    public ObservableCollection<TargetFolderSource> TargetSources { get; } = new();
}

public class MainViewModel : PropertyChangedNotifier
{
    string _searchText = "";
    FileInfo? _currentFile;
    ObservableCollection<DirectoryInfo>? _targetFolders;
    ICollectionView? _filteredTargets;
    int _currentFileIndex;
    DirectoryInfo? _currentTargetFolder;
    Settings? settings;

    readonly CollectionViewSource collectionView;
    readonly MainModule _main;
    readonly ILogger log;

    public MainViewModel(MainModule main, Settings settings, Log sink, ILogger logger)
    {
        _main = main;
        Logs = sink;
        log = logger.ForContext<MainViewModel>();
        Settings = settings;

        PropertyChanged += OnPropertyChanged;
        collectionView = new()
        {
            IsLiveFilteringRequested = true,
            IsLiveSortingRequested = true
        };

        main.ReadSourceFolder(this);
        main.CreateNewFolderFromSearch(this);

        GoToNextCommand = new RelayCommand(() => main.GoToNextFile(this));
        GoToPreviousCommand = new RelayCommand(() => main.GoToPreviousFile(this));
        DeleteFileCommand = new RelayCommand<FileInfo>(x => main.DeleteFile(this, x));
        OpenInExplorerCommand = new RelayCommand<FileInfo>(main.OpenInExplorer);
        MoveToTargetFolderCommand = new RelayCommand(() => main.MoveToTargetFolder(this));
        SelectFirstFolderCommand = new RelayCommand(() => main.SelectFirstFolder(this));
        OnEnterCommand = new RelayCommand(() => main.OnEnter(this));
        CreateNewFolderFromSearchCommand = new RelayCommand(() => main.CreateNewFolderFromSearch(this));

        AddFolderSource = new RelayCommand(() => main.AddNewSource(this));
        RemoveFolderSource = new RelayCommand<TargetFolderSource>(x => main.RemoveSource(this, x));
    }

    public Log Logs { get; }
    public Command AddFolderSource { get; }
    public Command RemoveFolderSource { get; }
    public Command GoToNextCommand { get; }
    public Command GoToPreviousCommand { get; }
    public Command DeleteFileCommand { get; }
    public Command OpenInExplorerCommand { get; }
    public Command MoveToTargetFolderCommand { get; }
    public Command SelectFirstFolderCommand { get; }
    public Command CreateNewFolderFromSearchCommand { get; }
    public Command OnEnterCommand { get; }

    public Settings? Settings
    {
        get => settings;
        set
        {
            if (settings != null)
            {
                settings.PropertyChanged -= FowardNotification;
                settings.PropertyChanged -= SaveToDiskOnChanged;
            }
            SetProperty(ref settings!, value);
            if (settings != null)
            {
                settings.PropertyChanged += SaveToDiskOnChanged;
                settings.PropertyChanged += FowardNotification;
            }
        }
    }

    public int CurrentFileIndex
    {
        get => _currentFileIndex.LowerLimit(0).Upperlimit(Files?.Count - 1 ?? 0);
        set
        {
            value = value.Upperlimit(Files?.Count - 1 ?? 0).LowerLimit(0);
            SetProperty(ref _currentFileIndex, value);
        }
    }

    public FileInfo? CurrentFile
    {
        get => _currentFile;
        set => SetProperty(ref _currentFile, value);
    }

    public ObservableCollection<DirectoryInfo>? TargetFolders
    {
        get => _targetFolders;
        set
        {
            if (_targetFolders != null)
                _targetFolders.CollectionChanged -= OnTargetFoldersContentChanged;
            SetProperty(ref _targetFolders, value);
            if (_targetFolders != null)
                _targetFolders.CollectionChanged += OnTargetFoldersContentChanged;
        }
    }

    public DirectoryInfo? CurrentTargetFolder
    {
        get => _currentTargetFolder;
        set => SetProperty(ref _currentTargetFolder, value);
    }

    public ICollectionView? FilteredTargets
    {
        get => _filteredTargets;
        set => SetProperty(ref _filteredTargets, value);
    }

    public ObservableCollection<FileInfo> Files { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CurrentFile):
                // Reset SearchText while keeping selected Target
                var temp = CurrentTargetFolder;
                SearchText = "";
                CurrentTargetFolder = temp;
                break;
            case nameof(Settings.SourceFolder):
                _main.ReadSourceFolder(this);
                break;
            case nameof(TargetFolders):
                collectionView.Source = TargetFolders;
                collectionView.View.Filter = Filter;
                FilteredTargets = collectionView.View;
                break;
            case nameof(SearchText):
                collectionView.View?.Refresh();
                _main.SelectFirstFolder(this);
                break;
            case nameof(CurrentFileIndex):
                CurrentFile = Files?.Count > 0 ? Files![CurrentFileIndex] : null;
                break;
        }
    }

    void OnTargetFoldersContentChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        collectionView.View?.Refresh();
    }

    void SaveToDiskOnChanged(object? sender, PropertyChangedEventArgs _)
    {
        if (sender is not Settings settings)
            return;

        var task = SettingsReader.SafeToDisk(settings);
        log.Information("Saving Settings to Disk, {task}", task);
    }

    bool Filter(object o)
    {
        if (o is not DirectoryInfo dicInfo)
            return false;

        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        return dicInfo.Name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
    }

    internal void FolderSourcePropertyChange(object? sender, PropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }
}