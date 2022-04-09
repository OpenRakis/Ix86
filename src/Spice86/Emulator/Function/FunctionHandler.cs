﻿namespace Spice86.Emulator.Function;

using Serilog;

using Spice86.Emulator.CPU;
using Spice86.Emulator.VM;
using Spice86.Emulator.Memory;
using Spice86.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FunctionHandler {
    private static readonly ILogger _logger = Program.Logger.ForContext<FunctionHandler>();

    private readonly Stack<FunctionCall> _callerStack = new();

    private readonly bool _recordData;

    private readonly Machine _machine;

    public FunctionHandler(Machine machine, bool recordData) {
        this._machine = machine;
        this._recordData = recordData;
    }

    public void Call(CallType callType, ushort entrySegment, ushort entryOffset, ushort expectedReturnSegment, ushort expectedReturnOffset) {
        Call(callType, entrySegment, entryOffset, expectedReturnSegment, expectedReturnOffset, null, true);
    }

    public void Call(CallType callType, ushort entrySegment, ushort entryOffset, ushort? expectedReturnSegment, ushort? expectedReturnOffset, string? name, bool recordReturn) {
        SegmentedAddress entryAddress = new(entrySegment, entryOffset);
        FunctionInformation currentFunction = GetOrCreateFunctionInformation(entryAddress, name);
        if (_recordData) {
            FunctionInformation? caller = GetFunctionInformation(CurrentFunctionCall);
            SegmentedAddress? expectedReturnAddress = null;
            if (expectedReturnSegment != null && expectedReturnOffset != null) {
                expectedReturnAddress = new SegmentedAddress(expectedReturnSegment.Value, expectedReturnOffset.Value);
            }

            FunctionCall currentFunctionCall = new(callType, entryAddress, expectedReturnAddress, CurrentStackAddress, recordReturn);
            _callerStack.Push(currentFunctionCall);
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
                _logger.Debug("Calling {@CurrentFunction} from {@Caller}", currentFunction, caller);
            }

            currentFunction.Enter(caller);
        }

        if (UseCodeOverride) {
            currentFunction.CallOverride();
        }
    }

    private FunctionInformation GetOrCreateFunctionInformation(SegmentedAddress entryAddress, string? name) {
        if (!FunctionInformations.TryGetValue(entryAddress, out FunctionInformation? res)) {
            res = new FunctionInformation(entryAddress, string.IsNullOrWhiteSpace(name) ? "unknown" : name);
            FunctionInformations.Add(entryAddress, res);
        }
        return res;
    }

    public string DumpCallStack() {
        StringBuilder res = new();
        foreach (FunctionCall functionCall in this._callerStack) {
            SegmentedAddress? returnAddress = functionCall.ExpectedReturnAddress;
            FunctionInformation? functionInformation = GetFunctionInformation(functionCall);
            res.Append(" - ");
            res.Append(functionInformation);
            res.Append(" expected to return to address ");
            res.Append(returnAddress);
            res.Append('\n');
        }

        return res.ToString();
    }

    public IDictionary<SegmentedAddress, FunctionInformation> FunctionInformations { get; set; } = new Dictionary<SegmentedAddress, FunctionInformation>();

    public void Icall(CallType callType, ushort entrySegment, ushort entryOffset, ushort expectedReturnSegment, ushort expectedReturnOffset, byte vectorNumber, bool recordReturn) {
        Call(callType, entrySegment, entryOffset, expectedReturnSegment, expectedReturnOffset, $"interrupt_handler_{ConvertUtils.ToHex(vectorNumber)}", recordReturn);
    }

    public SegmentedAddress? PeekReturnAddressOnMachineStack(CallType returnCallType) {
        uint stackPhysicalAddress = StackPhysicalAddress;
        return PeekReturnAddressOnMachineStack(returnCallType, stackPhysicalAddress);
    }

    public SegmentedAddress? PeekReturnAddressOnMachineStack(CallType returnCallType, uint stackPhysicalAddress) {
        Memory memory = _machine.Memory;
        State state = _machine.Cpu.State;
        return returnCallType switch {
            CallType.NEAR => new SegmentedAddress(state.CS, memory.GetUint16(stackPhysicalAddress)),
            CallType.FAR or CallType.INTERRUPT => new SegmentedAddress(
                memory.GetUint16(stackPhysicalAddress + 2),
                memory.GetUint16(stackPhysicalAddress)),
            CallType.MACHINE => null,
            _ => null
        };
    }

    public SegmentedAddress? PeekReturnAddressOnMachineStackForCurrentFunction() {
        FunctionCall? currentFunctionCall = CurrentFunctionCall;
        if (currentFunctionCall == null) {
            return null;
        }

        return PeekReturnAddressOnMachineStack(currentFunctionCall.CallType);
    }

    public bool Ret(CallType returnCallType) {
        if (_recordData) {
            if (_callerStack.TryPop(out FunctionCall? currentFunctionCall) == false) {
                if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Warning)) {
                    _logger.Warning("Returning but no call was done before!!");
                }
                return false;
            }
            FunctionInformation? currentFunctionInformation = GetFunctionInformation(currentFunctionCall);
            bool returnAddressAlignedWithCallStack = AddReturn(returnCallType, currentFunctionCall, currentFunctionInformation);
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
                _logger.Debug("Returning from {@CurrentFunctionInformation} to {@CurrentFunctionCall}", currentFunctionInformation, GetFunctionInformation(CurrentFunctionCall));
            }

            if (!returnAddressAlignedWithCallStack) {
                // Put it back in the stack, we did a jump not a return
                _callerStack.Push(currentFunctionCall);
            }
        }
        return true;
    }

    public bool UseCodeOverride { get; set; }

    private bool AddReturn(CallType returnCallType, FunctionCall currentFunctionCall, FunctionInformation? currentFunctionInformation) {
        FunctionReturn currentFunctionReturn = GenerateCurrentFunctionReturn(returnCallType);
        SegmentedAddress? actualReturnAddress = PeekReturnAddressOnMachineStack(returnCallType);
        bool returnAddressAlignedWithCallStack = IsReturnAddressAlignedWithCallStack(currentFunctionCall, actualReturnAddress, currentFunctionReturn);
        if (currentFunctionInformation != null && !UseOverride(currentFunctionInformation)) {
            SegmentedAddress? addressToRecord = actualReturnAddress;
            if (!currentFunctionCall.IsRecordReturn) {
                addressToRecord = null;
            }

            if (returnAddressAlignedWithCallStack) {
                currentFunctionInformation.AddReturn(currentFunctionReturn, addressToRecord);
            } else {
                currentFunctionInformation.AddUnalignedReturn(currentFunctionReturn, addressToRecord);
            }
        }

        return returnAddressAlignedWithCallStack;
    }

    private FunctionReturn GenerateCurrentFunctionReturn(CallType returnCallType) {
        Cpu cpu = _machine.Cpu;
        State state = cpu.State;
        ushort cs = state.CS;
        ushort ip = state.IP;
        return new FunctionReturn(returnCallType, new SegmentedAddress(cs, ip));
    }

    private FunctionCall? CurrentFunctionCall {
        get {

            if (_callerStack.Any() == false) {
                return null;
            }
            return _callerStack.TryPeek(out FunctionCall? firstElement) ? firstElement : null;
        }
    }


    private SegmentedAddress CurrentStackAddress {
        get {
            State state = _machine.Cpu.State;
            return new SegmentedAddress(state.SS, state.SP);
        }
    }

    private FunctionInformation? GetFunctionInformation(FunctionCall? functionCall) {
        if (functionCall == null) {
            return null;
        }
        if (FunctionInformations.TryGetValue(functionCall.EntryPointAddress, out FunctionInformation? value)) {
            return value;
        }
        return null;
    }

    private uint StackPhysicalAddress => _machine.Cpu.State.StackPhysicalAddress;

    private bool IsReturnAddressAlignedWithCallStack(FunctionCall currentFunctionCall, SegmentedAddress? actualReturnAddress, FunctionReturn currentFunctionReturn) {
        SegmentedAddress? expectedReturnAddress = currentFunctionCall.ExpectedReturnAddress;

        // Null check necessary for machine stop call, in this case it won't be equals to what is in
        // the stack but it's expected.
        if (actualReturnAddress != null && !actualReturnAddress.Equals(expectedReturnAddress)) {
            FunctionInformation? currentFunctionInformation = GetFunctionInformation(currentFunctionCall);
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information) && currentFunctionInformation != null
                && !currentFunctionInformation.UnalignedReturns.ContainsKey(currentFunctionReturn)) {
                CallType callType = currentFunctionCall.CallType;
                SegmentedAddress stackAddressAfterCall = currentFunctionCall.StackAddressAfterCall;
                SegmentedAddress? returnAddressOnCallTimeStack = PeekReturnAddressOnMachineStack(callType, stackAddressAfterCall.ToPhysical());
                SegmentedAddress currentStackAddress = CurrentStackAddress;
                string additionalInformation = Environment.NewLine;
                if (!currentStackAddress.Equals(stackAddressAfterCall)) {
                    int delta = (int)Math.Abs((long)currentStackAddress.ToPhysical() - (long)stackAddressAfterCall.ToPhysical());
                    additionalInformation +=
                        $"Stack is not pointing at the same address as it was at call time. Delta is {delta} bytes{Environment.NewLine}";
                }
                if (!Object.Equals(expectedReturnAddress, returnAddressOnCallTimeStack)) {
                    additionalInformation += "Return address on stack was modified";
                }
                _logger.Information(@"PROGRAM IS NOT WELL BEHAVED SO CALL STACK COULD NOT BE TRACEABLE ANYMORE!
                        Current function {@CurrentFunctionInformation} return {@CurrentFunctionReturn} will not go to the expected place:
                        - At {@CallType} call time, return was supposed to be {@ExpectedReturnAddress} stored at SS:SP {@StackAddressAfterCall}. Value there is now {@ReturnAddressOnCallTimeStack}
                        - On the stack it is now {@ActualReturnAddress} stored at SS:SP {@CurrentStackAddress}
                        {@AdditionalInformation}
                    ",
                    currentFunctionInformation.ToString(), currentFunctionReturn.ToString(),
                    callType.ToString(), expectedReturnAddress?.ToString(), stackAddressAfterCall.ToString(), returnAddressOnCallTimeStack?.ToString(),
                    actualReturnAddress.ToString(), currentStackAddress.ToString(),
                    additionalInformation);
            }
            return false;
        }
        return true;
    }

    private bool UseOverride(FunctionInformation? functionInformation) {
        return UseCodeOverride && functionInformation != null && functionInformation.HasOverride;
    }
}