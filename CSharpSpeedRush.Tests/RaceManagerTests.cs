using CSharpSpeedRush.Models;
using Xunit;

namespace CSharpSpeedRush.Tests;

/// <summary>
/// Unit tests for <see cref="RaceManager"/> turn processing and state transitions.
/// </summary>
public class RaceManagerTests
{
    /// <summary>
    /// Verifies that calling <see cref="RaceManager.ProcessTurn"/> before
    /// <see cref="RaceManager.StartRace"/> throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void ProcessTurn_BeforeStart_ThrowsInvalidOperationException()
    {
        var manager = new RaceManager(new SportsCar(), new Track());
        // StartRace() deliberately NOT called.

        Assert.Throws<InvalidOperationException>(() => manager.ProcessTurn(PlayerAction.SpeedUp));
    }

    /// <summary>
    /// Verifies that a SpeedUp turn costs exactly 12s
    /// (BaseTimePerTurn 15s − SpeedUpTimeBonus 3s) and reduces fuel.
    /// </summary>
    [Fact]
    public void ProcessTurn_SpeedUp_UsesReducedTimeAndConsumesFuel()
    {
        var car = new SportsCar();
        var manager = new RaceManager(car, new Track(), totalTimeSeconds: 300);
        manager.StartRace();
        double fuelBefore = car.CurrentFuel;
        double timeBefore = manager.TimeRemaining;

        var result = manager.ProcessTurn(PlayerAction.SpeedUp);

        // Time cost = BaseTimePerTurn - SpeedUpTimeBonus = 15 - 3 = 12s
        Assert.Equal(timeBefore - (RaceManager.BaseTimePerTurn - RaceManager.SpeedUpTimeBonus),
                     result.TimeRemaining, precision: 5);
        Assert.True(result.FuelRemaining < fuelBefore, "Fuel should decrease after SpeedUp.");
        Assert.Equal(RaceState.InProgress, result.State);
    }

    /// <summary>
    /// Verifies that a MaintainSpeed turn costs exactly the full
    /// <see cref="RaceManager.BaseTimePerTurn"/> (15s) and reduces fuel.
    /// </summary>
    [Fact]
    public void ProcessTurn_MaintainSpeed_UsesFullBaseTime()
    {
        var car = new SportsCar();
        var manager = new RaceManager(car, new Track(), totalTimeSeconds: 300);
        manager.StartRace();
        double timeBefore = manager.TimeRemaining;

        var result = manager.ProcessTurn(PlayerAction.MaintainSpeed);

        Assert.Equal(timeBefore - RaceManager.BaseTimePerTurn, result.TimeRemaining, precision: 5);
        Assert.Equal(RaceState.InProgress, result.State);
    }

    /// <summary>
    /// Verifies that running the car out of fuel transitions the race state to
    /// <see cref="RaceState.OutOfFuel"/> and clamps fuel to exactly 0.
    /// </summary>
    [Fact]
    public void ProcessTurn_OutOfFuel_SetsCorrectRaceStateAndClampsFuel()
    {
        var car = new SportsCar();
        car.CurrentFuel = 1.0; // Well below the SpeedUp cost (6.3L)
        var manager = new RaceManager(car, new Track());
        manager.StartRace();

        var result = manager.ProcessTurn(PlayerAction.SpeedUp);

        Assert.Equal(RaceState.OutOfFuel, result.State);
        Assert.Equal(0.0, result.FuelRemaining); // Must not go negative
    }

    /// <summary>
    /// Verifies that exceeding the time limit transitions state to
    /// <see cref="RaceState.OutOfTime"/> and clamps <see cref="RaceResult.TimeRemaining"/> to 0.
    /// </summary>
    [Fact]
    public void ProcessTurn_TimeExpires_SetsOutOfTimeAndClampsTime()
    {
        var car = new EcoCar(); // Low fuel usage so fuel isn't the limiting factor
        var manager = new RaceManager(car, new Track(), totalTimeSeconds: 5.0); // Very short race
        manager.StartRace();

        // Any action will consume at least 12s, exceeding the 5s limit.
        var result = manager.ProcessTurn(PlayerAction.MaintainSpeed);

        Assert.Equal(RaceState.OutOfTime, result.State);
        Assert.Equal(0.0, result.TimeRemaining);
    }

    /// <summary>
    /// Verifies that completing all 5 laps transitions the state to
    /// <see cref="RaceState.Finished"/>.
    /// </summary>
    [Fact]
    public void ProcessTurn_AllLapsComplete_SetsFinished()
    {
        var car = new MuscleCar(); // SpeedUp gives 0.30 per turn, so 4 turns = 1.2 laps
        var track = new Track();
        var manager = new RaceManager(car, track, totalTimeSeconds: 9999);
        manager.StartRace();

        // Advance track to lap 5 with a tiny bit of progress remaining.
        for (int i = 0; i < 4; i++) track.AdvanceLap(1.0);
        track.AdvanceLap(0.99); // Lap 5, almost done

        // One SpeedUp (+0.30) should push past 1.0, completing lap 5.
        var result = manager.ProcessTurn(PlayerAction.SpeedUp);

        Assert.Equal(RaceState.Finished, result.State);
    }

    /// <summary>
    /// Verifies that <see cref="RaceManager.OutOfFuel"/> takes priority over
    /// <see cref="RaceState.OutOfTime"/> when both conditions occur on the same turn.
    /// OutOfFuel is set first in ProcessTurn and the time check guards against overwriting it.
    /// </summary>
    [Fact]
    public void ProcessTurn_OutOfFuelAndTime_OutOfFuelTakesPriority()
    {
        var car = new SportsCar();
        car.CurrentFuel = 1.0; // Will go to 0 on SpeedUp
        var manager = new RaceManager(car, new Track(), totalTimeSeconds: 5.0); // Also expires
        manager.StartRace();

        var result = manager.ProcessTurn(PlayerAction.SpeedUp);

        Assert.Equal(RaceState.OutOfFuel, result.State);
    }
}
