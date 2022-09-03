using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterMessageBox;

public enum MessageBoxImage
{
    None,
    Hand,
    Stop,
    Error,
    Question,
    Exclamation,
    Warning,
    Asterisk,
    Information,
}

/// <summary>
/// Specifies where a check box is placed inside a message box.
/// </summary>
public enum MessageBoxCheckBoxPlacement
{
    /// <summary>
    /// The check box is placed below the message box's text.
    /// </summary>
    BelowText,

    /// <summary>
    /// The check box is placed next to the message box's buttons inside the button row.
    /// </summary>
    NextToButtons,
}

/// <summary>
/// Specifies the buttons that are displayed on a message box.
/// </summary>
public enum MessageBoxButton
{
    /// <summary>
    /// The message box displays an OK button.
    /// </summary>
    OK = 0,

    /// <summary>
    /// The message box displays OK and Cancel buttons.
    /// </summary>
    OKCancel = 1,

    /// <summary>
    /// The message box displays Yes, No and Cancel buttons.
    /// </summary>
    YesNoCancel = 3,

    /// <summary>
    /// The message box displays Yes and No buttons.
    /// </summary>
    YesNo = 4,
}

public enum MessageBoxResult
{
    None,
    OK,
    Cancel,
    Yes,
    No,
    Custom,
}
