using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Common;
using WPF.Common.Controls;

namespace FileSorter
{
    internal static class MainModule
    {
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
            return;
        }

        public static async Task LoadSettings(this MainViewModel mv)
        {
            var settings = await SettingsReader.GetSettingsFromFile();
            if (settings != null)
            {
                mv.Settings = settings;
                mv.SourceFolder = settings.SourceFolder;
                mv.TargetFoldersFolder = settings.TargetFoldersFolder;
            }
            else
            {
                mv.Settings = new Settings();
            }
        }

        public static Task WriteSettings(this MainViewModel mv)
        {
            return SettingsReader.SafeToDisk(mv.Settings);
        }



        public static void ReadTargetFoldersFolder(this MainViewModel mv) => mv.ReadTargetFoldersFolder(mv.TargetFoldersFolder);
        public static void ReadTargetFoldersFolder(this MainViewModel mv, string? path)
        {
            if (path == null)
            {
                mv.TargetFolders = null;
                return;
            }
            try
            {
                mv.Exception = null;
                mv.TargetFolders = Directory.GetDirectories(path)
                    .Select(x => Path.GetRelativePath(path, x))
                    .OrderBy(x => x.Length)
                    .ToObservable();
            }
            catch (Exception ex)
            {
                mv.Exception = ex;
                mv.Logs.Add(ex.Log());
            }
        }

        public static void ReadSourceFolder(this MainViewModel mv) => mv.ReadSourceFolder(mv.SourceFolder);
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
                mv.Exception = null;
                mv.Files = Directory.GetFiles(sourceFolder)
                    .Select(f => new FileInfo(f))
                    .OrderBy(x => x.Name, StringComparer.Ordinal)
                    .ToObservable();
                mv.CurrentFile = mv.Files.FirstOrDefault();
            }
            catch (Exception ex)
            {
                mv.Exception = ex;
                mv.Logs.Add(ex.Log());
            }
        }


    }
}