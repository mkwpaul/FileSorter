using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using WPF.Common;
using System.Windows.Data;
using System.Collections.Specialized;
using WPF.Common.Controls;

namespace FileSorter
{
    public class MainViewModel : PropertyChangedNotifier
    {
        private string _searchText;
        private FileInfo? _currentFilePath;
        private string? _targetFoldersFolder;
        public Exception? _Exception;
        private ObservableCollection<string>? _targetFolders;
        private string? _sourceFolder;
        private ObservableCollection<FileInfo>? _files;
        private ICollectionView? _filteredTargets;
        private CollectionViewSource collectionView;
        private int _currentFileIndex;
        private string? _currentTargetFolder;

        public Commands Commands { get; }

        public Settings Settings { get; set; }

        public MainViewModel()
        {
            PropertyChanged += OnPropertyChanged;
            collectionView = new();
            collectionView.IsLiveFilteringRequested = true;
            collectionView.IsLiveSortingRequested = true;
            Commands = new Commands(this);

            this.LoadSettings();
        }

        private bool Filter(object o)
        {
            if (o is not string s)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            return s.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
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
                case nameof(TargetFoldersFolder):
                    this.ReadTargetFoldersFolder();
                    break;
                case nameof(SourceFolder):
                    this.ReadSourceFolder();
                    break;
                case nameof(TargetFolders):
                    collectionView.Source = TargetFolders;
                    collectionView.View.Filter = Filter;
                    FilteredTargets = collectionView.View;
                    break;
                case nameof(SearchText):
                    collectionView.View?.Refresh();
                    Commands.SelectFirstFolder();
                    break;
                case nameof(CurrentFileIndex):

                    if (Files is null)
                        return;
                    if (Files.Count == 0)
                        CurrentFile = null;
                    else
                        CurrentFile = Files[CurrentFileIndex];
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
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        public string? TargetFoldersFolder
        {
            get => _targetFoldersFolder;
            set => SetProperty(ref _targetFoldersFolder, value);
        }

        public Exception? Exception
        {
            get => _Exception;
            set => SetProperty(ref _Exception, value);
        }

        public ObservableCollection<string>? TargetFolders
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

        private void OnTargetFoldersContentChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            collectionView.View?.Refresh();
        }

        public string? CurrentTargetFolder
        {
            get => _currentTargetFolder;
            set => SetProperty(ref _currentTargetFolder, value);
        }

        public ICollectionView? FilteredTargets
        {
            get => _filteredTargets;
            set => SetProperty(ref _filteredTargets, value);
        }

        public string? SourceFolder
        {
            get => _sourceFolder;
            set => SetProperty(ref _sourceFolder, value);
        }

        public ObservableCollection<FileInfo>? Files
        {
            get => _files;
            set => SetProperty(ref _files, value);
        }

        public ObservableCollection<IActionLog> Logs { get; } = new();

        public IActionLog _Log;
        public IActionLog Log
        {
            get => _Log;
            set => SetProperty(ref _Log, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }
    }
}
