using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterMessageBox.Contracts;

/// <summary>
/// Exposes properties used to configure the appearance and behavior of a <see cref="MessageBoxWindow"/>.
/// </summary>
public interface IMessageBoxModel<T>
{
    /// <summary>
    /// A <see cref="string"/> that specifies the text to display.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// A <see cref="string"/> that specifies the title bar caption to display.
    /// </summary>
    string Caption { get; }

    /// <summary>
    /// A collection of <see cref="IMessageBoxButtonModel"/> that specifies which buttons to display.
    /// </summary>
    IEnumerable<IButtonModel<T>> Buttons { get; }

    /// <summary>
    /// A collection of <see cref="IMessageBoxCheckBoxModel"/> that specifies which check boxes to display.
    /// </summary>
    IEnumerable<IMessageBoxCheckBoxModel> CheckBoxes { get; }

    /// <summary>
    /// A <see cref="MessageBoxImage"/> value that specifies the icon to display.
    /// </summary>
    MessageBoxImage Icon { get; }

    /// <summary>
    /// A <see cref="T"/> value that specifies the result the message box button that was clicked by the user returned.
    /// </summary>
    T Result { get; set; }

    /// <summary>
    /// An <see cref="IMessageBoxButtonModel"/> that specifies which message box button is clicked by the user.
    /// </summary>
    IButtonModel<T> ButtonPressed { get; set; }

    /// <summary>
    /// Specifies whether a system sound is played when the message box window opens. Which sound is played depends on the <see cref="Icon"/>.
    /// </summary>
    bool IsSoundEnabled { get; }
}


