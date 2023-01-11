using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using WPF.Common;
using System.Windows.Data;
using System.Collections.Specialized;
using Serilog;
using WPF.Common.Commands;
using AdonisUI;
using System.Collections;
using System.Runtime;
using System.Windows.Navigation;

namespace FileSorter;

public enum FolderSourceType { SubFolders, IndividualFolder, SubFoldersRecursive };

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

    public ObservableCollection<TargetFolderSource> TargetSources { get; set; } = new();
}

public class State : PropertyChangedNotifier
{
    string _searchText = "";
    FileInfo? _currentFile;
    int _currentFileIndex;
    ICollectionView? _filteredTargets;
    DirectoryInfo? _currentTargetFolder;

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

    public ObservableCollection<DirectoryInfo> TargetFolders { get; } = new();

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
}

public interface IWorld
{
    State State { get; }
    Settings Settings { get; }
}

public class MainViewModel : PropertyChangedNotifier, IWorld
{
    readonly CollectionViewSource collectionView;
    readonly MainModule _main;
    readonly ILogger log;

    public MainViewModel(MainModule main, Settings settings, ILogger logger)
    {
        _main = main;
        //Logs = sink;
        log = logger.ForContext<MainViewModel>();
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        settings.PropertyChanged += SaveToDiskOnChanged;
        settings.PropertyChanged += FowardNotification;

        PropertyChanged += OnPropertyChanged;

        collectionView = new()
        {
            IsLiveFilteringRequested = true,
            IsLiveSortingRequested = true
        };
        var state = new State();
        State = state;
        main.UpdateTargetFoldersFromTargetSources(settings, state);

        foreach (var folderSource in settings.TargetSources)
        {
            folderSource.PropertyChanged += (s, e) =>
            {
                main.UpdateTargetFoldersFromTargetSources(settings, state);
            };
        }

        collectionView.Source = state.TargetFolders;
        collectionView.View.Filter = Filter;

        if (collectionView.View is ListCollectionView view)
            view.CustomSort = new CustomDicComparer(this);

        state.FilteredTargets = collectionView.View;
        state.TargetFolders.CollectionChanged += OnTargetFoldersContentChanged;
        state.PropertyChanged += OnPropertyChanged;

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

    public Settings Settings { get; }

    public State State { get; }

    void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(State.CurrentFile):
                // Reset SearchText while keeping selected Target
                var temp = State.CurrentTargetFolder;
                State.SearchText = "";
                State.CurrentTargetFolder = temp;
                break;
            case nameof(Settings.SourceFolder):
                _main.ReadSourceFolder(this);
                break;

            case nameof(State.SearchText):
                collectionView.View?.Refresh();
                _main.SelectFirstFolder(this);
                break;
            case nameof(State.CurrentFileIndex):
                State.CurrentFile = State.Files?.Count > 0 ? State.Files![State.CurrentFileIndex] : null;
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

        if (string.IsNullOrWhiteSpace(State.SearchText))
            return true;

        if (dicInfo.Name.Contains(State.SearchText, StringComparison.CurrentCultureIgnoreCase))
            return true;

        var ratio = FuzzySharp.Fuzz.Ratio(dicInfo.Name, State.SearchText);
        return ratio > 80;
    }

    public class CustomDicComparer : IComparer
    {
        readonly MainViewModel mv;
        public CustomDicComparer(MainViewModel mv) { this.mv = mv; }

        public int Compare(object? x, object? y)
        {
            if (x is not DirectoryInfo infoX)
                return -1;
            if (y is not DirectoryInfo infoY)
                return 1;

            if (string.IsNullOrEmpty(mv.State.SearchText))
                return StringComparer.OrdinalIgnoreCase.Compare(infoX.Name, infoY.Name);

            var ratioX = FuzzySharp.Fuzz.Ratio(infoX.Name, mv.State.SearchText);
            var ratioY = FuzzySharp.Fuzz.Ratio(infoY.Name, mv.State.SearchText);

            // better matches (higher ratios) should be listed earlier
            // so compare Y to X instead of X to Y to reverse order.
            return ratioY.CompareTo(ratioX);
        }
    }
}