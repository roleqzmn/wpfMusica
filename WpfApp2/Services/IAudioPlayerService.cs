namespace WpfApp2.Services;

public interface IAudioPlayerService : IDisposable
{
    event EventHandler? PlaybackEnded;

    bool IsPlaying { get; }
    TimeSpan Position { get; }
    TimeSpan Duration { get; }
    double Volume { get; set; }

    void Play(byte[] audioData, string fileExtension);
    void Pause();
    void Resume();
    void Stop();
    void Seek(TimeSpan position);
}
