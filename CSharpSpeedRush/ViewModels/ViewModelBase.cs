using CommunityToolkit.Mvvm.ComponentModel;

namespace CSharpSpeedRush.ViewModels;

/// <summary>
/// Abstract base class for all ViewModels in the application.
/// Inherits <see cref="ObservableObject"/> from CommunityToolkit.Mvvm to provide
/// <c>INotifyPropertyChanged</c> support and source-generator integration.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
