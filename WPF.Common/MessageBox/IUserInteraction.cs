using AdonisUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    T Show<T>(IMessageBoxModel model) where T : unmanaged, Enum;

    bool Show(IMessageBoxModel<BooleanResult> model);
}
