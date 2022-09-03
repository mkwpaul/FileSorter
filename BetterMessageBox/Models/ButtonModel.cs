using BetterMessageBox.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterMessageBox;

public class ResultButtonModel : ButtonModel<MessageBoxResult>, IResultButtonModel
{
    /// <inheritdoc/>
    public ResultButtonModel(string label, MessageBoxResult causedResult) : base(label, causedResult)
    {
    }
}

/// <summary>
/// The default implementation of <see cref="IMessageBoxButtonModel"/> used to configure the appearance and behavior of a single button of some <see cref="MessageBoxWindow"/>.
/// </summary>
public class ButtonModel<T> : PropertyChangedBase, IButtonModel<T>
{
    private T _id;
    private string _label;
    private MessageBoxResult _causedResult;
    private bool _isDefault;
    private bool _isCancel;

    /// <inheritdoc/>
    public T Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <inheritdoc/>
    public string Label
    {
        get => _label;
        set => SetProperty(ref _label, value);
    }

    /// <inheritdoc/>
    public MessageBoxResult CausedResult
    {
        get => _causedResult;
        set => SetProperty(ref _causedResult, value);
    }

    /// <inheritdoc/>
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    /// <inheritdoc/>
    public bool IsCancel
    {
        get => _isCancel;
        set => SetProperty(ref _isCancel, value);
    }

    /// <summary>
    /// Creates an instance of <see cref="MessageBoxButtonModel"/>.
    /// </summary>
    /// <param name="label">A <see cref="string"/> that specifies the content of the button.</param>
    /// <param name="causedResult">A <see cref="MessageBoxResult"/> that will be used as <see cref="IMessageBoxModel.Result"/> of the parent message box if the button is pressed.</param>
    public ButtonModel(string label, MessageBoxResult causedResult)
    {
        Label = label;
        CausedResult = causedResult;
    }
}
