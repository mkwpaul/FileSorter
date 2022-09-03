﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPF.Common.MarkupExtensions;

/// <summary>
/// A base class for custom markup extension which provides properties
/// that can be found on regular <see cref="Binding"/> markup extension.<br/>
/// See: http://www.hardcodet.net/2008/04/wpf-custom-binding-class
/// </summary>
[MarkupExtensionReturnType(typeof(object))]
public abstract class CustomBindingBase : MarkupExtension
{
    protected CustomBindingBase() { }

    protected CustomBindingBase(string path)
    {
        Path = new PropertyPath(path);
    }

    protected readonly Binding Binding = new();

    #region properties

    [DefaultValue(null)]
    public object? AsyncState
    {
        get => Binding.AsyncState;
        set => Binding.AsyncState = value;
    }

    [DefaultValue(false)]
    public bool BindsDirectlyToSource
    {
        get => Binding.BindsDirectlyToSource;
        set => Binding.BindsDirectlyToSource = value;
    }

    [DefaultValue(null)]
    public virtual IValueConverter? Converter
    {
        get => Binding.Converter;
        set => Binding.Converter = value;
    }

    [DefaultValue(null)]
    public object? TargetNullValue
    {
        get => Binding.TargetNullValue;
        set => Binding.TargetNullValue = value;
    }

    [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter)), DefaultValue(null)]
    public CultureInfo ConverterCulture
    {
        get => Binding.ConverterCulture;
        set => Binding.ConverterCulture = value;
    }

    [DefaultValue(null)]
    public virtual object? ConverterParameter
    {
        get => Binding.ConverterParameter;
        set => Binding.ConverterParameter = value;
    }

    [DefaultValue(null)]
    public string? ElementName
    {
        get => Binding.ElementName;
        set => Binding.ElementName = value;
    }

    [DefaultValue(null)]
    public object? FallbackValue
    {
        get => Binding.FallbackValue;
        set => Binding.FallbackValue = value;
    }

    [DefaultValue(false)]
    public bool IsAsync
    {
        get => Binding.IsAsync;
        set => Binding.IsAsync = value;
    }

    [DefaultValue(BindingMode.Default)]
    public BindingMode Mode
    {
        get => Binding.Mode;
        set => Binding.Mode = value;
    }

    [DefaultValue(false)]
    public bool NotifyOnSourceUpdated
    {
        get => Binding.NotifyOnSourceUpdated;
        set => Binding.NotifyOnSourceUpdated = value;
    }

    [DefaultValue(false)]
    public bool NotifyOnTargetUpdated
    {
        get => Binding.NotifyOnTargetUpdated;
        set => Binding.NotifyOnTargetUpdated = value;
    }

    [DefaultValue(false)]
    public bool NotifyOnValidationError
    {
        get => Binding.NotifyOnValidationError;
        set => Binding.NotifyOnValidationError = value;
    }

    [DefaultValue(null)]
    public virtual PropertyPath? Path
    {
        get => Binding.Path;
        set => Binding.Path = value;
    }

    [DefaultValue(null)]
    public virtual RelativeSource? RelativeSource
    {
        get => Binding.RelativeSource;
        set => Binding.RelativeSource = value;
    }

    [DefaultValue(null)]
    public virtual object? Source
    {
        get => Binding.Source;
        set => Binding.Source = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
    {
        get => Binding.UpdateSourceExceptionFilter;
        set => Binding.UpdateSourceExceptionFilter = value;
    }

    [DefaultValue(UpdateSourceTrigger.Default)]
    public UpdateSourceTrigger UpdateSourceTrigger
    {
        get => Binding.UpdateSourceTrigger;
        set => Binding.UpdateSourceTrigger = value;
    }

    [DefaultValue(false)]
    public bool ValidatesOnDataErrors
    {
        get => Binding.ValidatesOnDataErrors;
        set => Binding.ValidatesOnDataErrors = value;
    }

    [DefaultValue(false)]
    public bool ValidatesOnExceptions
    {
        get => Binding.ValidatesOnExceptions;
        set => Binding.ValidatesOnExceptions = value;
    }

    [DefaultValue(null)]
    public string? XPath
    {
        get => Binding.XPath;
        set => Binding.XPath = value;
    }

    [DefaultValue(null)]
    public Collection<ValidationRule> ValidationRules => Binding.ValidationRules;

    [DefaultValue(null)]
    public string? StringFormat
    {
        get => Binding.StringFormat;
        set => Binding.StringFormat = value;
    }

    [DefaultValue("")]
    public string BindingGroupName
    {
        get => Binding.BindingGroupName;
        set => Binding.BindingGroupName = value;
    }

    #endregion

    /// <summary>
    /// This basic implementation just sets a binding on the targeted
    /// <see cref="DependencyObject"/> and returns the appropriate
    /// <see cref="BindingExpressionBase"/> instance.<br/>
    /// All this work is delegated to the decorated <see cref="Binding"/>
    /// instance.
    /// </summary>
    /// <returns>
    /// The object value to set on the property where the extension is applied. 
    /// In case of a valid binding expression, this is a <see cref="BindingExpressionBase"/>
    /// instance.
    /// </returns>
    /// <param name="provider">Object that can provide services for the markup
    /// extension.</param>
    public override object ProvideValue(IServiceProvider provider)
    {
        //create a binding and associate it with the target
        return Binding.ProvideValue(provider);
    }

    /// <summary>
    /// Validates a service provider that was submitted to the <see cref="ProvideValue"/>
    /// method. This method checks whether the provider is null (happens at design time),
    /// whether it provides an <see cref="IProvideValueTarget"/> service, and whether
    /// the service's <see cref="IProvideValueTarget.TargetObject"/> and
    /// <see cref="IProvideValueTarget.TargetProperty"/> properties are valid
    /// <see cref="DependencyObject"/> and <see cref="DependencyProperty"/>
    /// instances.
    /// </summary>
    /// <param name="provider">The provider to be validated.</param>
    /// <param name="target">The binding target of the binding.</param>
    /// <param name="dp">The target property of the binding.</param>
    /// <returns>True if the provider supports all that's needed.</returns>
    protected virtual bool TryGetTargetItems(IServiceProvider provider, out DependencyObject? target, out DependencyProperty dp)
    {
        target = null;
        dp = null;
        if (provider == null) return false;

        //create a binding and assign it to the target
        IProvideValueTarget service = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
        if (service == null) return false;

        //we need dependency objects / properties
        target = service.TargetObject as DependencyObject;
        dp = service.TargetProperty as DependencyProperty;
        return target != null && dp != null;
    }
}
