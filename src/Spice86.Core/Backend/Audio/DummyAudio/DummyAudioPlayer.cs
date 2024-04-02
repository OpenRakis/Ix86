using Spice86.Shared.Emulator.Audio;

namespace Spice86.Core.Backend.Audio.DummyAudio;

/// <summary>
/// Dummy audio player with no backend
/// </summary>
sealed class DummyAudioPlayer : AudioPlayer {
    /// <summary>
    /// Initializes a new instance of the <see cref="DummyAudioPlayer"/> class.
    /// </summary>
    /// <param name="format">The audio playback format.</param>
    public DummyAudioPlayer(AudioFormat format) : base(format) {
    }

    /// <summary>
    /// Fakes writing data to the audio device
    /// </summary>
    /// <param name="data">The input audio data</param>
    /// <returns>The data paramater length</returns>
    internal override int WriteDataInternal(AudioFrame<float> data) {
        // Tell we wrote it all, it's all fake anyway
        return data.Length;
    }
}