using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
using WpfApp2.Core;
using WpfApp2.Models;

namespace WpfApp2.ViewModels;

public class SongListViewModel : BaseViewModel
{
    private readonly Action<Song?> _onSelectedSongChanged;
    private readonly Action _onPlaylistContentChanged;
    private readonly RelayCommand _addSelectedSongToPlaylistCommand;
    private readonly RelayCommand _clearSearchCommand;
    private readonly RelayCommand _playSongCommand;
    private readonly Action<Song?> _onDeleteSongRequested;
    private readonly Action<Song?> _onPlaySongRequested;
    private string _searchText = string.Empty;
    private Song? _selectedSong;

    public SongListViewModel(
        ObservableCollection<Song> songs,
        ObservableCollection<Playlist> playlists,
        Action<Song?> onSelectedSongChanged,
        Action onPlaylistContentChanged,
        Action<Song?> onDeleteSongRequested,
        Action<Song?> onPlaySongRequested)
    {
        Songs = songs;
        Playlists = playlists;
        _onSelectedSongChanged = onSelectedSongChanged;
        _onPlaylistContentChanged = onPlaylistContentChanged;
        _onDeleteSongRequested = onDeleteSongRequested;
        _onPlaySongRequested = onPlaySongRequested;
        FilteredSongs = CollectionViewSource.GetDefaultView(Songs);
        FilteredSongs.Filter = FilterSong;

        _addSelectedSongToPlaylistCommand = new RelayCommand(AddSelectedSongToPlaylist, CanAddSelectedSongToPlaylist);
        AddSelectedSongToPlaylistCommand = _addSelectedSongToPlaylistCommand;
        DeleteSongCommand = new RelayCommand(DeleteSelectedSong, CanDeleteSelectedSong);
        _clearSearchCommand = new RelayCommand(ClearSearch, CanClearSearch);
        ClearSearchCommand = _clearSearchCommand;
        _playSongCommand = new RelayCommand(PlaySong, CanPlaySong);
        PlaySongCommand = _playSongCommand;
    }

    public ObservableCollection<Song> Songs { get; }
    public ObservableCollection<Playlist> Playlists { get; }
    public ICollectionView FilteredSongs { get; }
    public ICommand AddSelectedSongToPlaylistCommand { get; }
    public ICommand DeleteSongCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand PlaySongCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilteredSongs.Refresh();
                _clearSearchCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public Song? SelectedSong
    {
        get => _selectedSong;
        set
        {
            if (SetProperty(ref _selectedSong, value))
            {
                _onSelectedSongChanged(value);
                _addSelectedSongToPlaylistCommand.RaiseCanExecuteChanged();
                ((RelayCommand)DeleteSongCommand).RaiseCanExecuteChanged();
                _playSongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool CanPlaySong(object? parameter)
    {
        return parameter is Song || SelectedSong is not null;
    }

    private void PlaySong(object? parameter)
    {
        var song = parameter as Song ?? SelectedSong;
        if (song is null)
        {
            return;
        }

        _onPlaySongRequested(song);
    }

    private bool CanDeleteSelectedSong(object? parameter)
    {
        return parameter is Song || SelectedSong is not null;
    }

    private void DeleteSelectedSong(object? parameter)
    {
        var song = parameter as Song ?? SelectedSong;
        if (song is null)
        {
            return;
        }

        _onDeleteSongRequested(song);
        _addSelectedSongToPlaylistCommand.RaiseCanExecuteChanged();
        ((RelayCommand)DeleteSongCommand).RaiseCanExecuteChanged();
    }

    private bool CanAddSelectedSongToPlaylist(object? parameter)
    {
        if (SelectedSong is null || parameter is not Playlist playlist)
        {
            return false;
        }

        return !playlist.SongIds.Contains(SelectedSong.Id);
    }

    private void AddSelectedSongToPlaylist(object? parameter)
    {
        if (SelectedSong is null || parameter is not Playlist playlist)
        {
            return;
        }

        if (playlist.SongIds.Contains(SelectedSong.Id))
        {
            return;
        }

        playlist.SongIds.Add(SelectedSong.Id);
        _onPlaylistContentChanged();
        _addSelectedSongToPlaylistCommand.RaiseCanExecuteChanged();
    }

    private bool FilterSong(object obj)
    {
        if (obj is not Song song)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var term = SearchText.Trim();
        return song.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
               || song.Artist.Contains(term, StringComparison.OrdinalIgnoreCase)
               || song.Album.Contains(term, StringComparison.OrdinalIgnoreCase)
               || song.Genre.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private bool CanClearSearch()
    {
        return !string.IsNullOrWhiteSpace(SearchText);
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }
}
