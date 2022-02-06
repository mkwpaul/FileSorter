using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WPF.Common;
using WPF.Common.Commands;

namespace FileSorter
{
    public class Commands : PropertyChangedNotifier
    {
        public MainViewModel Mv { get; }

        public RelayCommand GoToNextCommand { get; }
        public RelayCommand GoToPreviousCommand { get; }
        public RelayCommand<FileInfo> DeleteFileCommand { get; }
        public RelayCommand<FileInfo> OpenInExplorerCommand { get; }
        public RelayCommand MoveToTargetFolderCommand { get; }
        public RelayCommand SelectFirstFolderCommand { get; }
        public RelayCommand CreateNewFolderFromSearchCommand { get; }
        public RelayCommand? OnEnterCommand { get; }

        public Commands(MainViewModel mv)
        {
            Mv = mv;

            GoToNextCommand = new RelayCommand(GoToNextFile);
            GoToPreviousCommand = new RelayCommand(GoToPreviousFile);
            DeleteFileCommand = new RelayCommand<FileInfo>(DeleteFile);
            OpenInExplorerCommand = new RelayCommand<FileInfo>(OpenInExplorer);
            MoveToTargetFolderCommand = new RelayCommand(MoveToTargetFolder);
            SelectFirstFolderCommand = new RelayCommand(SelectFirstFolder);
            OnEnterCommand = new RelayCommand(OnEnter);
            CreateNewFolderFromSearchCommand = new RelayCommand(CreateNewFolderFromSearch);
        }

        private void CreateNewFolderFromSearch()
        {
            if (string.IsNullOrWhiteSpace(Mv.SearchText))
                return;
            if (Mv.Settings.SourceFolder is null)
                return;
            if (Mv.Settings.TargetFoldersFolder is null)
                return;

            var newFolder = Mv.SearchText.EscapeFileName();
            var newFolderFull = Path.Combine(Mv.Settings.TargetFoldersFolder, newFolder);

            if (!DoesUserWantTo.CreateFolder(newFolderFull))
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

                if (DoesUserWantTo.MoveFileToNewFolder())
                    MoveToTargetFolder();
            }
        }

        private void OnEnter()
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

        private void MoveToTargetFolder()
        {
            if (Mv.CurrentFile is null)
                return;
            if (Mv.CurrentTargetFolder is null)
                return;

            var currentTarget = Mv.CurrentTargetFolder;
            var newFullPath = Path.Combine(currentTarget.FullName, Mv.CurrentFile.Name);
            var file = Mv.CurrentFile;

            if (File.Exists(newFullPath))
            {
                var answer = DoesUserWantTo.ReactToFileCollision(newFullPath);
                switch (answer)
                {
                    case FileCollisionReaction.Overwrite:
                        break;
                    case FileCollisionReaction.Delete:
                        DeleteFile(file, true);
                        return;
                    case FileCollisionReaction.Cancel:
                        return;
                }
            }

            try
            {
                File.Move(file.FullName, newFullPath, true);
                Mv.RemoveCurrentFile();
                Mv.Logs.Log($"Moved {file.Name} from {file.Directory} to {currentTarget}");
            }
            catch (IOException ex)
            {
                Mv.Logs.Log(ex);
            }
        }

        private void DeleteFile(FileInfo file) => DeleteFile(file, false);
        private void DeleteFile(FileInfo file, bool skipDialog)
        {
            if (file is null)
                return;

            if (!skipDialog && Mv.Settings.AskBeforeFileDeletion && DoesUserWantTo.DeleteFile())
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

        private void OpenInExplorer(FileInfo file)
        {
            if (file is null)
                return;

            if (!File.Exists(file.FullName))
                return;

            string argument = $"/select, \"{file.FullName}\"";
            Process.Start("explorer.exe", argument);
        }

        private void GoToPreviousFile()
        {
            if (Mv.Files is null || Mv.Files.Count == 0)
                return;
            int newIndex = Mv.CurrentFileIndex - 1;
            Mv.CurrentFileIndex = newIndex < 0 ? Mv.Files.Count - 1 : newIndex;
        }

        private void GoToNextFile()
        {
            if (Mv.Files is null || Mv.Files.Count == 0)
                return;
            int newIndex = Mv.CurrentFileIndex + 1;
            Mv.CurrentFileIndex = newIndex >= Mv.Files.Count ? 0 : newIndex;
        }
    }
}
