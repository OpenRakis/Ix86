﻿namespace Spice86.Core.Emulator.Devices.Sound.Blaster;

/// <summary>
/// A static class that contains methods for resampling audio data using linear interpolation.
/// </summary>
/// <remarks>
/// An adequate but not great audio resampler.
/// </remarks>
internal static class LinearUpsampler {
    /// <summary>
    /// Resamples mono 8-bit audio data from the source rate to the destination rate using linear interpolation.
    /// </summary>
    /// <param name="sourceRate">The sample rate of the source data.</param>
    /// <param name="destRate">The desired sample rate of the resampled data.</param>
    /// <param name="source">The source audio data to be resampled.</param>
    /// <param name="dest">The resampled audio data buffer.</param>
    /// <returns>The number of resampled samples.</returns>
    public static int Resample8Mono(int sourceRate, int destRate, ReadOnlySpan<byte> source, Span<short> dest) {
        double src2Dest = destRate / (double)sourceRate;
        double dest2Src = sourceRate / (double)destRate;

        int length = (int)(src2Dest * source.Length);

        for (int i = 0; i < length; i++) {
            int srcIndex = (int)(i * dest2Src);
            double remainder = i * dest2Src % 1;

            short value1 = Convert8To16(source[srcIndex]);
            if (srcIndex < source.Length - 1) {
                short value2 = Convert8To16(source[srcIndex + 1]);

                short newValue = Interpolate(value1, value2, remainder);
                dest[i << 1] = newValue;
                dest[(i << 1) + 1] = newValue;
            } else {
                dest[i << 1] = value1;
                dest[(i << 1) + 1] = value1;
            }
        }

        return length * 2;
    }

    /// <summary>
    /// Resamples stereo 8-bit audio data from the source rate to the destination rate using linear interpolation.
    /// </summary>
    /// <param name="sourceRate">The sample rate of the source data.</param>
    /// <param name="destRate">The desired sample rate of the resampled data.</param>
    /// <param name="source">The source audio data to be resampled.</param>
    /// <param name="dest">The resampled audio data buffer.</param>
    /// <returns>The number of resampled samples.</returns>
    public static int Resample8Stereo(int sourceRate, int destRate, ReadOnlySpan<byte> source, Span<short> dest) {
        double src2Dest = destRate / (double)sourceRate;
        double dest2Src = sourceRate / (double)destRate;

        int length = (int)(src2Dest * source.Length) / 2;

        for (int i = 0; i < length; i++) {
            int srcIndex = (int)(i * dest2Src) << 1;

            short value1Left = Convert8To16(source[srcIndex]);
            short value1Right = Convert8To16(source[srcIndex + 1]);
            if (srcIndex < source.Length - 3) {
                double remainder = i * dest2Src % 1;
                short value2Left = Convert8To16(source[srcIndex + 2]);
                short value2Right = Convert8To16(source[srcIndex + 3]);

                dest[i << 1] = Interpolate(value1Left, value2Left, remainder);
                dest[(i << 1) + 1] = Interpolate(value1Right, value2Right, remainder);
            } else {
                dest[i << 1] = value1Left;
                dest[(i << 1) + 1] = value1Right;
            }
        }

        return length * 2;
    }

    /// <summary>
    /// Resamples mono 16-bit audio data from the source rate to the destination rate using linear interpolation.
    /// </summary>
    /// <param name="sourceRate">The sample rate of the source data.</param>
    /// <param name="destRate">The desired sample rate of the resampled data.</param>
    /// <param name="source">The source audio data to be resampled.</param>
    /// <param name="dest">The resampled audio data buffer.</param>
    /// <returns>The number of resampled samples.</returns>
    public static int Resample16Mono(int sourceRate, int destRate, ReadOnlySpan<short> source, Span<short> dest) {
        double src2Dest = destRate / (double)sourceRate;
        double dest2Src = sourceRate / (double)destRate;

        int length = (int)(src2Dest * source.Length);

        for (int i = 0; i < length; i++) {
            int srcIndex = (int)(i * dest2Src);
            double remainder = i * dest2Src % 1;

            short value1 = source[srcIndex];
            if (srcIndex < source.Length - 1) {
                short value2 = source[srcIndex + 1];

                short newValue = Interpolate(value1, value2, remainder);
                dest[i << 1] = newValue;
                dest[(i << 1) + 1] = newValue;
            } else {
                dest[i << 1] = value1;
                dest[(i << 1) + 1] = value1;
            }
        }

        return length * 2;
    }

    /// <summary>
    /// Resamples stereo 16-bit audio data from the source rate to the destination rate using linear interpolation.
    /// </summary>
    /// <param name="sourceRate">The sample rate of the source data.</param>
    /// <param name="destRate">The desired sample rate of the resampled data.</param>
    /// <param name="source">The source audio data to be resampled.</param>
    /// <param name="dest">The resampled audio data buffer.</param>
    /// <returns>The number of resampled samples.</returns>
    public static int Resample16Stereo(int sourceRate, int destRate, ReadOnlySpan<short> source, Span<short> dest) {
        double src2Dest = destRate / (double)sourceRate;
        double dest2Src = sourceRate / (double)destRate;

        int length = (int)(src2Dest * source.Length) / 2;

        for (int i = 0; i < length; i++) {
            int srcIndex = (int)(i * dest2Src) << 1;

            short value1Left = source[srcIndex];
            short value1Right = source[srcIndex + 1];
            if (srcIndex < source.Length - 3) {
                double remainder = i * dest2Src % 1;
                short value2Left = source[srcIndex + 2];
                short value2Right = source[srcIndex + 3];

                dest[i << 1] = Interpolate(value1Left, value2Left, remainder);
                dest[(i << 1) + 1] = Interpolate(value1Right, value2Right, remainder);
            } else {
                dest[i << 1] = value1Left;
                dest[(i << 1) + 1] = value1Right;
            }
        }

        return length * 2;
    }

    /// <summary>
    /// Performs linear interpolation between two samples.
    /// </summary>
    /// <param name="a">The first sample.</param>
    /// <param name="b">The second sample.</param>
    /// <param name="factor">The interpolation factor between 0 and 1.</param>
    /// <returns>The interpolated sample.</returns>
    private static short Interpolate(short a, short b, double factor) => (short)(((b - a) * factor) + a);

    /// <summary>
    /// Converts an 8-bit value to a 16-bit signed value.
    /// </summary>
    /// <param name="s">The 8-bit value to convert.</param>
    /// <returns>The resulting 16-bit signed value.</returns>
    private static short Convert8To16(byte s) => (short)(s - 128 << 8);
}
