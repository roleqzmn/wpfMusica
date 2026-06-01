using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using WpfApp2.Core;
using WpfApp2.Models;
using WpfApp2.Services;

namespace WpfApp2.ViewModels;

public class PlayerViewModel : BaseViewModel
{
    private readonly IAudioPlayerService _audioPlayerService;
    private readonly DispatcherTimer _positionTimer;
    private readonly RelayCommand _playPauseCommand;
    private readonly RelayCommand _previousCommand;
    private readonly RelayCommand _nextCommand;

    private List<Song> _queue = new();
    private int _currentIndex = -1;
    private Song? _currentSong;
    private bool _isPlaying;
    private TimeSpan _position;
    private TimeSpan _duration;
    private double _volume = 45;

    public PlayerViewModel(IAudioPlayerService audioPlayerService)
    {
        _audioPlayerService = audioPlayerService;
        _audioPlayerService.PlaybackEnded += OnPlaybackEnded;

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _positionTimer.Tick += (_, _) => RefreshPlaybackStateFromService();

        _playPauseCommand = new RelayCommand(PlayPause, CanPlayPause);
        PlayPauseCommand = _playPauseCommand;
        _previousCommand = new RelayCommand(PlayPrevious, CanPlayPrevious);
        PreviousCommand = _previousCommand;
        _nextCommand = new RelayCommand(PlayNext, CanPlayNext);
        NextCommand = _nextCommand;
        SeekCommand = new RelayCommand(Seek);

        _audioPlayerService.Volume = _volume / 100.0;
    }

    public RelayCommand PlayPauseCommand { get; }
    public RelayCommand PreviousCommand { get; }
    public RelayCommand NextCommand { get; }
    public RelayCommand SeekCommand { get; }

    public Song? CurrentSong
    {
        get => _currentSong;
        private set
        {
            if (SetProperty(ref _currentSong, value))
            {
                OnPropertyChanged(nameof(CurrentSongTitle));
                OnPropertyChanged(nameof(CurrentSongArtist));
                OnPropertyChanged(nameof(CurrentSongCoverPath));
                _playPauseCommand.RaiseCanExecuteChanged();
                _previousCommand.RaiseCanExecuteChanged();
                _nextCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CurrentSongTitle => CurrentSong?.Title ?? "Nothing playing";
    public string CurrentSongArtist => CurrentSong?.Artist ?? string.Empty;
    public string CurrentSongCoverPath => CurrentSong?.CoverPath ?? "/Resources/vinyl.ico";

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            if (SetProperty(ref _isPlaying, value))
            {
                OnPropertyChanged(nameof(PlayPauseText));
            }
        }
    }

    public string PlayPauseText => IsPlaying ? "||" : ">";

    public TimeSpan Position
    {
        get => _position;
        set
        {
            if (SetProperty(ref _position, value))
            {
                OnPropertyChanged(nameof(PositionText));
                OnPropertyChanged(nameof(PositionSeconds));
                OnPropertyChanged(nameof(DurationText));
                OnPropertyChanged(nameof(RemainingText));
            }
        }
    }

    public TimeSpan Duration
    {
        get => _duration;
        private set
        {
            if (SetProperty(ref _duration, value))
            {
                OnPropertyChanged(nameof(DurationText));
                OnPropertyChanged(nameof(DurationSeconds));
                OnPropertyChanged(nameof(RemainingText));
            }
        }
    }

    public string PositionText => $"{Position:m\\:ss}";
    public string DurationText => $"{Duration:m\\:ss}";
    public string RemainingText => Duration > Position ? $"{Duration - Position:m\\:ss}" : "0:00";

    public double PositionSeconds
    {
        get => Position.TotalSeconds;
        set
        {
            var seekTo = TimeSpan.FromSeconds(Math.Max(0, value));
            Seek(seekTo);
        }
    }

    public double DurationSeconds => Math.Max(1, Duration.TotalSeconds);

    public double Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, value))
            {
                _audioPlayerService.Volume = _volume / 100.0;
            }
        }
    }

    public void PlaySong(Song song, IEnumerable<Song> queue)
    {
        var items = queue.ToList();
        if (items.Count == 0)
        {
            return;
        }

        var index = items.FindIndex(s => s.Id == song.Id);
        if (index < 0)
        {
            index = 0;
        }

        PlayQueue(items, index);
    }

    public void PlayQueue(IReadOnlyList<Song> songs, int startIndex)
    {
        if (songs.Count == 0)
        {
            return;
        }

        _queue = songs.ToList();
        _currentIndex = Math.Clamp(startIndex, 0, _queue.Count - 1);
        PlayCurrentIndex();
    }

    private bool CanPlayPause()
    {
        return CurrentSong is not null;
    }

    private void PlayPause()
    {
        if (CurrentSong is null)
        {
            return;
        }

        if (IsPlaying)
        {
            _audioPlayerService.Pause();
            IsPlaying = false;
        }
        else
        {
            _audioPlayerService.Resume();
            IsPlaying = true;
        }
    }

    private bool CanPlayPrevious()
    {
        return _currentIndex > 0;
    }

    private void PlayPrevious()
    {
        if (!CanPlayPrevious())
        {
            return;
        }

        _currentIndex--;
        PlayCurrentIndex();
    }

    private bool CanPlayNext()
    {
        return _currentIndex >= 0 && _currentIndex < _queue.Count - 1;
    }

    private void PlayNext()
    {
        if (!CanPlayNext())
        {
            return;
        }

        _currentIndex++;
        PlayCurrentIndex();
    }

    private void Seek(object? parameter)
    {
        var seconds = parameter switch
        {
            double d => d,
            float f => f,
            int i => i,
            TimeSpan ts => ts.TotalSeconds,
            _ => Position.TotalSeconds
        };

        var target = TimeSpan.FromSeconds(Math.Max(0, seconds));
        _audioPlayerService.Seek(target);
        Position = target;
    }

    private void Seek(TimeSpan target)
    {
        _audioPlayerService.Seek(target);
        Position = target;
    }

    private void PlayCurrentIndex()
    {
        if (_currentIndex < 0 || _currentIndex >= _queue.Count)
        {
            return;
        }

        CurrentSong = _queue[_currentIndex];
        _audioPlayerService.Play(CurrentSong.AudioData, CurrentSong.AudioFileExtension);
        IsPlaying = true;
        Position = TimeSpan.Zero;
        Duration = _audioPlayerService.Duration;
        _positionTimer.Start();
        _previousCommand.RaiseCanExecuteChanged();
        _nextCommand.RaiseCanExecuteChanged();
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        if (CanPlayNext())
        {
            PlayNext();
            return;
        }

        IsPlaying = false;
        _positionTimer.Stop();
    }

    private void RefreshPlaybackStateFromService()
    {
        Position = _audioPlayerService.Position;
        Duration = _audioPlayerService.Duration;
    }
}
