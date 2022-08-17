﻿namespace Spice86.Core.Emulator.Function;
public class ByteModificationRecord {
    public byte OldValue { get; }
    public byte NewValue { get; }

    public ByteModificationRecord(byte oldValue, byte newValue) {
        OldValue = oldValue;
        NewValue = newValue;
    }

    protected bool Equals(ByteModificationRecord other) {
        return OldValue == other.OldValue && NewValue == other.NewValue;
    }

    public override bool Equals(object? obj) {
        if (obj is null) {
            return false;
        }
        if (ReferenceEquals(this, obj)) {
            return true;
        }
        if (obj.GetType() != GetType()) {
            return false;
        }
        return Equals((ByteModificationRecord)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(OldValue, NewValue);
    }
}