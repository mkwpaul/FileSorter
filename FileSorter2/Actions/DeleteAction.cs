using FileSorter.Services;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace FileSorter.Actions;

public interface IReversibleAction
{
    string DisplayName { get; }
    void Act();

    void Revert();
}

public class DeleteAction : IReversibleAction
{
    readonly Action<string>? _notifyAct;
    readonly Action<string>? _notifyRevert;
    readonly string oldPath;
    readonly IRecycleBin _recycleBin;

    IDisposable? deletedFile;

    public DeleteAction(string path, IRecycleBin recycleBin, Action<string>? notifyAct, Action<string>? notifyRevert)
    {
        oldPath = path ?? throw new ArgumentNullException(nameof(path));
        _recycleBin = recycleBin ?? throw new ArgumentNullException(nameof(recycleBin));

        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);

        _notifyAct = notifyAct;
        _notifyRevert = notifyRevert;
    }

    public string DisplayName => string.Format("Delete {0}", Path.GetFileName(oldPath));

    public void Act()
    {
        if (deletedFile == null)
            deletedFile = _recycleBin.Send(oldPath);

        _notifyAct?.Invoke(oldPath);
    }

    public void Revert()
    {
        deletedFile?.Dispose();

        deletedFile = null;

        _notifyRevert?.Invoke(oldPath);
    }
}


public class MoveAction : IReversibleAction
{
    readonly string oldDestination;
    readonly string newDestination;
    readonly Action<string, string>? notifyAct;
    readonly Action<string, string>? notifyRevert;

    public MoveAction(string file, string toFolder, Action<string, string>? notifyAct, Action<string, string>? notifyRevert)
    {
        if (!File.Exists(file)) throw new FileNotFoundException(null, file);
        if (!Directory.Exists(toFolder))
            throw new DirectoryNotFoundException();

        this.notifyAct = notifyAct;
        this.notifyRevert = notifyRevert;

        // ensure absolute paths, there are weird windows path limit bugs
        file = Path.GetFullPath(file);
        toFolder = Path.GetFullPath(toFolder);

        oldDestination = file;
        newDestination = Path.Combine(toFolder, Path.GetFileName(file));
    }

    public string DisplayName => string.Format("Move {0} to {1}", oldDestination, newDestination);

    public void Act()
    {
        File.Move(oldDestination, newDestination);
        notifyAct?.Invoke(oldDestination, newDestination);
    }

    public void Revert()
    {
        File.Move(newDestination, oldDestination);
        notifyRevert?.Invoke(newDestination, oldDestination);
    }
}