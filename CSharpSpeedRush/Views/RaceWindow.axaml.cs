using Avalonia.Controls;
using CSharpSpeedRush.ViewModels;

namespace CSharpSpeedRush.Views;

/// <summary>
/// Code-behind for the race window. The DataContext is set externally by the car selection ViewModel.
/// </summary>
public partial class RaceWindow : Window
{
    /// <summary>Initialises the race window and subscribes to the back-to-menu event.</summary>
    public RaceWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Subscribes to the ViewModel's back-to-menu event when the DataContext is set.
    /// </summary>
    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is RaceWindowViewModel vm)
            vm.BackToMenuRequested += (_, _) => Close();
    }
}
