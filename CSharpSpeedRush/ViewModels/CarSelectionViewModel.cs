using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpSpeedRush.Models;
using CSharpSpeedRush.Views;

namespace CSharpSpeedRush.ViewModels;

/// <summary>
/// ViewModel for the car selection screen. Allows the player to choose a car
/// and review its stats before starting the race.
/// </summary>
public partial class CarSelectionViewModel : ViewModelBase
{
    /// <summary>Gets the list of cars available for selection.</summary>
    public ObservableCollection<Car> AvailableCars { get; } = new()
    {
        new SportsCar(),
        new EcoCar(),
        new MuscleCar()
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CarDetails))]
    private Car? _selectedCar;

    /// <summary>
    /// Gets a formatted multi-line string showing the selected car's stats.
    /// Returns a prompt if no car is selected.
    /// </summary>
    public string CarDetails
    {
        get
        {
            if (SelectedCar is null)
                return "Select a car to see its stats.";

            string pitRefuel = SelectedCar switch
            {
                SportsCar sc => $"{sc.PitStopRefuelAmount}L (+{sc.PitStopTimePenalty}s penalty)",
                EcoCar ec => $"{ec.PitStopRefuelAmount}L (+{ec.PitStopTimePenalty}s penalty)",
                MuscleCar mc => $"{mc.PitStopRefuelAmount}L (+{mc.PitStopTimePenalty}s penalty)",
                _ => "N/A"
            };

            double speedUp = SelectedCar.CalculateDistanceGained(PlayerAction.SpeedUp);
            double maintain = SelectedCar.CalculateDistanceGained(PlayerAction.MaintainSpeed);

            return $"""
                === {SelectedCar.Name} ===

                Max Speed:        {SelectedCar.MaxSpeed} km/h
                Fuel Capacity:    {SelectedCar.FuelCapacity} L
                Fuel per Turn:    {SelectedCar.FuelConsumptionRate} L/turn (base)

                Speed Up:         +{speedUp * 100:F0}% lap | uses {SelectedCar.CalculateFuelUsage(PlayerAction.SpeedUp):F1}L | 12s
                Maintain:         +{maintain * 100:F0}% lap | uses {SelectedCar.CalculateFuelUsage(PlayerAction.MaintainSpeed):F1}L | 15s
                Pit Stop:         Refuels {pitRefuel}

                Starting Fuel:    {SelectedCar.CurrentFuel:F0} L (full tank)
                """;
        }
    }

    /// <summary>Selects the Sports Car card.</summary>
    [RelayCommand]
    private void SelectSportsCar() => SelectedCar = AvailableCars[0];

    /// <summary>Selects the Eco Car card.</summary>
    [RelayCommand]
    private void SelectEcoCar() => SelectedCar = AvailableCars[1];

    /// <summary>Selects the Muscle Car card.</summary>
    [RelayCommand]
    private void SelectMuscleCar() => SelectedCar = AvailableCars[2];

    /// <summary>
    /// Validates the car selection, creates a new race, and opens the race window.
    /// </summary>
    [RelayCommand]
    private void StartRace()
    {
        if (SelectedCar is null)
            return;

        var track = new Track();
        var manager = new RaceManager(SelectedCar, track);
        manager.StartRace();

        var raceVm = new RaceWindowViewModel(manager);
        var raceWindow = new RaceWindow
        {
            DataContext = raceVm
        };
        raceWindow.Show();
    }
}
