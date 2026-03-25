namespace CSharpSpeedRush.Models;

/// <summary>Represents a player action each turn during the race.</summary>
public enum PlayerAction
{
    /// <summary>The player accelerates, gaining more distance but using more fuel and less time.</summary>
    SpeedUp,

    /// <summary>The player maintains current speed, balancing distance, fuel, and time.</summary>
    MaintainSpeed,

    /// <summary>The player stops to refuel, gaining no distance but restoring fuel.</summary>
    PitStop
}

/// <summary>Represents the current state of the race.</summary>
public enum RaceState
{
    /// <summary>The race has not yet begun.</summary>
    NotStarted,

    /// <summary>The race is actively in progress.</summary>
    InProgress,

    /// <summary>The race has finished successfully (all laps completed).</summary>
    Finished,

    /// <summary>The race ended because the car ran out of fuel.</summary>
    OutOfFuel,

    /// <summary>The race ended because the time limit was exceeded.</summary>
    OutOfTime
}

/// <summary>Stores a record of a single lap completed during the race.</summary>
public struct LapRecord
{
    /// <summary>Gets or sets the lap number (1-indexed).</summary>
    public int LapNumber { get; set; }

    /// <summary>Gets or sets the total time used during this lap, in seconds.</summary>
    public double TimeUsed { get; set; }

    /// <summary>Gets or sets the total fuel consumed during this lap, in litres.</summary>
    public double FuelUsed { get; set; }

    /// <summary>Gets or sets the action taken on the turn that completed this lap.</summary>
    public PlayerAction Action { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="LapRecord"/> with the specified values.
    /// </summary>
    /// <param name="lapNumber">The lap number.</param>
    /// <param name="timeUsed">Time used in seconds.</param>
    /// <param name="fuelUsed">Fuel used in litres.</param>
    /// <param name="action">The player action taken.</param>
    public LapRecord(int lapNumber, double timeUsed, double fuelUsed, PlayerAction action)
    {
        LapNumber = lapNumber;
        TimeUsed = timeUsed;
        FuelUsed = fuelUsed;
        Action = action;
    }
}
