using System;
using System.Text;

namespace CSharpSpeedRush.Models;

/// <summary>
/// Represents the race track, tracking lap progress and completion status.
/// </summary>
public class Track
{
    /// <summary>Gets the name of the track.</summary>
    public string Name { get; } = "Grand Circuit";

    /// <summary>Gets the total number of laps in the race.</summary>
    public int TotalLaps { get; } = 5;

    /// <summary>Gets the distance of a single lap in kilometres.</summary>
    public double LapDistance { get; } = 5.0;

    /// <summary>Gets the current lap number (1-indexed, starts at 1).</summary>
    public int CurrentLap { get; private set; } = 1;

    /// <summary>
    /// Gets the fractional progress within the current lap (0.0 to &lt;1.0).
    /// Resets to 0.0 each time a lap is completed.
    /// </summary>
    public double LapProgress { get; private set; } = 0.0;

    /// <summary>
    /// Advances the lap progress by the given fraction.
    /// Handles overflow correctly when <paramref name="progressGained"/> would carry across lap boundaries.
    /// </summary>
    /// <param name="progressGained">Fractional progress to add. Must be non-negative.</param>
    /// <returns><c>true</c> if at least one lap was completed; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="progressGained"/> is negative.
    /// </exception>
    public bool AdvanceLap(double progressGained)
    {
        if (progressGained < 0)
            throw new ArgumentOutOfRangeException(nameof(progressGained), "Progress gained must be non-negative.");

        LapProgress += progressGained;

        // Use a while loop so that a gain > 1.0 correctly counts multiple laps.
        bool lapCompleted = false;
        while (LapProgress >= 1.0)
        {
            LapProgress -= 1.0;
            CurrentLap++;
            lapCompleted = true;
        }
        return lapCompleted;
    }

    /// <summary>
    /// Determines whether the race has been completed (all laps finished).
    /// </summary>
    /// <returns><c>true</c> if <see cref="CurrentLap"/> exceeds <see cref="TotalLaps"/>; otherwise <c>false</c>.</returns>
    public bool IsRaceComplete() => CurrentLap > TotalLaps;

    /// <summary>
    /// Returns an ASCII progress bar representing the current lap progress.
    /// The bar uses <c>=</c> for filled sections, <c>&gt;</c> for the current position,
    /// and spaces for the remaining distance.
    /// </summary>
    /// <param name="width">
    /// Total character width of the bar interior (default 20).
    /// One character is reserved for the progress indicator (<c>&gt;</c>),
    /// so usable fill width is <c>width - 1</c>.
    /// </param>
    /// <returns>A string like <c>[=====>         ] 30%</c>.</returns>
    public string GetProgressBar(int width = 20)
    {
        // Clamp to [0, width-1] to defend against floating-point edge cases at 100%.
        int filled = Math.Clamp((int)(LapProgress * (width - 1)), 0, width - 1);

        var sb = new StringBuilder("[");
        for (int i = 0; i < width; i++)
        {
            if (i < filled)
                sb.Append('=');
            else if (i == filled)
                sb.Append('>');
            else
                sb.Append(' ');
        }
        sb.Append(']');
        sb.Append($" {(int)(LapProgress * 100)}%");
        return sb.ToString();
    }

    /// <summary>
    /// Resets the track to its initial state so the same instance can be reused.
    /// </summary>
    public void Reset()
    {
        CurrentLap = 1;
        LapProgress = 0.0;
    }
}
