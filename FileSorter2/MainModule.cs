using System.ComponentModel;
using System.IO;
using WPF.Common;
using Microsoft.Extensions.Logging;

namespace FileSorter
{
    internal static class MainModule
    {
        public static void SaveToDiskOnChanged(this MainViewModel mv, object? sender, PropertyChangedEventArgs _)
        {
            if (sender is not Settings settings)
                return;

            var task = SettingsReader.SafeToDisk(settings);
            mv.Logs.Log(task, "Saving Settings to Disk", LogLevel.None);
        }

        public static void RemoveCurrentFile(this MainViewModel mv) => mv.RemoveFile(mv.CurrentFile);
        public static void RemoveFile(this MainViewModel mv, FileInfo? info)
        {
            // Remove From MainViewModel so the file can be moved and isn't blocked by ourselves.
            if (info is null)
                return;

            mv.Files?.Remove(info);

            if (info == mv.CurrentFile)
            {
                mv.CurrentFileIndex = mv.CurrentFileIndex;
                if (mv.Files?.Count > 0)
                    mv.CurrentFile = mv.Files?[mv.CurrentFileIndex];
                else
                    mv.CurrentFile = null;
            }
        }

        public static Task WriteSettings(this MainViewModel mv)
        {
            return SettingsReader.SafeToDisk(mv.Settings);
        }

        public static void ReadTargetFoldersFolder(this MainViewModel mv) => mv.ReadTargetFoldersFolder(mv.Settings.TargetFoldersFolder);
        public static void ReadTargetFoldersFolder(this MainViewModel mv, string? path)
        {
            if (path == null)
            {
                mv.TargetFolders = null;
                return;
            }

            try
            {
                mv.TargetFolders = Directory.GetDirectories(path)
                    .Select(x => new DirectoryInfo(x))
                    .OrderBy(x => x.Name.Length)
                    .ToObservable();
            }
            catch (Exception ex)
            {
                mv.Logs.Log(ex);
            }
        }

        public static void ReadSourceFolder(this MainViewModel mv) => mv.ReadSourceFolder(mv.Settings.SourceFolder);
        public static void ReadSourceFolder(this MainViewModel mv, string? sourceFolder)
        {
            if (sourceFolder == null)
            {
                mv.Files = null;
                mv.CurrentFile = null;
                return;
            }

            try
            {
                mv.Files = Directory.GetFiles(sourceFolder)
                    .Select(f => new FileInfo(f))
                    .OrderBy(x => x.Name, StringComparer.Ordinal)
                    .ToObservable();
                mv.CurrentFile = mv.Files.FirstOrDefault();
            }
            catch (Exception ex)
            {
                mv.Logs.Log(ex);
            }
        }

        public static bool Filter(this MainViewModel mv, object o)
        {
            if (o is not DirectoryInfo dicInfo)
                return false;

            if (string.IsNullOrWhiteSpace(mv.SearchText))
                return true;

            return dicInfo.Name.Contains(mv.SearchText, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}