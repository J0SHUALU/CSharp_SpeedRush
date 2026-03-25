using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpSpeedRush.Models;

namespace CSharpSpeedRush.ViewModels;

/// <summary>
/// ViewModel for the race window. Manages race state updates, player commands,
/// and all display properties shown during an active race session.
/// </summary>
public partial class RaceWindowViewModel : ViewModelBase
{
    private readonly RaceManager _raceManager;
    private readonly DispatcherTimer _timer;

    /// <summary>
    /// Guards against processing two turns simultaneously if the player clicks a button
    /// twice before the UI has time to disable the controls.
    /// </summary>
    private bool _isProcessingTurn = false;

    [ObservableProperty] private string _lapDisplay = "Lap 1 / 5";
    [ObservableProperty] private double _fuelLevel = 100;
    [ObservableProperty] private double _fuelPercentage = 100;
    [ObservableProperty] private double _timeRemaining = 420;
    [ObservableProperty] private double _timePercentage = 100;
    [ObservableProperty] private string _speedStatus = "Ready to race!";
    [ObservableProperty] private string _progressBarDisplay = "[>                   ] 0%";
    [ObservableProperty] private string _eventLogDisplay = "Race started! Make your move.";
    [ObservableProperty] private string _statusMessage = "Choose your action!";
    [ObservableProperty] private bool _actionsEnabled = true;
    [ObservableProperty] private bool _speedUpEnabled = true;
    [ObservableProperty] private bool _maintainEnabled = true;
    [ObservableProperty] private bool _pitStopEnabled = true;
    [ObservableProperty] private bool _raceOver = false;

    /// <summary>Gets the large headline shown in the end-of-race popup (e.g. "VICTORY!").</summary>
    [ObservableProperty] private string _outcomeHeadline = string.Empty;

    /// <summary>Gets the detail line shown in the end-of-race popup (laps / fuel / time).</summary>
    [ObservableProperty] private string _outcomeDetail = string.Empty;

    /// <summary>True when the player won (all laps completed); drives popup color.</summary>
    [ObservableProperty] private bool _isVictory = false;

    /// <summary>
    /// Initialises a new <see cref="RaceWindowViewModel"/> with the given race manager.
    /// Syncs the initial display state and starts the per-second UI refresh timer.
    /// </summary>
    /// <param name="raceManager">The race manager controlling game logic.</param>
    public RaceWindowViewModel(RaceManager raceManager)
    {
        _raceManager = raceManager;
        SyncDisplayFromManager();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    /// <summary>
    /// Handles the per-second timer tick. Refreshes UI while the race is running;
    /// stops the timer once the race reaches a terminal state.
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_raceManager.State == RaceState.InProgress)
            SyncDisplayFromManager();
        else
            _timer.Stop();
    }

    /// <summary>
    /// Reads the current state from <see cref="_raceManager"/> and updates all bound properties.
    /// Uses the shared <see cref="RaceManager.PitStopFuelFullThreshold"/> so the pit stop
    /// button enable logic stays in sync with the model's validation.
    /// </summary>
    private void SyncDisplayFromManager()
    {
        var car = _raceManager.ActiveCar;
        var track = _raceManager.ActiveTrack;

        // Cap displayed lap at TotalLaps so the header never reads "Lap 6 / 5" at race end.
        int displayLap = Math.Min(track.CurrentLap, track.TotalLaps);
        LapDisplay = $"Lap {displayLap} / {track.TotalLaps}";

        FuelLevel = (car.CurrentFuel / car.FuelCapacity) * 100.0;
        FuelPercentage = FuelLevel;

        TimeRemaining = _raceManager.TimeRemaining;
        TimePercentage = (_raceManager.TimeRemaining / _raceManager.TotalTime) * 100.0;

        SpeedStatus = $"{car.Name} | {car.MaxSpeed} km/h | Fuel: {car.CurrentFuel:F1}L";
        ProgressBarDisplay = track.GetProgressBar();

        // Reverse so the most recent event appears at the top of the log panel.
        EventLogDisplay = _raceManager.EventLog.Any()
            ? string.Join("\n", _raceManager.EventLog.Reverse())
            : "No events yet.";

        bool alive = _raceManager.State == RaceState.InProgress;
        ActionsEnabled = alive;
        SpeedUpEnabled = alive;
        MaintainEnabled = alive;

        // Pit stop is also gated on the shared fuel threshold constant.
        PitStopEnabled = alive &&
            (car.CurrentFuel / car.FuelCapacity) < RaceManager.PitStopFuelFullThreshold;
    }

    /// <summary>Processes the Speed Up action for this turn.</summary>
    [RelayCommand]
    private void SpeedUp() => ExecuteAction(PlayerAction.SpeedUp);

    /// <summary>Processes the Maintain Speed action for this turn.</summary>
    [RelayCommand]
    private void MaintainSpeed() => ExecuteAction(PlayerAction.MaintainSpeed);

    /// <summary>Processes the Pit Stop action for this turn.</summary>
    [RelayCommand]
    private void PitStop() => ExecuteAction(PlayerAction.PitStop);

    /// <summary>
    /// Stops the timer and raises <see cref="BackToMenuRequested"/> so the view can close itself.
    /// </summary>
    [RelayCommand]
    private void BackToMenu()
    {
        _timer.Stop();
        BackToMenuRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised when the player requests to return to the main menu.</summary>
    public event EventHandler? BackToMenuRequested;

    /// <summary>
    /// Forwards the given <paramref name="action"/> to the race manager, then syncs the UI.
    /// Invalid game-logic actions (e.g. pit stop on a full tank) are caught and shown
    /// as a status message rather than crashing. A re-entrancy guard prevents double-clicks
    /// from submitting two turns before the buttons disable.
    /// </summary>
    /// <param name="action">The player action to process.</param>
    private void ExecuteAction(PlayerAction action)
    {
        // Re-entrancy guard: ignore the call if a turn is already being processed.
        if (_isProcessingTurn) return;
        _isProcessingTurn = true;

        try
        {
            var result = _raceManager.ProcessTurn(action);
            StatusMessage = result.StatusMessage;
            SyncDisplayFromManager();

            if (result.State != RaceState.InProgress)
            {
                _timer.Stop();
                ActionsEnabled = false;
                SpeedUpEnabled = false;
                MaintainEnabled = false;
                PitStopEnabled = false;
                RaceOver = true;
                ShowRaceSummary(result);
            }
        }
        catch (InvalidOperationException ex)
        {
            // Expected game-rule violations (full tank pit stop, race not started, etc.)
            StatusMessage = $"Invalid action: {ex.Message}";
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // Unrecognised PlayerAction — should never happen in normal play.
            StatusMessage = $"Unexpected action error: {ex.Message}";
        }
        catch (Exception ex)
        {
            // Catch-all for truly unexpected failures so the UI stays responsive.
            StatusMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            _isProcessingTurn = false;
        }
    }

    /// <summary>
    /// Composes the end-of-race summary string and updates the status bar and event log.
    /// </summary>
    /// <param name="result">The final <see cref="RaceResult"/> returned by <see cref="RaceManager.ProcessTurn"/>.</param>
    private void ShowRaceSummary(RaceResult result)
    {
        // CurrentLap is incremented past TotalLaps on race completion; clamp for the display.
        int lapsCompleted = Math.Min(
            Math.Max(0, _raceManager.ActiveTrack.CurrentLap - 1),
            _raceManager.ActiveTrack.TotalLaps);

        IsVictory = result.State == RaceState.Finished;

        OutcomeHeadline = result.State switch
        {
            RaceState.Finished  => "VICTORY!",
            RaceState.OutOfFuel => "OUT OF FUEL",
            RaceState.OutOfTime => "TIME EXPIRED",
            _                   => "RACE OVER"
        };

        OutcomeDetail = $"Laps completed: {lapsCompleted} / {_raceManager.ActiveTrack.TotalLaps}" +
                        $"\nFuel remaining: {result.FuelRemaining:F1} L" +
                        $"\nTime remaining: {result.TimeRemaining:F0} s";

        string outcome = result.State switch
        {
            RaceState.Finished  => "You finished the race!",
            RaceState.OutOfFuel => "You ran out of fuel!",
            RaceState.OutOfTime => "Time expired!",
            _                   => "Race ended."
        };

        StatusMessage = $"{outcome} | Laps: {lapsCompleted}/{_raceManager.ActiveTrack.TotalLaps} | " +
                        $"Fuel left: {result.FuelRemaining:F1}L | Time left: {result.TimeRemaining:F0}s";

        EventLogDisplay = _raceManager.EventLog.Any()
            ? string.Join("\n", _raceManager.EventLog.Reverse())
            : "No events logged.";
    }
}
