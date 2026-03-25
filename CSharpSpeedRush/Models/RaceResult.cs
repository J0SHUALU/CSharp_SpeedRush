namespace CSharpSpeedRush.Models;

/// <summary>
/// Represents the result of a single turn processed by the <see cref="RaceManager"/>.
/// </summary>
public struct RaceResult
{
    /// <summary>Gets or sets whether a lap was completed on this turn.</summary>
    public bool LapCompleted { get; set; }

    /// <summary>Gets or sets the current lap number after this turn.</summary>
    public int CurrentLap { get; set; }

    /// <summary>Gets or sets the remaining fuel in litres after this turn.</summary>
    public double FuelRemaining { get; set; }

    /// <summary>Gets or sets the remaining time in seconds after this turn.</summary>
    public double TimeRemaining { get; set; }

    /// <summary>Gets or sets the current race state after this turn.</summary>
    public RaceState State { get; set; }

    /// <summary>Gets or sets a human-readable message describing what happened this turn.</summary>
    public string StatusMessage { get; set; }

    /// <summary>Gets or sets the ASCII progress bar for lap progress.</summary>
    public string ProgressBar { get; set; }

    /// <summary>
    /// Initialises a new <see cref="RaceResult"/> with all fields specified.
    /// </summary>
    /// <param name="lapCompleted">Whether a lap was completed.</param>
    /// <param name="currentLap">Current lap number.</param>
    /// <param name="fuelRemaining">Remaining fuel in litres.</param>
    /// <param name="timeRemaining">Remaining time in seconds.</param>
    /// <param name="state">Current race state.</param>
    /// <param name="statusMessage">Status message for this turn.</param>
    /// <param name="progressBar">ASCII progress bar string.</param>
    public RaceResult(bool lapCompleted, int currentLap, double fuelRemaining, double timeRemaining,
                      RaceState state, string statusMessage, string progressBar)
    {
        LapCompleted = lapCompleted;
        CurrentLap = currentLap;
        FuelRemaining = fuelRemaining;
        TimeRemaining = timeRemaining;
        State = state;
        StatusMessage = statusMessage;
        ProgressBar = progressBar;
    }
}
