using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterMessageBox.Contracts;

namespace BetterMessageBox;

/// <summary>
/// The default implementation of <see cref="IMessageBoxCheckBoxModel"/> used to configure the appearance and behavior of a single check box of some <see cref="MessageBoxWindow"/>.
/// </summary>
public class MessageBoxCheckBoxModel : PropertyChangedBase, IMessageBoxCheckBoxModel
{
    private object _id;
    private string _label;
    private bool _isChecked;
    private MessageBoxCheckBoxPlacement _placement;

    /// <summary>
    /// An <see cref="object"/> that can be used to identify the check box.
    /// </summary>
    public object Id
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
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    /// <inheritdoc/>
    public MessageBoxCheckBoxPlacement Placement
    {
        get => _placement;
        set => SetProperty(ref _placement, value);
    }

    /// <summary>
    /// Creates an instance of <see cref="MessageBoxCheckBoxModel"/>.
    /// </summary>
    /// <param name="label">A <see cref="string"/> that specifies the content of the check box.</param>
    public MessageBoxCheckBoxModel(string label)
    {
        _label = label;
    }
}
