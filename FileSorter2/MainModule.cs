using Microsoft.VisualBasic.FileIO;
using Serilog;
using System.Diagnostics;
using System.IO;
using WPF.Common;
using System.Windows;
using System.Collections.ObjectModel;

namespace FileSorter;

public enum FolderSourceType { SubFolders, IndividualFolder };

public record TargetFolder(TargetFolderSource Source, string Name);

public class MainModule
{
    readonly IUserInteraction _doesUserWant;
    readonly Settings _settings;
    readonly ILogger _log;

    public MainModule(ILogger logger, Settings settings, IUserInteraction userInteraction)
    {
        _log = logger.ForContext<MainModule>();
        _doesUserWant = userInteraction;
        _settings = settings;
    }

    #region Settings

    public void AddNewSource(MainViewModel mv)
    {
        var folderSource = new TargetFolderSource();
        folderSource.PropertyChanged += (s, e) =>
        {
            mv.TargetFolders = GetDirectories(_settings.TargetSources);
        };

        _settings.TargetSources.Add(folderSource);
    }

    public void RemoveSource(MainViewModel mv, TargetFolderSource folderSource)
    {
        _settings.TargetSources.Remove(folderSource);
        folderSource.ClearNotifications();
        mv.TargetFolders = GetDirectories(_settings.TargetSources);
    }

    public ObservableCollection<DirectoryInfo> GetDirectories(IEnumerable<TargetFolderSource> sources)
    {
        // hashset to keep track of entries and check for duplicates.
        var hashset = new HashSet<string>();
        var result = new ObservableCollection<DirectoryInfo>();
        foreach(var source in sources)
            ReadTargetFolderSource(result, hashset, source);

        return result;
    }

    void ReadTargetFolderSource(ObservableCollection<DirectoryInfo> result, HashSet<string> hashset, TargetFolderSource source)
    {
        try
        {
            if (!Directory.Exists(source.Folder))
                return;

            if (source.FolderType == FolderSourceType.IndividualFolder)
            {
                if (hashset.Add(source.Folder))
                    result.Add(new DirectoryInfo(source.Folder));
            }

            else if (source.FolderType == FolderSourceType.SubFolders)
            {
                string[] dics = Directory.GetDirectories(source.Folder);
                foreach (var dic in dics)
                {
                    if (hashset.Add(dic))
                    {
                        var info = new DirectoryInfo(dic);
                        result.Add(info);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error occured reading folder {targetSource}", source.Folder);
        }
    }

    #endregion

    public static void RemoveFile(MainViewModel mv, FileInfo? info)
    {
        // Remove From MainViewModel so the file can be moved and isn't blocked by ourselves.
        if (info is null)
            return;

        mv.Files?.Remove(info);
        if (info != mv.CurrentFile)
            return;

        mv.CurrentFileIndex = mv.CurrentFileIndex;
        if (mv.Files?.Count > 0)
            mv.CurrentFile = mv.Files?[mv.CurrentFileIndex];
        else
            mv.CurrentFile = null;
    }


    public void ReadSourceFolder(MainViewModel mv) => ReadSourceFolder(mv, mv.Settings?.SourceFolder);
    public void ReadSourceFolder(MainViewModel mv, string? sourceFolder)
    {
        mv.Files.Clear();
        if (sourceFolder == null)
        {
            mv.CurrentFile = null;
            return;
        }

        try
        {
            var newEntries = Directory.GetFiles(sourceFolder)
                .Select(f => new FileInfo(f))
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            foreach (var item in newEntries)
                mv.Files.Add(item);
                
            mv.CurrentFile = mv.Files.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "");
        }
    }

    public void CreateNewFolderFromSearch(MainViewModel Mv)
    {
        if (string.IsNullOrWhiteSpace(Mv.SearchText))
            return;
        if (_settings.SourceFolder is null)
            return;
        if (_settings.TargetFoldersFolder is null)
            return;

        var newFolder = Mv.SearchText.EscapeFileName();

        var newFolderFull = Path.Combine(_settings.TargetFoldersFolder, newFolder);

        if (!_doesUserWant.CreateFolder(newFolderFull))
            return;

        try
        {
            Directory.CreateDirectory(newFolderFull);
            _log.Information("Created new Folder at {newFolderFull}", newFolderFull);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Create new Folder at {newFolderFull}", newFolderFull);
            return;
        }

        if (Directory.Exists(newFolderFull))
        {
            var newDicInfo = new DirectoryInfo(newFolderFull);
            Mv.TargetFolders?.Add(newDicInfo);
            Mv.CurrentTargetFolder = newDicInfo;

            if (_doesUserWant.MoveFileToNewFolder())
                MoveToTargetFolder(Mv);
        }
    }

    public void OnEnter(MainViewModel Mv)
    {
        if (Mv.CurrentTargetFolder is null)
            SelectFirstFolder(Mv);
        else
            MoveToTargetFolder(Mv);
    }

    public void SelectFirstFolder(MainViewModel Mv)
    {
        var entry = Mv.FilteredTargets?.OfType<DirectoryInfo>().FirstOrDefault();
        Mv.CurrentTargetFolder = entry;
    }

    public void MoveToTargetFolder(MainViewModel mv)
    {
        if (mv.CurrentFile is null)
            return;
        if (mv.CurrentTargetFolder is null)
            return;

        var currentTarget = mv.CurrentTargetFolder;
        var newFullPath = Path.Combine(currentTarget.FullName, mv.CurrentFile.Name);
        var file = mv.CurrentFile;

        bool cancel = CheckForFileConflict(mv, newFullPath, file);
        if (cancel)
            return;

        GoToNextFile(mv);
        Task.Run(() =>
        {
            try
            {
                File.Move(file.FullName, newFullPath, true);
                RemoveFile(mv, file);

                Application.Current.Dispatcher.InvokeAsync(() =>
                    _log.Information("Moved {file} from {directory} to {currentTarget}", file.Name, file.Directory, currentTarget));
            }
            catch (IOException ex)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                    _log.Error(ex, "MoveToTargetFolder Failed."));
            }
        });
    }

    bool CheckForFileConflict(MainViewModel mv, string newFullPath, FileInfo file)
    {
        if (!File.Exists(newFullPath))
            return false;

        var answer = _doesUserWant.ReactToFileCollision(newFullPath);
        switch (answer)
        {
            case FileCollisionReaction.Overwrite:
                return false;
            case FileCollisionReaction.Delete:
                DeleteFile(mv, file, true);
                return true;
            case FileCollisionReaction.Cancel:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    public void DeleteFile(MainViewModel mv, FileInfo file) => DeleteFile(mv, file, false);
    public void DeleteFile(MainViewModel mv, FileInfo file, bool skipDialog)
    {
        if (file is null)
            return;

        if (!skipDialog && _settings.AskBeforeFileDeletion && !_doesUserWant.DeleteFile())
        {
            return;
        }

        GoToNextFile(mv);
        try
        {
            FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            RemoveFile(mv, file);
            _log.Information("Deleted {file}", file.Name);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Delete File Failed");
        }
    }

    public void OpenInExplorer(FileInfo file)
    {
        if (file is null)
            return;

        if (!File.Exists(file.FullName))
            return;

        string argument = $"/select, \"{file.FullName}\"";
        Process.Start("explorer.exe", argument);
    }

    public static void GoToPreviousFile(IList<FileInfo> files, int currentIndex, Action<int> setIndex)
    {
        if (files is null || files.Count == 0)
            return;

        int newIndex = (currentIndex == 0) ? files.Count - 1 : currentIndex - 1;
        setIndex(newIndex);
    }

    public void GoToPreviousFile(MainViewModel mv)
    {
        if (mv.Files is null || mv.Files.Count == 0)
            return;

        int newIndex = mv.CurrentFileIndex - 1;
        mv.CurrentFileIndex = newIndex < 0 ? mv.Files.Count - 1 : newIndex;
    }

    public void GoToNextFile(MainViewModel mv)
    {
        if (mv.Files is null || mv.Files.Count == 0)
            return;
        int newIndex = mv.CurrentFileIndex + 1;
        mv.CurrentFileIndex = newIndex >= mv.Files.Count ? 0 : newIndex;
    }
}