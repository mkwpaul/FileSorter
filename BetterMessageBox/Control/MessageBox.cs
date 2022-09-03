using BetterMessageBox.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BetterMessageBox;

/// <summary>
/// Displays a message box.
/// </summary>
public static class MessageBox
{
    /// <summary>
    /// Displays a message box that has a message and that returns a result.
    /// </summary>
    /// <param name="text">A <see cref="String"/> that specifies the text to display.</param>
    /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static MessageBoxResult Show(string text)
    {
        var messageBoxModel = new ResultMessageBoxModel
        {
            Text = text,
        };

        return Show(messageBoxModel);
    }

    /// <summary>
    /// Displays a message box that is configured like specified in the <see cref="IMessageBoxModel"/> and that returns a result.
    /// </summary>
    /// <param name="messageBoxModel">An <see cref="IMessageBoxModel"/> that configures the appearance and behavior of the message box.</param>
    /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static T Show<T>(IMessageBoxModel<T> messageBoxModel)
    {
        var activeWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        if (activeWindow is null)
            throw new InvalidProgramException();

        return Show(activeWindow, messageBoxModel);
    }

    /// <summary>
    /// Displays a message box in front of the specified window. The message box is configured like specified in the <see cref="IMessageBoxModel"/> and returns a result.
    /// </summary>
    /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
    /// <param name="messageBoxModel">An <see cref="IMessageBoxModel"/> that configures the appearance and behavior of the message box.</param>
    /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
    public static T Show<T>(Window owner, IMessageBoxModel<T> messageBoxModel)
    {
        var messageBox = new MessageBoxWindow<T>
        {
            Owner = owner,
            ViewModel = messageBoxModel,
        };

        messageBox.ShowDialog();
        return messageBoxModel.Result;
    }
}
