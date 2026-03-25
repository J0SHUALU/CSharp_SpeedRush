using System;
using System.Collections.Generic;

namespace CSharpSpeedRush.Models;

/// <summary>
/// Core game controller that manages race state, processes player turns,
/// and tracks history and events for a race session.
/// </summary>
public class RaceManager
{
    private const int MaxEventLogSize = 5;

    /// <summary>Base time cost of any turn in seconds.</summary>
    public const double BaseTimePerTurn = 15.0;

    /// <summary>
    /// Seconds saved on a SpeedUp turn (net cost = BaseTimePerTurn - SpeedUpTimeBonus = 12s).
    /// </summary>
    public const double SpeedUpTimeBonus = 3.0;

    /// <summary>
    /// Fuel fill fraction at or above which a pit stop is forbidden (90%).
    /// Shared with the ViewModel so both use the same threshold.
    /// </summary>
    public const double PitStopFuelFullThreshold = 0.90;

    /// <summary>Gets the active car racing on the track.</summary>
    public Car ActiveCar { get; }

    /// <summary>Gets the track the race is being run on.</summary>
    public Track ActiveTrack { get; }

    /// <summary>Gets the remaining time in seconds for this race.</summary>
    public double TimeRemaining { get; private set; }

    /// <summary>Gets the total time allowed for this race in seconds.</summary>
    public double TotalTime { get; }

    /// <summary>Gets the current state of the race.</summary>
    public RaceState State { get; private set; } = RaceState.NotStarted;

    /// <summary>Gets the ordered list of completed lap records.</summary>
    public List<LapRecord> LapHistory { get; } = new();

    /// <summary>
    /// Gets the rolling event log of the most recent race events (capped at 5 entries).
    /// Older messages are automatically discarded when the cap is exceeded.
    /// </summary>
    public Queue<string> EventLog { get; } = new();

    /// <summary>
    /// Initialises a new <see cref="RaceManager"/> with the given car, track, and time limit.
    /// </summary>
    /// <param name="selectedCar">The car the player chose.</param>
    /// <param name="track">The track to race on.</param>
    /// <param name="totalTimeSeconds">Maximum race time in seconds (default 300s).</param>
    public RaceManager(Car selectedCar, Track track, double totalTimeSeconds = 420.0)
    {
        ActiveCar = selectedCar;
        ActiveTrack = track;
        TotalTime = totalTimeSeconds;
        TimeRemaining = totalTimeSeconds;
    }

    /// <summary>
    /// Starts the race, transitioning state to <see cref="RaceState.InProgress"/>.
    /// </summary>
    public void StartRace()
    {
        State = RaceState.InProgress;
        LogEvent("Race started! Good luck!");
    }

    /// <summary>
    /// Processes a single player turn, applying the chosen action to car and track state.
    /// Updates fuel, time, lap progress, event log, and race state accordingly.
    /// </summary>
    /// <param name="action">The action the player chose this turn.</param>
    /// <returns>A <see cref="RaceResult"/> describing the outcome of this turn.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the race is not in progress, or when a pit stop is attempted
    /// with a fuel level at or above <see cref="PitStopFuelFullThreshold"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Bubbled from <see cref="Car.CalculateFuelUsage"/> or <see cref="Car.CalculateDistanceGained"/>
    /// if an unrecognised <see cref="PlayerAction"/> is passed.
    /// </exception>
    public RaceResult ProcessTurn(PlayerAction action)
    {
        if (State != RaceState.InProgress)
            throw new InvalidOperationException("Race has not started.");

        // Validate pit stop: forbid if tank is already near-full.
        if (action == PlayerAction.PitStop)
        {
            double fuelPercent = ActiveCar.CurrentFuel / ActiveCar.FuelCapacity;
            if (fuelPercent >= PitStopFuelFullThreshold)
                throw new InvalidOperationException("Cannot pit stop: fuel tank is already at 90% or above.");
        }

        double fuelUsed = ActiveCar.CalculateFuelUsage(action);
        double timeUsed = BaseTimePerTurn;
        bool lapCompleted = false;

        if (action == PlayerAction.PitStop)
        {
            // Pit stop: refuel and apply time penalty; no lap progress gained.
            double refuelAmount = GetPitStopRefuelAmount();
            double pitPenalty = GetPitStopTimePenalty();
            ActiveCar.CurrentFuel = Math.Min(ActiveCar.FuelCapacity, ActiveCar.CurrentFuel + refuelAmount);
            timeUsed = BaseTimePerTurn + pitPenalty;
            LogEvent($"Pit stop! Refuelled +{refuelAmount}L. Time penalty: +{pitPenalty}s");
        }
        else
        {
            // Deduct fuel, clamping at 0 to prevent a negative fuel reading.
            ActiveCar.CurrentFuel = Math.Max(0, ActiveCar.CurrentFuel - fuelUsed);

            // SpeedUp saves 3s; MaintainSpeed uses the full base time.
            if (action == PlayerAction.SpeedUp)
                timeUsed = BaseTimePerTurn - SpeedUpTimeBonus; // 12s

            if (ActiveCar.CurrentFuel <= 0)
            {
                State = RaceState.OutOfFuel;
                LogEvent("Out of fuel! Race over.");
            }
            else
            {
                // Advance lap progress — AdvanceLap handles overflow across lap boundaries.
                double distanceGained = ActiveCar.CalculateDistanceGained(action);
                lapCompleted = ActiveTrack.AdvanceLap(distanceGained);

                if (lapCompleted)
                    LogEvent($"Lap {ActiveTrack.CurrentLap - 1} completed!");
                else
                    LogEvent($"Turn: {action}. Progress: {(int)(ActiveTrack.LapProgress * 100)}%");
            }
        }

        // Deduct time. OutOfFuel is the higher-priority terminal state; guard against overwriting it.
        TimeRemaining = Math.Max(0, TimeRemaining - timeUsed);
        if (TimeRemaining <= 0 && State == RaceState.InProgress)
        {
            State = RaceState.OutOfTime;
            LogEvent("Time's up! Race over.");
        }

        // Race complete check runs last; Finished only triggers while InProgress.
        if (State == RaceState.InProgress && ActiveTrack.IsRaceComplete())
        {
            State = RaceState.Finished;
            LogEvent("Race complete! Congratulations!");
        }

        if (lapCompleted)
        {
            LapHistory.Add(new LapRecord(
                lapNumber: ActiveTrack.CurrentLap - 1,
                timeUsed: timeUsed,
                fuelUsed: fuelUsed,
                action: action
            ));
        }

        string statusMessage = BuildStatusMessage(action, lapCompleted, fuelUsed, timeUsed);

        return new RaceResult(
            lapCompleted: lapCompleted,
            currentLap: ActiveTrack.CurrentLap,
            fuelRemaining: ActiveCar.CurrentFuel,
            timeRemaining: TimeRemaining,
            state: State,
            statusMessage: statusMessage,
            progressBar: ActiveTrack.GetProgressBar()
        );
    }

    /// <summary>
    /// Returns the refuel amount for the active car during a pit stop.
    /// Falls back to 15L for unknown car types.
    /// </summary>
    /// <returns>Litres added to the tank during a pit stop.</returns>
    private double GetPitStopRefuelAmount()
    {
        return ActiveCar switch
        {
            SportsCar sc => sc.PitStopRefuelAmount,
            EcoCar ec => ec.PitStopRefuelAmount,
            MuscleCar mc => mc.PitStopRefuelAmount,
            _ => 15.0
        };
    }

    /// <summary>
    /// Returns the time penalty for the active car during a pit stop.
    /// Falls back to 10s for unknown car types.
    /// </summary>
    /// <returns>Extra seconds added to the turn cost during a pit stop.</returns>
    private double GetPitStopTimePenalty()
    {
        return ActiveCar switch
        {
            SportsCar sc => sc.PitStopTimePenalty,
            EcoCar ec => ec.PitStopTimePenalty,
            MuscleCar mc => mc.PitStopTimePenalty,
            _ => 10.0
        };
    }

    /// <summary>
    /// Enqueues a message to the event log and removes the oldest entry if the cap is exceeded.
    /// </summary>
    /// <param name="message">The event message to record.</param>
    private void LogEvent(string message)
    {
        EventLog.Enqueue(message);
        while (EventLog.Count > MaxEventLogSize)
            EventLog.Dequeue();
    }

    /// <summary>
    /// Constructs a human-readable summary of the turn outcome for display in the UI.
    /// </summary>
    /// <param name="action">The action taken this turn.</param>
    /// <param name="lapCompleted">Whether a lap was completed.</param>
    /// <param name="fuelUsed">Fuel consumed this turn in litres.</param>
    /// <param name="timeUsed">Time consumed this turn in seconds.</param>
    /// <returns>A descriptive status string appropriate for the current race state.</returns>
    private string BuildStatusMessage(PlayerAction action, bool lapCompleted, double fuelUsed, double timeUsed)
    {
        return State switch
        {
            RaceState.OutOfFuel => "RACE OVER: You ran out of fuel!",
            RaceState.OutOfTime => "RACE OVER: Time expired!",
            RaceState.Finished => $"RACE COMPLETE! All {ActiveTrack.TotalLaps} laps finished!",
            _ => lapCompleted
                ? $"Lap completed! Action: {action}, Fuel used: {fuelUsed:F1}L, Time: {timeUsed:F0}s"
                : $"Action: {action} | Fuel: {ActiveCar.CurrentFuel:F1}L | Time left: {TimeRemaining:F0}s"
        };
    }
}
