using Avalonia.Controls;
using CSharpSpeedRush.ViewModels;

namespace CSharpSpeedRush.Views;

/// <summary>
/// Code-behind for the car selection window.
/// </summary>
public partial class CarSelectionWindow : Window
{
    /// <summary>Initialises the car selection window and sets up its DataContext.</summary>
    public CarSelectionWindow()
    {
        InitializeComponent();
        DataContext = new CarSelectionViewModel();
    }

    /// <summary>Handles the Back button click by closing this window.</summary>
    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
