using System;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfApp2.Core;
using WpfApp2.Models;

namespace WpfApp2.ViewModels;

public class PlaylistViewModel : BaseViewModel
{
    private readonly Action<Song?> _onSelectedSongChanged;
    private readonly Action _onPlaylistContentChanged;
    private readonly RelayCommand _playPlaylistCommand;
    private readonly RelayCommand _removeSongCommand;
    private readonly RelayCommand _moveSongUpCommand;
    private readonly RelayCommand _moveSongDownCommand;
    private Song? _selectedSong;

    public PlaylistViewModel(
        Playlist playlist,
        ObservableCollection<Song> allSongs,
        Action<Song?> onSelectedSongChanged,
        Action onPlaylistContentChanged)
    {
        Playlist = playlist;
        _onSelectedSongChanged = onSelectedSongChanged;
        _onPlaylistContentChanged = onPlaylistContentChanged;

        Songs = new ObservableCollection<Song>(
            playlist.SongIds
                .Select(id => allSongs.FirstOrDefault(song => song.Id == id))
                .OfType<Song>());

        Songs.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SongCount));
            OnPropertyChanged(nameof(SongCountText));
            _playPlaylistCommand.RaiseCanExecuteChanged();
            _removeSongCommand.RaiseCanExecuteChanged();
            _moveSongUpCommand.RaiseCanExecuteChanged();
            _moveSongDownCommand.RaiseCanExecuteChanged();
        };

        _playPlaylistCommand = new RelayCommand(PlayPlaylist, CanPlayPlaylist);
        PlayPlaylistCommand = _playPlaylistCommand;
        _removeSongCommand = new RelayCommand(RemoveSelectedSong, CanRemoveSelectedSong);
        RemoveSongCommand = _removeSongCommand;
        _moveSongUpCommand = new RelayCommand(MoveSelectedSongUp, CanMoveSelectedSongUp);
        MoveSongUpCommand = _moveSongUpCommand;
        _moveSongDownCommand = new RelayCommand(MoveSelectedSongDown, CanMoveSelectedSongDown);
        MoveSongDownCommand = _moveSongDownCommand;
    }

    public Playlist Playlist { get; }
    public ObservableCollection<Song> Songs { get; }
    public ICommand PlayPlaylistCommand { get; }
    public ICommand RemoveSongCommand { get; }
    public ICommand MoveSongUpCommand { get; }
    public ICommand MoveSongDownCommand { get; }

    public string Name => Playlist.Name;
    public string CoverPath => Playlist.CoverPath;
    public int SongCount => Songs.Count;
    public string SongCountText => SongCount == 1 ? "1 song" : $"{SongCount} songs";

    public Song? SelectedSong
    {
        get => _selectedSong;
        set
        {
            if (SetProperty(ref _selectedSong, value))
            {
                _onSelectedSongChanged(value);
                _removeSongCommand.RaiseCanExecuteChanged();
                _moveSongUpCommand.RaiseCanExecuteChanged();
                _moveSongDownCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool CanPlayPlaylist()
    {
        return Songs.Count > 0;
    }

    private void PlayPlaylist()
    {
        if (Songs.Count == 0)
        {
            return;
        }

        SelectedSong = Songs[0];
    }

    private bool CanRemoveSelectedSong()
    {
        return SelectedSong is not null;
    }

    private void RemoveSelectedSong()
    {
        if (SelectedSong is null)
        {
            return;
        }

        var index = Songs.IndexOf(SelectedSong);
        if (index < 0)
        {
            return;
        }

        Songs.RemoveAt(index);
        Playlist.SongIds.RemoveAt(index);
        _onPlaylistContentChanged();
        SelectedSong = null;
    }

    private bool CanMoveSelectedSongUp()
    {
        if (SelectedSong is null)
        {
            return false;
        }

        return Songs.IndexOf(SelectedSong) > 0;
    }

    private void MoveSelectedSongUp()
    {
        if (!CanMoveSelectedSongUp() || SelectedSong is null)
        {
            return;
        }

        var index = Songs.IndexOf(SelectedSong);
        Songs.Move(index, index - 1);
        Playlist.SongIds.Move(index, index - 1);
        _onPlaylistContentChanged();
    }

    private bool CanMoveSelectedSongDown()
    {
        if (SelectedSong is null)
        {
            return false;
        }

        var index = Songs.IndexOf(SelectedSong);
        return index >= 0 && index < Songs.Count - 1;
    }

    private void MoveSelectedSongDown()
    {
        if (!CanMoveSelectedSongDown() || SelectedSong is null)
        {
            return;
        }

        var index = Songs.IndexOf(SelectedSong);
        Songs.Move(index, index + 1);
        Playlist.SongIds.Move(index, index + 1);
        _onPlaylistContentChanged();
    }
}
