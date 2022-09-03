using AdonisUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Common;

public interface IMessageBoxModel<T> : IMessageBoxModel
{
}

public class MessageBoxModel<T> : MessageBoxModel, IMessageBoxModel<T>
{
}

/// <summary>
/// Default Implementation that will actually Show MessageBoxes
/// </summary>
public class UserInteraction : IUserInteraction
{
    public void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }

    public void ShowMessage(IMessageBoxModel model)
    {
        MessageBox.Show(model);
    }

    public MessageBoxResult Show(IMessageBoxModel model)
    {
        return MessageBox.Show(model);
    }

    public T Show<T>(IMessageBoxModel<T> model) where T : unmanaged, Enum
    {
        var result = MessageBox.Show(model);
        if (result != MessageBoxResult.Custom)
            throw new InvalidOperationException();

        var result2 = model.ButtonPressed?.Id ?? model.Buttons.FirstOrDefault(x => x.IsDefault);

        int val = (int)result2;
        return val.ToEnum<T>();
    }

    public bool Show(IMessageBoxModel<BooleanResult> model)
    {
        var result = Show<BooleanResult>(model);

        return result == BooleanResult.Yes;
    }
}

public static class MessageBoxExtensions
{
    public static void ShowMessage(this IMessageBoxModel model, IUserInteraction service)
    {
        service.ShowMessage(model);
    }

    public static MessageBoxResult Show(this IMessageBoxModel model, IUserInteraction service)
    {
        return service.Show(model);
    }

    public static bool Show(this IMessageBoxModel<BooleanResult> model, IUserInteraction service)
    {
        return service.Show(model);
    }

    public static T Show<T>(this IMessageBoxModel<T> model, IUserInteraction service) where T : unmanaged, Enum
    {
        return model.Show(service);
    }
}

public static class EnumExtensions
{
    public static TEnum ToEnum<TEnum>(this int val) where TEnum : unmanaged, Enum
    {
        return From<int, TEnum>(val);
    }

    public unsafe static TResult To<TResult, TEnum>(this TEnum value) where TResult : unmanaged where TEnum : unmanaged, Enum
    {
        VerifyUnderlyingType<TResult, TEnum>();
        if (sizeof(TResult) != sizeof(TEnum))
            throw new InvalidCastException();

        return *(TResult*)&value;
    }

    public unsafe static TEnum From<TSource, TEnum>(this TSource value) where TSource : unmanaged where TEnum : unmanaged, Enum
    {
        VerifyUnderlyingType<TSource, TEnum>();
        if (sizeof(TEnum) != sizeof(TSource))
            throw new InvalidCastException();

        return *(TEnum*)&value;
    }

    [Conditional("DEBUG")]
    static void VerifyUnderlyingType<TOther, TEnum>() where TOther : unmanaged where TEnum : unmanaged, Enum
    {
        if (typeof(TEnum).GetEnumUnderlyingType() != typeof(TOther))
            throw new InvalidCastException();
    }
}