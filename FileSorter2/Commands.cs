using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WPF.Common;
using WPF.Common.Commands;

namespace FileSorter;

public class Commands : PropertyChangedNotifier
{
    readonly MainViewModel Mv;
    readonly IUserInteraction doesUserWant;

    public Command GoToNextCommand { get; }
    public Command GoToPreviousCommand { get; }
    public Command DeleteFileCommand { get; }
    public Command OpenInExplorerCommand { get; }
    public Command MoveToTargetFolderCommand { get; }
    public Command SelectFirstFolderCommand { get; }
    public Command CreateNewFolderFromSearchCommand { get; }
    public Command OnEnterCommand { get; }

    public Commands(MainViewModel mv, IUserInteraction userInteraction)
    {
        Mv = mv;
        doesUserWant = userInteraction;
        GoToNextCommand = new RelayCommand(GoToNextFile);
        GoToPreviousCommand = new RelayCommand(GoToPreviousFile);
        DeleteFileCommand = new RelayCommand<FileInfo>(DeleteFile);
        OpenInExplorerCommand = new RelayCommand<FileInfo>(OpenInExplorer);
        MoveToTargetFolderCommand = new RelayCommand(MoveToTargetFolder);
        SelectFirstFolderCommand = new RelayCommand(SelectFirstFolder);
        OnEnterCommand = new RelayCommand(OnEnter);
        CreateNewFolderFromSearchCommand = new RelayCommand(CreateNewFolderFromSearch);
    }


    public void CreateNewFolderFromSearch()
    {
        if (string.IsNullOrWhiteSpace(Mv.SearchText))
            return;
        if (Mv.Settings.SourceFolder is null)
            return;
        if (Mv.Settings.TargetFoldersFolder is null)
            return;

        var newFolder = Mv.SearchText.EscapeFileName();
        var newFolderFull = Path.Combine(Mv.Settings.TargetFoldersFolder, newFolder);

        if (!doesUserWant.CreateFolder(newFolderFull))
            return;
        try
        {
            Directory.CreateDirectory(newFolderFull);
            Mv.Logs.Log($"Created new Folder at {newFolderFull}");
        }
        catch (IOException io)
        {
            Mv.Logs.Log(io, $"Create new Folder at {newFolderFull}");
        }

        if (Directory.Exists(newFolderFull))
        {
            var newDicInfo = new DirectoryInfo(newFolderFull);
            Mv.TargetFolders?.Add(newDicInfo);
            Mv.CurrentTargetFolder = newDicInfo;

            if (doesUserWant.MoveFileToNewFolder())
                MoveToTargetFolder();
        }
    }

    public void OnEnter()
    {
        if (Mv.CurrentTargetFolder is null)
            SelectFirstFolder();
        else
            MoveToTargetFolder();
    }

    public void SelectFirstFolder()
    {
        var entry = Mv.FilteredTargets?.OfType<DirectoryInfo>().FirstOrDefault();
        Mv.CurrentTargetFolder = entry;
    }

    public void MoveToTargetFolder()
    {
        if (Mv.CurrentFile is null)
            return;
        if (Mv.CurrentTargetFolder is null)
            return;

        var currentTarget = Mv.CurrentTargetFolder;
        var newFullPath = Path.Combine(currentTarget.FullName, Mv.CurrentFile.Name);
        var file = Mv.CurrentFile;

        bool cancel = CheckForFileConflict(newFullPath, file);
        if (cancel)
            return;

        GoToNextFile();
        Task.Run(() =>
        {
            try
            {
                File.Move(file.FullName, newFullPath, true);
                Mv.RemoveFile(file);

                Application.Current.Dispatcher.InvokeAsync(() =>
                    Mv.Logs.Log($"Moved {file.Name} from {file.Directory} to {currentTarget}"));
            }
            catch (IOException ex)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                    Mv.Logs.Log(ex));
            }
        });
    }

    bool CheckForFileConflict(string newFullPath, FileInfo file)
    {
        if (!File.Exists(newFullPath))
            return false;

        var answer = doesUserWant.ReactToFileCollision(newFullPath);
        switch (answer)
        {
            case FileCollisionReaction.Overwrite:
                return false;
            case FileCollisionReaction.Delete:
                DeleteFile(file, true);
                return true;
            case FileCollisionReaction.Cancel:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    public void DeleteFile(FileInfo file) => DeleteFile(file, false);
    private void DeleteFile(FileInfo file, bool skipDialog)
    {
        if (file is null)
            return;

        if (!skipDialog && Mv.Settings.AskBeforeFileDeletion && !doesUserWant.DeleteFile())
        {
            return;
        }

        GoToNextFile();
        try
        {
            FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            Mv.RemoveFile(file);
            Mv.Logs.Log($"Deleted {file.Name}");
        }
        catch (Exception ex)
        {
            Mv.Logs.Log(ex);
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

    public void GoToPreviousFile()
    {
        if (Mv.Files is null || Mv.Files.Count == 0)
            return;

        int newIndex = Mv.CurrentFileIndex - 1;
        Mv.CurrentFileIndex = newIndex < 0 ? Mv.Files.Count - 1 : newIndex;
    }

    public void GoToNextFile()
    {
        if (Mv.Files is null || Mv.Files.Count == 0)
            return;
        int newIndex = Mv.CurrentFileIndex + 1;
        Mv.CurrentFileIndex = newIndex >= Mv.Files.Count ? 0 : newIndex;
    }
}
