using Microsoft.VisualBasic.FileIO;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Security;
using WPF.Common;

namespace FileSorter;

public class MainModule
{
    readonly IUserInteraction _doesUserWant;
    readonly ILogger _log;

    public MainModule(ILogger logger, IUserInteraction userInteraction)
    {
        _log = logger.ForContext<MainModule>();
        _doesUserWant = userInteraction;
    }

    #region Settings

    public void AddNewSource(MainViewModel mv)
    {
        var folderSource = new TargetFolderSource();
        folderSource.PropertyChanged += (s, e) =>
        {
            UpdateTargetFoldersFromTargetSources(mv.Settings, mv.State);
        };

        mv.Settings.TargetSources.Add(folderSource);
    }

    public void RemoveSource(IWorld mv, TargetFolderSource folderSource)
    {
        mv.Settings.TargetSources.Remove(folderSource);
        folderSource.ClearNotifications();
        UpdateTargetFoldersFromTargetSources(mv.Settings, mv.State);
    }

    public void UpdateTargetFoldersFromTargetSources(Settings settings, State state)
    {
        _log.Information("Reading TargetFolders from targetFolder sources...");

        var newValues = GetDirectories(settings.TargetSources);
        state.TargetFolders.Clear();
        foreach (var entry in newValues)
            state.TargetFolders.Add(entry);

        List<DirectoryInfo> GetDirectories(IEnumerable<TargetFolderSource> sources)
        {
            // hashset to keep track of entries and check for duplicates.
            var hashset = new HashSet<string>();
            var result = new List<DirectoryInfo>();
            foreach (var source in sources)
                ReadTargetFolderSource(result, hashset, source);

            return result;
        }

        void ReadTargetFolderSource(List<DirectoryInfo> result, HashSet<string> hashset, TargetFolderSource source)
        {
            try
            {
                if (!Directory.Exists(source.Folder))
                    return;

                var folders = (source.FolderType) switch
                {
                    FolderSourceType.IndividualFolder => new string[] { source.Folder },
                    FolderSourceType.SubFolders => Directory.GetDirectories(source.Folder),
                    FolderSourceType.SubFoldersRecursive => Directory.GetDirectories(source.Folder, "", System.IO.SearchOption.AllDirectories),
                };

                folders
                    .Where(hashset.Add)
                    .Select(x => new DirectoryInfo(x))
                    .ForEach(result.Add);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException or IOException)
                    _log.Error(ex, "Error occured reading folder {targetSource}", source.Folder);
                else
                    throw;
            }
        }
    }

    #endregion

    static void RemoveFile(MainViewModel mv, FileInfo? info)
    {
        // Remove From MainViewModel so the file can be moved and isn't blocked by ourselves.
        if (info is null)
            return;

        mv.State.Files?.Remove(info);
        if (info != mv.State.CurrentFile)
            return;

        mv.State.CurrentFileIndex = mv.State.CurrentFileIndex;
        if (mv.State.Files?.Count > 0)
            mv.State.CurrentFile = mv.State.Files?[mv.State.CurrentFileIndex];
        else
            mv.State.CurrentFile = null;
    }

    public void ReadSourceFolder(MainViewModel mv) => ReadSourceFolder(mv, mv.Settings?.SourceFolder);
    public void ReadSourceFolder(MainViewModel mv, string? sourceFolder)
    {

        _log.Information("Reading files from sourceFolder {sourceFolder}...", sourceFolder);
        mv.State.Files.Clear();
        if (sourceFolder == null)
        {
            mv.State.CurrentFile = null;
            return;
        }

        try
        {
            var newEntries = Directory.GetFiles(sourceFolder)
                .Select(f => new FileInfo(f))
                .OrderBy(x => x.Name, StringComparer.Ordinal)
                .ToList();

            _log.Information("Found {fileCount} files", newEntries.Count);


            foreach (var item in newEntries)
                mv.State.Files.Add(item);

            mv.State.CurrentFile = mv.State.Files.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error reading files from sourceFolder {sourceFolder}", sourceFolder);
        }
    }

    public void CreateNewFolderFromSearch(MainViewModel mv)
    {
        if (string.IsNullOrWhiteSpace(mv.State.SearchText))
            return;
        if (mv.Settings.SourceFolder is null)
            return;

        var test = _doesUserWant.SelectFolder(mv.State.SearchText, mv.Settings.TargetSources);
        if (test is BooleanResult.No)
            return;
        if (test is not TargetFolderSource source)
            return;

        var newFolder = mv.State.SearchText.EscapeFileName();
        var newFolderFull = Path.Combine(source.Folder, newFolder);

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
            mv.State.TargetFolders?.Add(newDicInfo);
            mv.State.CurrentTargetFolder = newDicInfo;

            if (_doesUserWant.MoveFileToNewFolder())
                MoveToTargetFolder(mv);
        }
    }

    public void OnEnter(MainViewModel mv)
    {
        if (mv.State.CurrentTargetFolder is null)
            SelectFirstFolder(mv);
        else
            MoveToTargetFolder(mv);
    }

    public void SelectFirstFolder(MainViewModel mv)
    {
        var entry = mv.State.FilteredTargets?.OfType<DirectoryInfo>().FirstOrDefault();
        mv.State.CurrentTargetFolder = entry;
    }

    public void MoveToTargetFolder(MainViewModel mv)
    {
        if (mv.State.CurrentFile is null)
            return;
        if (mv.State.CurrentTargetFolder is null)
            return;

        var currentTarget = mv.State.CurrentTargetFolder;
        var newFullPath = Path.Combine(currentTarget.FullName, mv.State.CurrentFile.Name);
        var file = mv.State.CurrentFile;

        _log.Information("Moving file {file} to {targetFolder}", file.Name, newFullPath);
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
                _log.Information("Moved {file} from {directory} to {currentTarget}", file.Name, file.Directory, currentTarget);
            }
            catch (IOException ex)
            {
                _log.Error(ex, "MoveToTargetFolder Failed.");
            }
        });

        _log.Verbose("Exited MoveToTargetFolder");
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
    void DeleteFile(MainViewModel mv, FileInfo file, bool skipDialog)
    {
        _log.Information("Deleting {file}...", file.Name);
        if (file is null)
            return;

        if (!skipDialog && mv.Settings.AskBeforeFileDeletion && !_doesUserWant.DeleteFile())
            return;

        GoToNextFile(mv);
        Task.Run(() =>
        {
            try
            {
                File.Delete(file.FullName);
                RemoveFile(mv, file);
                _log.Information("Deleted {file}", file.Name);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Delete File Failed");
            }
        });
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
        if (mv.State.Files is null || mv.State.Files.Count == 0)
            return;

        int newIndex = mv.State.CurrentFileIndex - 1;
        mv.State.CurrentFileIndex = newIndex < 0 ? mv.State.Files.Count - 1 : newIndex;
    }

    public void GoToNextFile(IWorld mv)
    {
        if (mv.State.Files is null || mv.State.Files.Count == 0)
            return;
        int newIndex = mv.State.CurrentFileIndex + 1;
        mv.State.CurrentFileIndex = newIndex >= mv.State.Files.Count ? 0 : newIndex;
    }
}