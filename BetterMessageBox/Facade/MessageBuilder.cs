using BetterMessageBox.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterMessageBox;

public static class MessageBuilder
{
    public static ResultMessageBoxModel CreateYesNo()
    {
        var model = new ResultMessageBoxModel()
        {
            Buttons = MessageBoxButtons.YesNo(),
            Icon = MessageBoxImage.Question,
        };
        return model;
    }

    public static ResultMessageBoxModel CreateOkCancel()
    {
        var model = new ResultMessageBoxModel()
        {
            Buttons = MessageBoxButtons.OkCancel(),
        };

        return model;
    }

    public static MessageBoxModel<T> CreateCustom<T>()
    {
        return new MessageBoxModel<T>()
        {
            Buttons = new List<IButtonModel<T>>(),
        };
    }

    public static MessageBoxModel<T> AddAnswer<T>(this MessageBoxModel<T> model, T answer, string? caption = null)
    {
        caption ??= answer.ToString();

        var buttons = (List<IButtonModel<T>>)model.Buttons;
        var button = MessageBoxButtons.Custom(caption, answer);

        buttons.Add(button);
        return model;
    }

    public static MessageBoxModel<T> SetText<T>(this MessageBoxModel<T> model, string text)
    {
        model.Text = text;
        return model;
    }

    public static MessageBoxModel<T> SetIcon<T>(this MessageBoxModel<T> model, MessageBoxImage image)
    {
        model.Icon = image;
        return model;
    }

    public static MessageBoxModel<T> SetCaption<T>(this MessageBoxModel<T> model, string caption)
    {
        model.Caption = caption;
        return model;
    }
}
