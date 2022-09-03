using BetterMessageBox.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterMessageBox;

public class ResultMessageBoxModel : MessageBoxModel<MessageBoxResult>
{
}

/// <summary>
/// The default implementation of <see cref="IMessageBoxModel"/> used to configure the appearance and behavior of a <see cref="MessageBoxWindow"/>.
/// </summary>
public class MessageBoxModel<T> : PropertyChangedBase, IMessageBoxModel<T>
{
    private string _text;
    private string _caption;
    private IEnumerable<IButtonModel<T>> _buttons = new List<IButtonModel<T>>();
    private IEnumerable<IMessageBoxCheckBoxModel> _checkBoxes = new List<IMessageBoxCheckBoxModel>();
    private MessageBoxImage _icon;
    private T _result;
    private IButtonModel<T> _buttonPressed;
    private bool _isSoundEnabled = true;

    /// <inheritdoc/>
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    /// <inheritdoc/>
    public string Caption
    {
        get => _caption;
        set => SetProperty(ref _caption, value);
    }

    /// <inheritdoc/>
    public IEnumerable<IButtonModel<T>> Buttons
    {
        get => _buttons;
        set => SetProperty(ref _buttons, value);
    }

    /// <inheritdoc/>
    public IEnumerable<IMessageBoxCheckBoxModel> CheckBoxes
    {
        get => _checkBoxes;
        set => SetProperty(ref _checkBoxes, value);
    }

    /// <inheritdoc/>
    public MessageBoxImage Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    /// <inheritdoc/>
    public T Result
    {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    /// <inheritdoc/>
    public IButtonModel<T> ButtonPressed
    {
        get => _buttonPressed;
        set => SetProperty(ref _buttonPressed, value);
    }

    /// <inheritdoc/>
    public bool IsSoundEnabled
    {
        get => _isSoundEnabled;
        set => SetProperty(ref _isSoundEnabled, value);
    }

    /// <summary>
    /// Sets <see cref="IButtonModel.IsDefault"/> to <see langword="true"/> on the first button that matches the given <paramref name="defaultResult"/>
    /// and to <see langword="false"/> on all other buttons.
    /// </summary>
    /// <param name="defaultResult">The result that matches the default button's <see cref="IButtonModel.CausedResult"/>.</param>
    public void SetDefault(T defaultResult)
    {
        var defaultButton = _buttons.FirstOrDefault(btn => btn.Id.Equals(defaultResult));
        SetDefault(defaultButton);
    }

    /// <summary>
    /// Sets <see cref="IButtonModel.IsDefault"/> to <see langword="true"/> on the given button and to <see langword="false"/> on all other buttons.
    /// </summary>
    /// <param name="defaultButton">The button that is supposed to be the default button. It must be part of the <see cref="Buttons"/> collection.</param>
    public void SetDefault(IButtonModel<T>? defaultButton)
    {
        foreach (var button in _buttons)
            button.IsDefault = false;

        if (defaultButton != null && _buttons.Contains(defaultButton))
            defaultButton.IsDefault = true;
    }
}
