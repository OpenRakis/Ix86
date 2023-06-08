﻿namespace Spice86.Core.Emulator.InterruptHandlers.Input.Mouse;

using Spice86.Core.Emulator.InterruptHandlers;
using Spice86.Core.Emulator.VM;

using Spice86.Core.Emulator.Callback;
using Spice86.Shared.Interfaces;

/// <summary>
/// Interface between the mouse and the emulator.<br/>
/// Re-implements int33.<br/>
/// </summary>
public class MouseInt33Handler : InterruptHandler {
    private const ushort MOUSE_RANGE_X = 639;
    private const ushort MOUSE_RANGE_Y = 199;
    private readonly IGui? _gui;
    private ushort _mouseMaxX = MOUSE_RANGE_X;
    private ushort _mouseMaxY = MOUSE_RANGE_Y;
    private ushort _mouseMinX;
    private ushort _mouseMinY;
    private ushort _userCallbackMask;
    private ushort _userCallbackOffset;
    private ushort _userCallbackSegment;

    public MouseInt33Handler(Machine machine, ILoggerService loggerService, IGui? gui) : base(machine, loggerService) {
        _gui = gui;
        _dispatchTable.Add(0x00, new Callback(0x00, MouseInstalledFlag));
        _dispatchTable.Add(0x01, new Callback(0x01, ShowMouseCursor));
        _dispatchTable.Add(0x02, new Callback(0x02, HideMouseCursor));
        _dispatchTable.Add(0x03, new Callback(0x03, GetMousePositionAndStatus));
        _dispatchTable.Add(0x04, new Callback(0x04, SetMouseCursorPosition));
        _dispatchTable.Add(0x07, new Callback(0x07, SetMouseHorizontalMinMaxPosition));
        _dispatchTable.Add(0x08, new Callback(0x08, SetMouseVerticalMinMaxPosition));
        _dispatchTable.Add(0x0C, new Callback(0x0C, SetMouseUserDefinedSubroutine));
        _dispatchTable.Add(0x0F, new Callback(0x0F, SetMouseMickeyPixelRatio));
        _dispatchTable.Add(0x13, new Callback(0x13, SetMouseDoubleSpeedThreshold));
        _dispatchTable.Add(0x14, new Callback(0x14, SwapMouseUserDefinedSubroutine));
        _dispatchTable.Add(0x1A, new Callback(0x1A, SetMouseSensitivity));
        _dispatchTable.Add(0x1C, new Callback(0x1C, SetInterruptRate));
        _dispatchTable.Add(0x24, new Callback(0x24, GetSoftwareVersionAndMouseType));
        _dispatchTable.Add(0xA1, new Callback(0xA1, Unsupported));
    }

    /// <summary>
    /// NOP
    /// </summary>
    public void Unsupported() {
    }

    public void GetSoftwareVersionAndMouseType() {
        _state.BX = 0x805; //Version 8.05
        _state.CH = 0x04;  /* PS/2 type */
        _state.CL = 0;     /* PS/2 (unused) */
    }

    public override byte Index => 0x33;

    /// <summary>
    /// Interrupt rate is set by the host OS. NOP.
    /// </summary>
    public void SetInterruptRate() {
        // NOP
    }

    public void GetMousePositionAndStatus() {
        if (_gui is null) {
            return;
        }
        ushort x = RestrictValue((ushort)_gui.MouseX, (ushort)_gui.Width, _mouseMinX, _mouseMaxX);
        ushort y = RestrictValue((ushort)_gui.MouseY, (ushort)_gui.Height, _mouseMinY, _mouseMaxY);
        bool leftClick = _gui.IsLeftButtonClicked;
        bool rightClick = _gui.IsRightButtonClicked;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("GET MOUSE POSITION AND STATUS {MouseX}, {MouseY}, {LeftClick}, {RightClick}", x, y, leftClick, rightClick);
        }
        _state.CX = x;
        _state.DX = y;
        _state.BX = (ushort)((leftClick ? 1 : 0) | (rightClick ? 1 : 0) << 1);
    }

    public void MouseInstalledFlag() {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("MOUSE INSTALLED FLAG");
        }
        _state.AX = 0xFFFF;

        // 3 buttons
        _state.BX = 3;
    }

    public override void Run() {
        byte operation = _state.AL;
        Run(operation);
    }

    public void SetMouseCursorPosition() {
        if (_gui is null) {
            return;
        }

        ushort x = _state.CX;
        ushort y = _state.DX;

        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SET MOUSE CURSOR POSITION {MouseX}, {MouseY}", x, y);
        }

        _gui.MouseX = x;
        _gui.MouseY = y;
    }

    public void SetMouseDoubleSpeedThreshold() {
        ushort threshold = _state.DX;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SET MOUSE DOUBLE SPEED THRESHOLD {Threshold}", threshold);
        }
    }

    public void SetMouseHorizontalMinMaxPosition() {
        _mouseMinX = _state.CX;
        _mouseMaxX = _state.DX;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SET MOUSE HORIZONTAL MIN MAX POSITION {MinX}, {MaxX}", _mouseMinX, _mouseMaxX);
        }
    }

    public void SetMouseMickeyPixelRatio() {
        ushort rx = _state.CX;
        ushort ry = _state.DX;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SET MOUSE MICKEY PIXEL RATIO {Rx}, {Ry}", rx, ry);
        }
    }

    public void SetMouseSensitivity() {
        ushort horizontalSpeed = _state.BX;
        ushort verticalSpeed = _state.CX;
        ushort threshold = _state.DX;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SET MOUSE SENSITIVITY {HorizontalSpeed}, {VerticalSpeed}, {Threshold}", horizontalSpeed, verticalSpeed, threshold);
        }
    }

    public void SetMouseUserDefinedSubroutine() {
        _userCallbackMask = _state.CX;
        _userCallbackSegment = _state.ES;
        _userCallbackOffset = _state.DX;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SET MOUSE USER DEFINED SUBROUTINE (unimplemented!) {Mask}, {Segment}, {Offset}", _userCallbackMask, _userCallbackSegment, _userCallbackOffset);
        }
    }

    public void SetMouseVerticalMinMaxPosition() {
        _mouseMinY = _state.CX;
        _mouseMaxY = _state.DX;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SET MOUSE VERTICAL MIN MAX POSITION {MinY}, {MaxY}", _mouseMinY, _mouseMaxY);
        }
    }

    public void SwapMouseUserDefinedSubroutine() {
        ushort newUserCallbackMask = _state.CX;
        ushort newUserCallbackSegment = _state.ES;
        ushort newUserCallbackOffset = _state.DX;
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SWAP MOUSE USER DEFINED SUBROUTINE (unimplemented!) {Mask}, {Segment}, {Offset}", newUserCallbackMask, newUserCallbackSegment, newUserCallbackOffset);
        }
        _state.CX = _userCallbackMask;
        _state.ES = _userCallbackSegment;
        _state.DX = _userCallbackOffset;
        _userCallbackMask = newUserCallbackMask;
        _userCallbackOffset = newUserCallbackOffset;
        _userCallbackSegment = newUserCallbackSegment;
    }

    public void ShowMouseCursor() {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("SHOW MOUSE CURSOR");
        }
        _gui?.ShowMouseCursor();
    }

    public void HideMouseCursor() {
        if (_loggerService.IsEnabled(Serilog.Events.LogEventLevel.Verbose)) {
            _loggerService.Verbose("HIDE MOUSE CURSOR");
        }
        _gui?.HideMouseCursor();
    }

    /// <summary>
    /// Translates values from the GUI into values for the emulated display area
    /// </summary>
    /// <param name="value">Raw value from the GUI</param>
    /// <param name="maxValue">Max of what that value can be</param>
    /// <param name="min">min expected by program</param>
    /// <param name="max">max expected by program</param>
    /// <returns>Value within the emulated display surface area</returns>
    private static ushort RestrictValue(ushort value, ushort maxValue, ushort min, ushort max) {
        int range = max - min;
        ushort valueInRange = value;
        if (maxValue is not 0) {
            valueInRange = (ushort)(value * range / maxValue);
        }
        if (valueInRange > max) {
            return Math.Min(max, value);
        }

        if (valueInRange < min) {
            return Math.Max(min, value);
        }

        return valueInRange;
    }
}