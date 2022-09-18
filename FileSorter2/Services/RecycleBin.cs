using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Shell32;


namespace FileSorter.Services;

public interface IRecycleBin
{
    /// <summary>
    ///     Sends the file or folder at the given path to the recycle bin.
    /// </summary>
    /// <param name="path">The path to the file or folder that should be send to the recycle bin.</param>
    /// <param name="confirmationNeeded">Should ask for user confirmation before sending to recycle bin.</param>
    /// <returns>The <see cref="IDisposable" /> will, when disposed, restore the file.</returns>
    /// <remarks>
    ///     The <see cref="IDisposable" /> handle to the deleted file will throw a
    ///     <see cref="FileRestorationNotPossibleException" />
    ///     when it cannot restore the file.
    /// </remarks>
    IDisposable Send(string path, bool confirmationNeeded = false);
}


class ActionDisposable<T> : IDisposable
{
    public T? Arg { get; init; }
    public Action<T?>? Action { get; init; }

    public void Dispose()
    {
        Action?.Invoke(Arg);
    }
}

class RecycleBin : IRecycleBin
{
    private readonly Shell shell = new Shell();

    public IDisposable Send(string path, bool confirmationNeeded = false)
    {
        var success = false;

        if (confirmationNeeded)
            success = FileOperationApiWrapper.Send(path,
                FileOperationFlags.AllowUndo | FileOperationFlags.EnableNukeWarning);
        else
            success = FileOperationApiWrapper.Send(path,
                FileOperationFlags.AllowUndo | FileOperationFlags.NoConfirmation | FileOperationFlags.EnableNukeWarning);

        if (!success) 
            throw new IOException($"Could not delete {Path.GetFileName(path)}");

        return new ActionDisposable<string> { Arg = path, Action = RestoreFileFromRecycleBin };
    }



    private void RestoreFileFromRecycleBin(string path)
    {
        var recycler = shell.NameSpace(10);

        foreach (FolderItem item in recycler.Items())
        {
            var fileName = recycler.GetDetailsOf(item, 0);

            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                fileName += Path.GetExtension(item.Path);

            var filePath = recycler.GetDetailsOf(item, 1);

            if (PathEquals(path, Path.Combine(filePath, fileName)))
            {
                Restore(item);
                return;
            }
        }

        throw new FileNotFoundException(null, path);
    }

    public static bool PathEquals(string path1, string path2)
    {
        return Path.GetFullPath(path1).Equals(Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase);
    }

    private void Restore(FolderItem item)
    {
        var itemVerbs = item.Verbs();

        itemVerbs.Item(0).DoIt();
    }
}

/// <summary>
///     Possible flags for the SHFileOperation method.
/// </summary>
[Flags]
public enum FileOperationFlags : ushort
{
    /// <summary>
    ///     Do not show a dialog during the process
    /// </summary>
    Silent = 0x0004,

    /// <summary>
    ///     Do not ask the user to confirm selection
    /// </summary>
    NoConfirmation = 0x0010,

    /// <summary>
    ///     Delete the file to the recycle bin.  (Required flag to send a file to the bin
    /// </summary>
    AllowUndo = 0x0040,

    /// <summary>
    ///     Do not show the names of the files or folders that are being recycled.
    /// </summary>
    SimpleProgress = 0x0100,

    /// <summary>
    ///     Surpress errors, if any occur during the process.
    /// </summary>
    NoErrorUi = 0x0400,

    /// <summary>
    ///     Warn if files are too big to fit in the recycle bin and will need
    ///     to be deleted completely.
    /// </summary>
    EnableNukeWarning = 0x4000
}

/// <summary>
///     File Operation Function Type for SHFileOperation
/// </summary>
public enum FileOperationType : uint
{
    /// <summary>
    ///     Move the objects
    /// </summary>
    Move = 0x0001,

    /// <summary>
    ///     Copy the objects
    /// </summary>
    Copy = 0x0002,

    /// <summary>
    ///     Delete (or recycle) the objects
    /// </summary>
    Delete = 0x0003,

    /// <summary>
    ///     Rename the object(s)
    /// </summary>
    Rename = 0x0004
}

public static class FileOperationApiWrapper
{

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

    /// <summary>
    ///     Send file to recycle bin
    /// </summary>
    /// <param name="path">Location of directory or file to recycle</param>
    /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
    public static bool Send(string path, FileOperationFlags flags)
    {
        try
        {
            var fs = new SHFILEOPSTRUCT
            {
                wFunc = FileOperationType.Delete,
                pFrom = path + '\0' + '\0',
                fFlags = FileOperationFlags.AllowUndo | flags
            };
            SHFileOperation(ref fs);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
    /// </summary>
    /// <param name="path">Location of directory or file to recycle</param>
    public static bool Send(string path)
    {
        return Send(path, FileOperationFlags.NoConfirmation | FileOperationFlags.EnableNukeWarning);
    }

    /// <summary>
    ///     Send file silently to recycle bin.  Surpress dialog, surpress errors, delete if too large.
    /// </summary>
    /// <param name="path">Location of directory or file to recycle</param>
    public static bool MoveToRecycleBin(string path)
    {
        return Send(path,
            FileOperationFlags.NoConfirmation | FileOperationFlags.NoErrorUi |
            FileOperationFlags.Silent);
    }

    static bool deleteFile(string path, FileOperationFlags flags)
    {
        try
        {
            var fs = new SHFILEOPSTRUCT
            {
                wFunc = FileOperationType.Delete,
                pFrom = path + '\0' + '\0',
                fFlags = flags
            };
            SHFileOperation(ref fs);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool DeleteCompletelySilent(string path)
    {
        return deleteFile(path,
            FileOperationFlags.NoConfirmation | FileOperationFlags.NoErrorUi |
            FileOperationFlags.Silent);
    }

    /// <summary>
    ///     SHFILEOPSTRUCT for SHFileOperation from COM
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEOPSTRUCT
    {
        public readonly IntPtr hwnd;

        [MarshalAs(UnmanagedType.U4)] public FileOperationType wFunc;

        public string pFrom;
        public readonly string pTo;
        public FileOperationFlags fFlags;

        [MarshalAs(UnmanagedType.Bool)] public readonly bool fAnyOperationsAborted;

        public readonly IntPtr hNameMappings;
        public readonly string lpszProgressTitle;
    }
}