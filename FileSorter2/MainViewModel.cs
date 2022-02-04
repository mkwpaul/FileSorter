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

namespace FileSorter
{
    public class MainViewModel : PropertyChangedNotifier
    {
        private string _searchText;
        private FileInfo? _currentFilePath;
        private string? _targetFoldersFolder;
        public Exception? _targetFolderIssue;
        private ObservableCollection<string>? _targetFolders;
        private string? _sourceFolder;
        private ObservableCollection<FileInfo>? _files;
        private ICollectionView? _filteredTargets;
        private CollectionViewSource collectionView;
        private int _currentFileIndex;
        private string? _currentTargetFolder;


        public Commands Commands { get; } 

        public MainViewModel()
        {
            PropertyChanged += OnPropertyChanged;
            collectionView = new();
            collectionView.IsLiveFilteringRequested = true;
            Commands = new Commands(this);
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
                    ReadTargetFoldersFolder(TargetFoldersFolder);
                    break;
                case nameof(SourceFolder):
                    ReadSourceFolder(SourceFolder);
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

        private void ReadSourceFolder(string? sourceFolder)
        {
            if (sourceFolder == null)
            {
                Files = null;
                CurrentFile = null;
                return;
            }
            try
            {
                Exception = null;
                Files = Directory.GetFiles(sourceFolder)
                    .Select(f => new FileInfo(f))
                    .ToObservable();
                CurrentFile = Files.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        private void ReadTargetFoldersFolder(string? path)
        {
            if (path == null)
            {
                TargetFolders = null;
                return;
            }
            try
            {
                Exception = null;
                TargetFolders = Directory.GetDirectories(path)
                    .Select(x => Path.GetRelativePath(path, x))
                    .ToObservable();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public int CurrentFileIndex
        {
            get => _currentFileIndex.Upperlimit(Files?.Count - 1 ?? 0).LowerLimit(0);
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
            get => _targetFolderIssue;
            set => SetProperty(ref _targetFolderIssue, value);
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

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }
    }
}
