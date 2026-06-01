using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using WpfApp2.Core;
using WpfApp2.Models;

namespace WpfApp2.ViewModels;

public class SongListViewModel : BaseViewModel
{
    private readonly Action<Song?> _onSelectedSongChanged;
    private string _searchText = string.Empty;
    private Song? _selectedSong;

    public SongListViewModel(ObservableCollection<Song> songs, Action<Song?> onSelectedSongChanged)
    {
        Songs = songs;
        _onSelectedSongChanged = onSelectedSongChanged;
        FilteredSongs = CollectionViewSource.GetDefaultView(Songs);
        FilteredSongs.Filter = FilterSong;
    }

    public ObservableCollection<Song> Songs { get; }
    public ICollectionView FilteredSongs { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilteredSongs.Refresh();
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
            }
        }
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
}
