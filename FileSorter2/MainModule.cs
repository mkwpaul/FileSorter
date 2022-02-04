using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSorter
{
    internal static class MainModule
    {
        public static FileInfo RemoveFile(this MainViewModel mv, FileInfo? info)
        {
            // Remove From MainViewModel so the file can be moved and isn't blocked by ourselves.
            if (info is null)
                return null;
            mv.Files?.Remove(info);

            if (info == mv.CurrentFile)
            {
                mv.CurrentFileIndex = mv.CurrentFileIndex;
                mv.CurrentFile = mv.Files?[mv.CurrentFileIndex];
            }
            return info;
        }

        public static FileInfo RemoveCurrentFile(this MainViewModel mv) => mv.RemoveFile(mv.CurrentFile);
    }
}