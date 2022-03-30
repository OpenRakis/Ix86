﻿using System.Transactions;

namespace Spice86.Emulator.Devices.Timer;

using Spice86.Emulator.CPU;

/// <summary>
/// Counter activator based on emulated cycles
/// </summary>
public class CyclesCounterActivator : CounterActivator {
    private readonly long _instructionsPerSecond;
    private readonly State _state;
    private long _cyclesBetweenActivations;
    private long _lastActivationCycle;

    public CyclesCounterActivator(State state, long instructionsPerSecond, double multiplier) : base(multiplier) {
        this._state = state;
        this._instructionsPerSecond = instructionsPerSecond;
    }

    public override bool IsActivated {
        get {
            if (IsFrozen) {
                return false;
            }
            long currentCycles = _state.Cycles;
            long elapsedInstructions = _state.Cycles - _lastActivationCycle;
            if (elapsedInstructions <= _cyclesBetweenActivations) {
                return false;
            }

            _lastActivationCycle = currentCycles;
            return true;
        }
    }

    public override void UpdateDesiredFrequency(long desiredFrequency) {
        _cyclesBetweenActivations = (long) (this._instructionsPerSecond / (Multiplier * desiredFrequency));
    }
}