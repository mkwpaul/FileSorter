using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using WPF.Common;
using System.Windows.Data;
using System.Collections.Specialized;

namespace FileSorter
{
    public partial class MainViewModel : PropertyChangedNotifier
    {
        private string _searchText = "";

        private FileInfo? _currentFile;
        private ObservableCollection<DirectoryInfo>? _targetFolders;
        private ObservableCollection<FileInfo>? _files;
        private ICollectionView? _filteredTargets;
        private readonly CollectionViewSource collectionView;
        private int _currentFileIndex;
        private DirectoryInfo? _currentTargetFolder;
        private Settings settings;

        public ObservableCollection<IActionLog> Logs { get; } = new();

        public Commands Commands { get; }

        public Settings Settings
        {
            get => settings;
            set
            {
                if (settings != null)
                {
                    settings.PropertyChanged -= FowardNotification;
                    settings.PropertyChanged -= this.SaveToDiskOnChanged;
                }
                SetProperty(ref settings!, value);
                if (settings != null)
                {
                    settings.PropertyChanged += this.SaveToDiskOnChanged;
                    settings.PropertyChanged += FowardNotification;
                }
            }
        }

        public MainViewModel()
        {
            PropertyChanged += OnPropertyChanged;
            collectionView = new();
            collectionView.IsLiveFilteringRequested = true;
            collectionView.IsLiveSortingRequested = true;
            Commands = new Commands(this);

            Settings = settings = SettingsReader.GetSettingsFromFile() ?? new Settings();
            this.ReadSourceFolder();
            this.ReadTargetFoldersFolder();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                 case nameof(CurrentFile):
                    var temp = CurrentTargetFolder;
                    SearchText = "";
                    CurrentTargetFolder = temp;
                    break;
                case nameof(Settings.TargetFoldersFolder):
                    this.ReadTargetFoldersFolder();
                    break;
                case nameof(Settings.SourceFolder):
                    this.ReadSourceFolder();
                    break;
                case nameof(TargetFolders):
                    collectionView.Source = TargetFolders;
                    collectionView.View.Filter = this.Filter;
                    FilteredTargets = collectionView.View;
                    break;
                case nameof(SearchText):
                    collectionView.View?.Refresh();
                    Commands.SelectFirstFolder();
                    break;
                case nameof(CurrentFileIndex):
                    CurrentFile = Files?.Count > 0 ? Files![CurrentFileIndex] : null;
                    break;
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

                void OnTargetFoldersContentChanged(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    collectionView.View?.Refresh();
                }
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

        public ObservableCollection<FileInfo>? Files
        {
            get => _files;
            set => SetProperty(ref _files, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }
    }
}
