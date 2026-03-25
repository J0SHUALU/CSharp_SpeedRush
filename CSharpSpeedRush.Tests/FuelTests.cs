using System;
using CSharpSpeedRush.Models;
using Xunit;

namespace CSharpSpeedRush.Tests;

/// <summary>
/// Unit tests for fuel consumption behaviour of cars and the pit stop fuel logic.
/// </summary>
public class FuelTests
{
    /// <summary>
    /// Verifies that a SportsCar reports the correct fuel usage for a SpeedUp action.
    /// Expected: 4.5 (base) × 1.4 (SpeedUp multiplier) = 6.3 L.
    /// </summary>
    [Fact]
    public void SportsCar_SpeedUp_ReducesFuelCorrectly()
    {
        var car = new SportsCar();
        double expectedFuelUsed = car.FuelConsumptionRate * 1.4; // 6.3 L

        double actualFuelUsed = car.CalculateFuelUsage(PlayerAction.SpeedUp);

        Assert.Equal(expectedFuelUsed, actualFuelUsed, precision: 5);
    }

    /// <summary>
    /// Verifies that EcoCar consumes less fuel than SportsCar for the same SpeedUp action,
    /// confirming the car-differentiation design.
    /// </summary>
    [Fact]
    public void EcoCar_ConsumesFuelSlowerThanSportsCar()
    {
        var sports = new SportsCar();
        var eco = new EcoCar();

        double sportsFuel = sports.CalculateFuelUsage(PlayerAction.SpeedUp);
        double ecoFuel = eco.CalculateFuelUsage(PlayerAction.SpeedUp);

        Assert.True(ecoFuel < sportsFuel,
            $"EcoCar ({ecoFuel:F1}L) should use less fuel than SportsCar ({sportsFuel:F1}L).");
    }

    /// <summary>
    /// Verifies that attempting a pit stop when fuel is at or above 90% of capacity
    /// throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void PitStop_OverfullFuel_ThrowsException()
    {
        var car = new SportsCar();
        // Fill to 95% — above the 90% pit stop threshold
        car.CurrentFuel = car.FuelCapacity * 0.95;
        var manager = new RaceManager(car, new Track());
        manager.StartRace();

        Assert.Throws<InvalidOperationException>(() => manager.ProcessTurn(PlayerAction.PitStop));
    }

    /// <summary>
    /// Verifies that a pit stop correctly adds the car-specific refuel amount
    /// without exceeding the tank capacity.
    /// SportsCar starts at 30L and adds 20L → expected 50L.
    /// </summary>
    [Fact]
    public void PitStop_RefuelsCorrectAmountWithoutOverflow()
    {
        var car = new SportsCar();
        car.CurrentFuel = 30.0; // Partially drained (below 90% threshold = 54L)
        var manager = new RaceManager(car, new Track());
        manager.StartRace();

        var result = manager.ProcessTurn(PlayerAction.PitStop);

        double expected = Math.Min(car.FuelCapacity, 30.0 + car.PitStopRefuelAmount); // 50L
        Assert.Equal(expected, result.FuelRemaining, precision: 5);
    }

    /// <summary>
    /// Verifies that fuel is clamped to exactly 0.0 when a turn would otherwise make it negative.
    /// Ensures the UI never displays a negative fuel reading.
    /// </summary>
    [Fact]
    public void SpeedUp_WithAlmostNoFuel_ClampsFuelToZero()
    {
        var car = new SportsCar();
        car.CurrentFuel = 1.0; // Far below the 6.3L SpeedUp cost
        var manager = new RaceManager(car, new Track());
        manager.StartRace();

        var result = manager.ProcessTurn(PlayerAction.SpeedUp);

        Assert.Equal(0.0, result.FuelRemaining);
        Assert.Equal(RaceState.OutOfFuel, result.State);
    }

    /// <summary>
    /// Verifies that an unrecognised <see cref="PlayerAction"/> value (cast from int)
    /// throws <see cref="ArgumentOutOfRangeException"/> from the car's fuel calculator.
    /// </summary>
    [Fact]
    public void CalculateFuelUsage_UnknownAction_ThrowsArgumentOutOfRange()
    {
        var car = new SportsCar();
        var badAction = (PlayerAction)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => car.CalculateFuelUsage(badAction));
    }
}
