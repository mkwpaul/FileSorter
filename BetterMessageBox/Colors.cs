using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;

namespace BetterMessageBox;


[DefaultProperty(nameof(Val))]
[MarkupExtensionReturnType(typeof(Brush))]

public class Color : MarkupExtension
{
    public KnownColor Val { get; set; }

    public Color() { }

    public Color(KnownColor val)
    {
        Val = val;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider))!;
        var dictionary = rootObjectProvider?.RootObject as IDictionary;
        return dictionary?[Val];
    }
}

public enum KnownColor
{
    ForegroundColor,
    AccentColor,
    AccentHighlightColor,
    AccentForegroundColor,
    AccentIntenseHighlightColor,
    AccentIntenseHighlightBorderColor,
    AccentInteractionColor,
    AccentInteractionBorderColor,
    AccentInteractionForegroundColor,
    Layer0BackgroundColor,
    Layer0BorderColor,
    Layer1BackgroundColor,
    Layer1BorderColor,
    Layer1HighlightColor,
    Layer1HighlightBorderColor,
    Layer1IntenseHighlightColor,
    Layer1IntenseHighlightBorderColor,
    Layer1InteractionColor,
    Layer1InteractionBorderColor,
    Layer1InteractionForegroundColor,
    Layer2BackgroundColor,
    Layer2BorderColor,
    Layer2HighlightColor,
    Layer2HighlightBorderColor,
    Layer2IntenseHighlightColor,
    Layer2IntenseHighlightBorderColor,
    Layer2InteractionColor,
    Layer2InteractionBorderColor,
    Layer2InteractionForegroundColor,
    Layer3BackgroundColor,
    Layer3BorderColor,
    Layer3HighlightColor,
    Layer3HighlightBorderColor,
    Layer3IntenseHighlightColor,
    Layer3IntenseHighlightBorderColor,
    Layer3InteractionColor,
    Layer3InteractionBorderColor,
    Layer3InteractionForegroundColor,
    Layer4BackgroundColor,
    Layer4BorderColor,
    Layer4HighlightColor,
    Layer4HighlightBorderColor,
    Layer4IntenseHighlightColor,
    Layer4IntenseHighlightBorderColor,
    Layer4InteractionColor,
    Layer4InteractionBorderColor,
    Layer4InteractionForegroundColor,
    DisabledForegroundColor,
    DisabledAccentForegroundColor,
    SuccessColor,
    ErrorColor,
    AlertColor,
    HyperlinkColor,
    WindowButtonHighlightColor,
    WindowButtonInteractionColor,
}
