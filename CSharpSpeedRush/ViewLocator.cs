using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CSharpSpeedRush.ViewModels;

namespace CSharpSpeedRush;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Creates the view that matches the given ViewModel by replacing "ViewModel" with "View" in the type name.
    /// Returns a TextBlock with an error message if no matching view is found.
    /// </summary>
    /// <param name="param">The ViewModel instance to find a view for.</param>
    /// <returns>The matching view control, or a TextBlock if not found.</returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Returns true if the given data is a ViewModel, meaning this locator can handle it.
    /// </summary>
    /// <param name="data">The data object to check.</param>
    /// <returns>True if <paramref name="data"/> is a <see cref="ViewModelBase"/>.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
