using BetterMessageBox.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterMessageBox;

// keep interface intentionally plain. 
// Extension Methods can be used to inplement a more userfriendly facade without making every implementer duplicate effort.
public interface IUserInteraction
{
    void ShowMessage(string message);

    void ShowMessage<T>(IMessageBoxModel<T> model);

    T Show<T>(IMessageBoxModel<T> model);

}
