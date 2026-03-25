using System;
using CSharpSpeedRush.Models;
using Xunit;

namespace CSharpSpeedRush.Tests;

/// <summary>
/// Unit tests for lap advancement and race completion logic in <see cref="Track"/>.
/// </summary>
public class LapProgressionTests
{
    /// <summary>
    /// Verifies that adding exactly 1.0 of progress completes a lap, increments
    /// <see cref="Track.CurrentLap"/>, and resets <see cref="Track.LapProgress"/> to 0.
    /// </summary>
    [Fact]
    public void Track_AdvanceLap_CompletesLapWhenProgressReaches100()
    {
        var track = new Track();
        int initialLap = track.CurrentLap;

        bool lapCompleted = track.AdvanceLap(1.0);

        Assert.True(lapCompleted);
        Assert.Equal(initialLap + 1, track.CurrentLap);
        Assert.Equal(0.0, track.LapProgress, precision: 5);
    }

    /// <summary>
    /// Verifies that four increments of 0.25 each accumulate correctly, with the lap
    /// completing only on the fourth call (not before).
    /// </summary>
    [Fact]
    public void Track_MultipleAdvances_AccumulatesProgress()
    {
        var track = new Track();
        bool earlyCompletion = false;

        for (int i = 0; i < 3; i++)
            earlyCompletion |= track.AdvanceLap(0.25);

        bool completedOnFourth = track.AdvanceLap(0.25);

        Assert.False(earlyCompletion, "Lap should not complete before reaching 1.0.");
        Assert.True(completedOnFourth, "Lap should complete on the 4th increment of 0.25.");
        Assert.Equal(2, track.CurrentLap);
    }

    /// <summary>
    /// Verifies that <see cref="Track.IsRaceComplete"/> returns <c>true</c> only after
    /// all 5 laps have been completed.
    /// </summary>
    [Fact]
    public void Track_IsRaceComplete_ReturnsTrueAfterFiveLaps()
    {
        var track = new Track();

        for (int i = 0; i < 5; i++)
            track.AdvanceLap(1.0);

        Assert.True(track.IsRaceComplete());
    }

    /// <summary>
    /// Verifies that a progress gain greater than 1.0 in a single call correctly
    /// completes one lap and carries the remainder into the next lap
    /// (no laps are silently skipped).
    /// </summary>
    [Fact]
    public void Track_AdvanceLap_OverflowProgress_CountsLapAndCarriesRemainder()
    {
        var track = new Track();

        bool lapCompleted = track.AdvanceLap(1.3); // should complete lap 1, leaving 0.3 progress

        Assert.True(lapCompleted);
        Assert.Equal(2, track.CurrentLap);
        Assert.Equal(0.3, track.LapProgress, precision: 5);
    }

    /// <summary>
    /// Verifies that a progress gain of 2.0 in a single call correctly completes
    /// two laps, not just one.
    /// </summary>
    [Fact]
    public void Track_AdvanceLap_DoubleOverflow_CountsTwoLaps()
    {
        var track = new Track();

        bool lapCompleted = track.AdvanceLap(2.0);

        Assert.True(lapCompleted);
        Assert.Equal(3, track.CurrentLap); // started at 1, gained 2 laps
        Assert.Equal(0.0, track.LapProgress, precision: 5);
    }

    /// <summary>
    /// Verifies that passing a negative progress value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void Track_AdvanceLap_NegativeProgress_ThrowsArgumentOutOfRange()
    {
        var track = new Track();

        Assert.Throws<ArgumentOutOfRangeException>(() => track.AdvanceLap(-0.5));
    }
}
