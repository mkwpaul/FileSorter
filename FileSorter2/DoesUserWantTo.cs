using AdonisUI.Controls;
using System.Diagnostics;
using WPF.Common;

namespace FileSorter;

public static class DoesUserWantTo
{
    const string question = "Question";

    public static bool CreateFolder(this IUserInteraction interaction, string newFolderFull)
    {
        return MessageBuilder.CreateYesNo()
                .SetCaption(question)
                .SetText($"Do you want to create a new Folder at: \n\n {newFolderFull}")
                .Show(interaction);
    }

    public static bool MoveFileToNewFolder(this IUserInteraction interaction)
    {
        return MessageBuilder.CreateYesNo()
            .SetCaption(question)
            .SetText("Do you want to move the current file there?")
            .Show(interaction);
    }

    public static bool DeleteFile(this IUserInteraction interaction)
    {
        return MessageBuilder.CreateYesNo()
            .SetText("Are you sure you want to delete?")
            .SetCaption("File Deletion")
            .SetIcon(MessageBoxImage.Exclamation)
            .Show(interaction);
    }

    public static FileCollisionReaction ReactToFileCollision(this IUserInteraction interaction, string newFullPath)
    {
        return MessageBuilder.CreateCustom<FileCollisionReaction>()
            .SetCaption(question)
            .SetText($"A file already exists at {newFullPath}. What do you want to do?")
            .AddAnswer(FileCollisionReaction.Overwrite, "Overwrite file")
            .AddAnswer(FileCollisionReaction.Delete, "Delete current file")
            .AddAnswer(FileCollisionReaction.Cancel, "Cancel")
            .SetDefault(FileCollisionReaction.Overwrite)
            .Show(interaction);
    }
}

public enum FileCollisionReaction
{
    /// <summary>
    /// Cancel the Process of Moving the file, deleting no files.
    /// </summary>
    Cancel,

    /// <summary>
    /// Overwrite the already existing file with the file that's about to move
    /// </summary>
    Overwrite,

    /// <summary>
    /// Keep the existing file at the target destination and delete the file that was supposed to move
    /// </summary>
    Delete,
}
