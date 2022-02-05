using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Common.Commands;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using System.Windows;
using WPF.Common;
using Microsoft.WindowsAPICodePack.Shell;
using WPF.Common.Controls;

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
            if (Mv.SourceFolder is null)
                return;

            var newFolder = Mv.SearchText.EscapeFileName();
            var newFolderFull = Path.Combine(Mv.TargetFoldersFolder, newFolder);
            var answer = MessageBox.Show($"Do you want to create a new Folder at: \n\n {newFolderFull}", "Question", MessageBoxButton.YesNo);
            if (answer != MessageBoxResult.Yes)
                return;
            try
            {
                Directory.CreateDirectory(newFolderFull);
            }
            catch (IOException io)
            {
                Mv.Exception = io;
            }

            if (Directory.Exists(newFolderFull))
            {
                Mv.TargetFolders?.Add(newFolder);
                Mv.CurrentTargetFolder = newFolder;
                answer = MessageBox.Show("Do you want to move the current file there?", "Question", MessageBoxButton.YesNo);
                if (answer == MessageBoxResult.Yes)
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
            var entry = Mv.FilteredTargets?.OfType<string>().FirstOrDefault();
            Mv.CurrentTargetFolder = entry;
        }

        private async void MoveToTargetFolder()
        {
            if (Mv.CurrentFile is null)
                return;
            if (string.IsNullOrEmpty(Mv.CurrentTargetFolder))
                return;

            string currentTarget = Mv.CurrentTargetFolder;
            if (string.IsNullOrWhiteSpace(Mv.TargetFoldersFolder))
                return;

            var newFullPath = Path.Combine(Mv.TargetFoldersFolder, currentTarget, Mv.CurrentFile.Name);
            var file = Mv.CurrentFile;

            int tryCount = 0;
            while (tryCount < 3)
            {
                try
                {
                    if (File.Exists(newFullPath))
                    {
                        var answer = MessageBox.Show($"A file alerady exists at {newFullPath}. Do you want to override it?", "Question", MessageBoxButton.YesNoCancel);
                        switch (answer)
                        {
                            case MessageBoxResult.Yes:
                                break;
                            case MessageBoxResult.No:
                                DeleteFile(file);
                                break;
                            case MessageBoxResult.Cancel:
                                return;
                        }
                    }

                    File.Move(file.FullName, newFullPath, true);
                    Mv.RemoveCurrentFile();
                    var log = $"Moved {file.Name} from {file.Directory} to {currentTarget}".LogSuccess();
                    Mv.Logs.Add(log);
                    Mv.Log = log;
                    Mv.Exception = null;
                    return;
                }
                catch (IOException ex)
                {
                    Mv.Exception = ex;
                    Mv.Logs.Add(ex.Log());
                    tryCount++;
                    await Task.Delay(1000);
                }
            }

            bool moveSucces = File.Exists(newFullPath);
            MessageBox.Show($"Move success: {Path.GetFileName(newFullPath)} {moveSucces}");
        }

        private async void DeleteFile(FileInfo file) => DeleteFile(file, false);
        private async void DeleteFile(FileInfo file, bool skipDialog)
        {
            if (file is null)
                return;

            if (!skipDialog && Mv.Settings.AskBeforeFileDeletion)
            {
                var answer = MessageBox.Show("Are you sure you want to delete?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);

                if (answer != MessageBoxResult.Yes)
                    return;
            }

            int tryCount = 0;
            while (tryCount < 10)
            {
                try
                {
                    GoToNextFile();
                    await Task.Delay(1000);
                    FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    Mv.RemoveFile(file);
                    Mv.Exception = null;
                    Mv.Logs.Add($"Deleted {file.Name}".LogSuccess());
                    return;
                }
                catch (Exception ex)
                {
                    Mv.Exception = ex;
                    Mv.Logs.Add(ex.Log());
                    tryCount++;
                }
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
