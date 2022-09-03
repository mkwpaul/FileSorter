using AdonisUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Common;

public static class MessageBuilder
{
    public static MessageBoxModel<TEnum> CreateCustom<TEnum>() where TEnum : Enum
    {
        return new MessageBoxModel<TEnum>()
        {
            Buttons = new List<IMessageBoxButtonModel>(),
        };
    }

    public static MessageBoxModel<TEnum> AddAnswer<TEnum>(this MessageBoxModel<TEnum> model, TEnum answer, string? caption = null) where TEnum : Enum
    {
        caption ??= answer.ToString();

        var buttons = (List<IMessageBoxButtonModel>)model.Buttons;
        var button = MessageBoxButtons.Custom(caption, answer);

        buttons.Add(button);
        return model;
    }

    public static MessageBoxModel<BooleanResult> CreateYesNo()
    {
        var model = new MessageBoxModel<BooleanResult>()
        {
            Buttons = new IMessageBoxButtonModel[]
            {
             MessageBoxButtons.Custom("Yes", BooleanResult.Yes),
             MessageBoxButtons.Custom("No", BooleanResult.No),
            },
            Icon = MessageBoxImage.Question,
        };
        model.SetDefault(BooleanResult.Yes);
        return model;
    }

    public static MessageBoxModel<BooleanResult> CreateOkCancel()
    {
        var model = new MessageBoxModel<BooleanResult>()
        {
            Buttons = new IMessageBoxButtonModel[]
            {
             MessageBoxButtons.Custom("Ok", BooleanResult.Yes),
             MessageBoxButtons.Custom("Cancel", BooleanResult.No),
            },
        };

        model.SetDefault(BooleanResult.Yes);
        return model;
    }

    [Obsolete]
    public static MessageBoxModel SetDefault(this MessageBoxModel model, ValueType id)
    {
        var defaultBtn = model.Buttons.FirstOrDefault(x => x.Id.Equals(id));
        if (defaultBtn != null)
            defaultBtn.IsDefault = true;

        return model;
    }

    public static MessageBoxModel<T> SetDefault<T>(this MessageBoxModel<T> model, T id)
    {
        var defaultBtn = model.Buttons.FirstOrDefault(x => x.Id.Equals(id));
        if (defaultBtn != null)
            defaultBtn.IsDefault = true;

        return model;
    }

    public static MessageBoxModel SetText(this MessageBoxModel model, string text)
    {
        model.Text = text;
        return model;
    }

    public static MessageBoxModel<T> SetText<T>(this MessageBoxModel<T> model, string text)
    {
        model.Text = text;
        return model;
    }

    public static MessageBoxModel SetIcon(this MessageBoxModel model, MessageBoxImage image)
    {
        model.Icon = image;
        return model;
    }

    public static MessageBoxModel<T> SetIcon<T>(this MessageBoxModel<T> model, MessageBoxImage image)
    {
        model.Icon = image;
        return model;
    }

    public static MessageBoxModel SetCaption(this MessageBoxModel model, string caption)
    {
        model.Caption = caption;
        return model;
    }

    public static MessageBoxModel<T> SetCaption<T>(this MessageBoxModel<T> model, string caption)
    {
        model.Caption = caption;
        return model;
    }
}
