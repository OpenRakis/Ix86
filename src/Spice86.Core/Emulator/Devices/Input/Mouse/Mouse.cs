namespace Spice86.Core.Emulator.Devices.Input.Mouse;

using Serilog.Events;

using Spice86.Core.Emulator.InterruptHandlers.Input.Mouse;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Emulator.Mouse;
using Spice86.Shared.Interfaces;

/// <summary>
///     Basic implementation of a keyboard
/// </summary>
public class Mouse : DefaultIOPortHandler, IMouseDevice {
    private const int IrqNumber = 12;
    private readonly IGui? _gui;
    private readonly ILoggerService _logger;
    private long _lastUpdateTimestamp;
    private bool _previousIsLeftButtonDown;
    private bool _previousIsMiddleButtonDown;
    private bool _previousIsRightButtonDown;
    private double _previousMouseXRelative;
    private double _previousMouseYRelative;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Keyboard" /> class.
    /// </summary>
    /// <param name="machine">The emulator machine.</param>
    /// <param name="gui">The graphical user interface. Is null in headless mode.</param>
    /// <param name="configuration"></param>
    /// <param name="loggerService">The logger service implementation.</param>
    public Mouse(Machine machine, IGui? gui, Configuration configuration, ILoggerService loggerService) : base(machine, configuration, loggerService) {
        _gui = gui;
        MouseType = configuration.Mouse;
        _logger = loggerService.WithLogLevel(LogEventLevel.Verbose);
        Initialize();
    }

    /// <inheritdoc />
    public double DeltaY { get; private set; }

    /// <inheritdoc />
    public double DeltaX { get; private set; }

    /// <inheritdoc />
    public ushort ButtonCount { get; } = 3;

    /// <inheritdoc />
    public double MouseXRelative { get; private set; }

    /// <inheritdoc />
    public double MouseYRelative { get; private set; }

    /// <inheritdoc />
    public MouseEventMask LastTrigger { get; private set; }

    /// <inheritdoc />
    public ushort SampleRate { get; set; } = 100;

    /// <inheritdoc />
    public MouseType MouseType { get; }

    /// <inheritdoc />
    public bool IsLeftButtonDown { get; private set; }

    /// <inheritdoc />
    public bool IsRightButtonDown { get; private set; }

    /// <inheritdoc />
    public bool IsMiddleButtonDown { get; private set; }

    /// <inheritdoc />
    public ushort DoubleSpeedThreshold { get; set; }

    /// <inheritdoc />
    public ushort HorizontalMickeysPerPixel { get; set; } = 8;

    /// <inheritdoc />
    public ushort VerticalMickeysPerPixel { get; set; } = 16;

    private void Initialize() {
        if (_gui is not null && MouseType != MouseType.None) {
            _gui.MouseButtonUp += OnMouseClick;
            _gui.MouseButtonDown += OnMouseClick;
            _gui.MouseMoved += OnMouseMoved;
        }
        if (_logger.IsEnabled(LogEventLevel.Information)) {
            _logger.Information("Mouse initialized: {MouseType}", MouseType);
        }
    }

    private void OnMouseMoved(object? sender, MouseMoveEventArgs eventArgs) {
        MouseXRelative = eventArgs.X;
        MouseYRelative = eventArgs.Y;
        UpdateMouse();
    }

    private void OnMouseClick(object? sender, MouseButtonEventArgs eventArgs) {
        switch (eventArgs.Button) {
            case MouseButton.Left:
                IsLeftButtonDown = eventArgs.ButtonDown;
                break;
            case MouseButton.Right:
                IsRightButtonDown = eventArgs.ButtonDown;
                break;
            case MouseButton.Middle:
                IsMiddleButtonDown = eventArgs.ButtonDown;
                break;
            case MouseButton.None:
            case MouseButton.XButton1:
            case MouseButton.XButton2:
            default: {
                if (_logger.IsEnabled(LogEventLevel.Information)) {
                    _logger.Information("Unknown mouse button clicked: {@EventArgs}", eventArgs);
                    return;
                }
                break;
            }
        }
        UpdateMouse();
    }

    private void UpdateMouse() {
        if (_machine.IsPaused) {
            return;
        }

        long timestamp = DateTime.Now.Ticks;
        // Check sample rate to see if we need to send an update yet.
        long ticksDuration = timestamp - _lastUpdateTimestamp;
        long threshold = TimeSpan.TicksPerSecond / SampleRate;
        if (ticksDuration < threshold) {
            return;
        }
        _lastUpdateTimestamp = timestamp;

        MouseEventMask trigger = 0;
        DeltaX = MouseXRelative - _previousMouseXRelative;
        DeltaY = MouseYRelative - _previousMouseYRelative;
        if (Math.Abs(DeltaX) > double.Epsilon || Math.Abs(DeltaY) > double.Epsilon) {
            trigger |= MouseEventMask.Movement;
        }
        _previousMouseXRelative = MouseXRelative;
        _previousMouseYRelative = MouseYRelative;

        if (IsLeftButtonDown != _previousIsLeftButtonDown) {
            trigger |= IsLeftButtonDown ? MouseEventMask.LeftButtonDown : MouseEventMask.LeftButtonUp;
        }
        _previousIsLeftButtonDown = IsLeftButtonDown;

        if (IsRightButtonDown != _previousIsRightButtonDown) {
            trigger |= IsRightButtonDown ? MouseEventMask.RightButtonDown : MouseEventMask.RightButtonUp;
        }
        _previousIsRightButtonDown = IsRightButtonDown;

        if (IsMiddleButtonDown != _previousIsMiddleButtonDown) {
            trigger |= IsMiddleButtonDown ? MouseEventMask.MiddleButtonDown : MouseEventMask.MiddleButtonUp;
        }
        _previousIsMiddleButtonDown = IsMiddleButtonDown;

        LastTrigger = trigger;
        TriggerInterruptRequest();
    }

    private void TriggerInterruptRequest() {
        _machine.DualPic.ProcessInterruptRequest(IrqNumber);
    }
}