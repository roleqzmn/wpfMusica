using System;
using System.IO;
using System.Windows.Media;

namespace WpfApp2.Services;

public class AudioPlayerService : IAudioPlayerService
{
    private readonly MediaPlayer _mediaPlayer = new();
    private string? _currentTempFile;
    private bool _disposed;

    public AudioPlayerService()
    {
        _mediaPlayer.MediaEnded += (_, _) => PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? PlaybackEnded;

    public bool IsPlaying { get; private set; }

    public TimeSpan Position => _mediaPlayer.Position;

    public TimeSpan Duration => _mediaPlayer.NaturalDuration.HasTimeSpan
        ? _mediaPlayer.NaturalDuration.TimeSpan
        : TimeSpan.Zero;

    public double Volume
    {
        get => _mediaPlayer.Volume;
        set => _mediaPlayer.Volume = Math.Clamp(value, 0.0, 1.0);
    }

    public void Play(byte[] audioData, string fileExtension)
    {
        CleanupTempFile();

        var extension = string.IsNullOrWhiteSpace(fileExtension) ? ".mp3" : fileExtension;
        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "Musica", "audio");
        Directory.CreateDirectory(tempDirectory);
        _currentTempFile = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(_currentTempFile, audioData);

        _mediaPlayer.Open(new Uri(_currentTempFile, UriKind.Absolute));
        _mediaPlayer.Play();
        IsPlaying = true;
    }

    public void Pause()
    {
        _mediaPlayer.Pause();
        IsPlaying = false;
    }

    public void Resume()
    {
        _mediaPlayer.Play();
        IsPlaying = true;
    }

    public void Stop()
    {
        _mediaPlayer.Stop();
        IsPlaying = false;
    }

    public void Seek(TimeSpan position)
    {
        if (position < TimeSpan.Zero)
        {
            position = TimeSpan.Zero;
        }

        var duration = Duration;
        if (duration > TimeSpan.Zero && position > duration)
        {
            position = duration;
        }

        _mediaPlayer.Position = position;
    }

    private void CleanupTempFile()
    {
        if (string.IsNullOrWhiteSpace(_currentTempFile))
        {
            return;
        }

        try
        {
            if (File.Exists(_currentTempFile))
            {
                File.Delete(_currentTempFile);
            }
        }
        catch
        {
        }

        _currentTempFile = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _mediaPlayer.Close();
        CleanupTempFile();
        _disposed = true;
    }
}
