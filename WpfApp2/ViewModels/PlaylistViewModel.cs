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
    private Song? _selectedSong;

    public PlaylistViewModel(Playlist playlist, ObservableCollection<Song> allSongs, Action<Song?> onSelectedSongChanged)
    {
        Playlist = playlist;
        _onSelectedSongChanged = onSelectedSongChanged;

        Songs = new ObservableCollection<Song>(
            allSongs.Where(song => playlist.SongIds.Contains(song.Id)));

        PlayPlaylistCommand = new RelayCommand(PlayPlaylist, CanPlayPlaylist);
    }

    public Playlist Playlist { get; }
    public ObservableCollection<Song> Songs { get; }
    public ICommand PlayPlaylistCommand { get; }

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
}
