namespace Spice86.Shared.Diagnostics;

using Spice86.Shared.Interfaces;

/// <inheritdoc />
public class PerformanceMeasurer : IPerformanceMeasurer {
    private long _measure;
    private long _lastTimeInMilliseconds;
    private long _sampledMetricsCount;

    /// <inheritdoc />
    public long ValuePerMillisecond { get; private set; }

    /// <inheritdoc />
    public long ValuePerSecond => ValuePerMillisecond * 1000;

    /// <inheritdoc />
    public long AverageValuePerSecond { get; private set; }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public PerformanceMeasurer() => _lastTimeInMilliseconds = GetCurrentTime();

    private long _firstMeasureTimeInTicks = 0;

    private const int WindowSizeInSeconds = 30;

    private static long GetCurrentTime() => System.Environment.TickCount64;

    /// <inheritdoc />
    public void UpdateValue(long newMeasure) {
        long newTimeInMilliseconds = GetCurrentTime();
        if (_firstMeasureTimeInTicks == 0) {
            _firstMeasureTimeInTicks = newTimeInMilliseconds;
        } else if((TimeSpan.FromTicks(newTimeInMilliseconds) - TimeSpan.FromTicks(_firstMeasureTimeInTicks)).TotalSeconds >= WindowSizeInSeconds) {
            _firstMeasureTimeInTicks = newTimeInMilliseconds;
            _sampledMetricsCount = 0;
            AverageValuePerSecond = 0;
        }
        long millisecondsDelta = newTimeInMilliseconds - _lastTimeInMilliseconds;
        _lastTimeInMilliseconds = newTimeInMilliseconds;
        long valueDelta = newMeasure - _measure;
        _measure = newMeasure;
        ValuePerMillisecond = valueDelta / Math.Max(millisecondsDelta, 1);
        AverageValuePerSecond = ApproxRollingAverage(AverageValuePerSecond, ValuePerSecond, _sampledMetricsCount++);
    }

    private static long ApproxRollingAverage(long measureAverage, long valuePerSecond, long sampledMetricsCount) {
        measureAverage -= measureAverage / Math.Max(sampledMetricsCount, 1);
        measureAverage += valuePerSecond / Math.Max(sampledMetricsCount, 1);
        return measureAverage;
    }
}