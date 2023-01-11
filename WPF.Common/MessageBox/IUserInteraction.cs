using AdonisUI.Controls;

namespace WPF.Common;

public enum BooleanResult
{
    Yes,
    No,
}

// keep interface intentionally plain. 
// Extension Methods can be used to inplement a more userfriendly facade without making every implementer duplicate effort.
public interface IUserInteraction
{
    void ShowMessage(string message);

    void ShowMessage(IMessageBoxModel model);

    MessageBoxResult Show(IMessageBoxModel model);

    T ShowEnum<T>(IMessageBoxModel<T> model) where T : unmanaged, Enum;

    T Show<T>(IMessageBoxModel<T> model);

    bool Show(IMessageBoxModel<BooleanResult> model);
}
