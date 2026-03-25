using System;

namespace CSharpSpeedRush.Models;

/// <summary>
/// Abstract base class representing a racing car with speed, fuel, and action-based behaviour.
/// </summary>
public abstract class Car
{
    /// <summary>Gets the display name of the car.</summary>
    public abstract string Name { get; }

    /// <summary>Gets the maximum speed of the car in km/h.</summary>
    public abstract double MaxSpeed { get; }

    /// <summary>Gets the base fuel consumption rate per turn at maximum speed, in litres.</summary>
    public abstract double FuelConsumptionRate { get; }

    /// <summary>Gets the total fuel capacity of the car in litres.</summary>
    public abstract double FuelCapacity { get; }

    /// <summary>Gets or sets the current fuel level in litres.</summary>
    public double CurrentFuel { get; set; }

    /// <summary>
    /// Gets or sets the current speed of the car in km/h.
    /// Reserved for future telemetry display; not used in game-logic calculations.
    /// </summary>
    public double CurrentSpeed { get; set; }

    /// <summary>
    /// Calculates the fuel used for the given player action.
    /// </summary>
    /// <param name="action">The player action to calculate fuel usage for.</param>
    /// <returns>The amount of fuel consumed in litres.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unrecognised <see cref="PlayerAction"/> is passed.</exception>
    public abstract double CalculateFuelUsage(PlayerAction action);

    /// <summary>
    /// Calculates the fractional lap distance gained for the given player action.
    /// </summary>
    /// <param name="action">The player action taken this turn.</param>
    /// <returns>A value from 0.0 to 1.0 representing the fraction of a lap gained.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unrecognised <see cref="PlayerAction"/> is passed.</exception>
    public virtual double CalculateDistanceGained(PlayerAction action)
    {
        return action switch
        {
            PlayerAction.SpeedUp => GetSpeedUpDistance(),
            PlayerAction.MaintainSpeed => GetMaintainDistance(),
            PlayerAction.PitStop => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unrecognised player action.")
        };
    }

    /// <summary>Returns the lap fraction gained when speeding up.</summary>
    /// <returns>Fractional lap progress for SpeedUp.</returns>
    protected abstract double GetSpeedUpDistance();

    /// <summary>Returns the lap fraction gained when maintaining speed.</summary>
    /// <returns>Fractional lap progress for MaintainSpeed.</returns>
    protected abstract double GetMaintainDistance();

    /// <summary>
    /// Returns a human-readable string showing the car's key statistics.
    /// </summary>
    /// <returns>A formatted string with car stats.</returns>
    public override string ToString()
        => $"{Name} | Max Speed: {MaxSpeed} km/h | Fuel: {CurrentFuel:F1}/{FuelCapacity} L | Consumption: {FuelConsumptionRate} L/turn";
}

/// <summary>
/// A high-speed sports car with high fuel consumption and excellent acceleration.
/// </summary>
public class SportsCar : Car
{
    /// <inheritdoc/>
    public override string Name => "Sports Car";

    /// <inheritdoc/>
    public override double MaxSpeed => 220;

    /// <inheritdoc/>
    public override double FuelConsumptionRate => 4.5;

    /// <inheritdoc/>
    public override double FuelCapacity => 60;

    /// <summary>Initialises a new <see cref="SportsCar"/> with a full tank.</summary>
    public SportsCar() { CurrentFuel = FuelCapacity; }

    /// <inheritdoc/>
    public override double CalculateFuelUsage(PlayerAction action)
    {
        return action switch
        {
            PlayerAction.SpeedUp => FuelConsumptionRate * 1.4,
            PlayerAction.MaintainSpeed => FuelConsumptionRate * 1.0,
            PlayerAction.PitStop => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unrecognised player action.")
        };
    }

    /// <inheritdoc/>
    protected override double GetSpeedUpDistance() => 0.25;

    /// <inheritdoc/>
    protected override double GetMaintainDistance() => 0.18;

    /// <summary>Gets the amount of fuel restored during a pit stop in litres.</summary>
    public double PitStopRefuelAmount => 20.0;

    /// <summary>Gets the time penalty (seconds) added when performing a pit stop.</summary>
    public double PitStopTimePenalty => 10.0;
}

/// <summary>
/// A fuel-efficient eco car with lower speed but excellent range.
/// </summary>
public class EcoCar : Car
{
    /// <inheritdoc/>
    public override string Name => "Eco Car";

    /// <inheritdoc/>
    public override double MaxSpeed => 160;

    /// <inheritdoc/>
    public override double FuelConsumptionRate => 2.0;

    /// <inheritdoc/>
    public override double FuelCapacity => 45;

    /// <summary>Initialises a new <see cref="EcoCar"/> with a full tank.</summary>
    public EcoCar() { CurrentFuel = FuelCapacity; }

    /// <inheritdoc/>
    public override double CalculateFuelUsage(PlayerAction action)
    {
        return action switch
        {
            PlayerAction.SpeedUp => FuelConsumptionRate * 1.3,
            PlayerAction.MaintainSpeed => FuelConsumptionRate * 1.0,
            PlayerAction.PitStop => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unrecognised player action.")
        };
    }

    /// <inheritdoc/>
    protected override double GetSpeedUpDistance() => 0.18;

    /// <inheritdoc/>
    protected override double GetMaintainDistance() => 0.15;

    /// <summary>Gets the amount of fuel restored during a pit stop in litres.</summary>
    public double PitStopRefuelAmount => 15.0;

    /// <summary>Gets the time penalty (seconds) added when performing a pit stop.</summary>
    public double PitStopTimePenalty => 8.0;
}

/// <summary>
/// A powerful muscle car with raw speed and high fuel consumption.
/// </summary>
public class MuscleCar : Car
{
    /// <inheritdoc/>
    public override string Name => "Muscle Car";

    /// <inheritdoc/>
    public override double MaxSpeed => 200;

    /// <inheritdoc/>
    public override double FuelConsumptionRate => 6.0;

    /// <inheritdoc/>
    public override double FuelCapacity => 70;

    /// <summary>Initialises a new <see cref="MuscleCar"/> with a full tank.</summary>
    public MuscleCar() { CurrentFuel = FuelCapacity; }

    /// <inheritdoc/>
    public override double CalculateFuelUsage(PlayerAction action)
    {
        return action switch
        {
            PlayerAction.SpeedUp => FuelConsumptionRate * 1.5,
            PlayerAction.MaintainSpeed => FuelConsumptionRate * 1.0,
            PlayerAction.PitStop => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unrecognised player action.")
        };
    }

    /// <inheritdoc/>
    protected override double GetSpeedUpDistance() => 0.30;

    /// <inheritdoc/>
    protected override double GetMaintainDistance() => 0.20;

    /// <summary>Gets the amount of fuel restored during a pit stop in litres.</summary>
    public double PitStopRefuelAmount => 25.0;

    /// <summary>Gets the time penalty (seconds) added when performing a pit stop.</summary>
    public double PitStopTimePenalty => 12.0;
}
