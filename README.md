# C# Speed Rush

A turn-based, time-focused racing simulation desktop game built with Avalonia UI and .NET 8.

---

## Description

C# Speed Rush puts you behind the wheel of one of three unique racing cars for a tense,
tactical 5-lap race against the clock. Every turn counts — burn fuel too fast and you'll
stall out; drive too conservatively and the timer will run out. The right pit-stop at the
right moment can mean the difference between victory and defeat.

---

## Features

- **3 Unique Cars** — Sports Car, Eco Car, and Muscle Car, each with distinct speed,
  fuel economy, and tank capacity
- **Turn-Based Gameplay** — choose Speed Up, Maintain Speed, or Pit Stop each turn
- **Live Race HUD** — ASCII lap progress bar, fuel gauge, time countdown, and event log
- **Fuel & Time Management** — race ends if you run out of fuel or exceed the 300-second limit
- **Pit Stop Strategy** — refuel mid-race with a time penalty; cannot pit if tank is ≥ 90% full
- **Event Log** — rolling display of the last 5 race events
- **MVVM Architecture** — clean separation of concerns using CommunityToolkit.Mvvm
- **Unit Tested** — 10 xUnit tests covering fuel calculations, lap progression, and race states

---

## How to Play

1. **Launch the game** — the main menu appears
2. **Click "Start Race"** — the car selection screen opens
3. **Select a car** — click any car in the list to see its full stats on the right
4. **Click "Start Race!"** — the race begins immediately
5. **Each turn, choose one action:**
   - 🚀 **Speed Up** — fastest progress (+25–30% lap), highest fuel burn, only 12s used
   - ➡ **Maintain Speed** — moderate progress (+15–20% lap), normal fuel burn, 15s used
   - ⛽ **Pit Stop** — no progress, refuels the car, costs 15s + car-specific penalty
6. **Watch your gauges:**
   - Green bar = remaining fuel
   - Blue bar = remaining time
   - ASCII bar = current lap progress
7. **Win condition** — complete all 5 laps before running out of fuel or time

---


## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) or later
- A platform supported by Avalonia (Windows, macOS, Linux)

---

## How to Run

```bash
# Clone / navigate to the project
cd CSharpSpeedRush

# Run the game
dotnet run
```

---

