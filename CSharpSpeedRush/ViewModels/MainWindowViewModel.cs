using System;
using CommunityToolkit.Mvvm.Input;
using CSharpSpeedRush.Views;

namespace CSharpSpeedRush.ViewModels;

/// <summary>
/// ViewModel for the main menu window. Provides commands to navigate to
/// car selection or view game instructions.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>Gets the game title displayed on the main menu.</summary>
    public string GameTitle => "C# Speed Rush";

    /// <summary>Gets the game tagline displayed beneath the title.</summary>
    public string Tagline => "Turn-based racing at the speed of code!";

    /// <summary>
    /// Opens the car selection window to begin a new race.
    /// </summary>
    [RelayCommand]
    private void StartRace()
    {
        var selectionWindow = new CarSelectionWindow();
        selectionWindow.Show();
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    [RelayCommand]
    private void Quit()
    {
        Environment.Exit(0);
    }
}
