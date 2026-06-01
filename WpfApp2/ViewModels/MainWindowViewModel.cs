using System;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp2.Core;
using WpfApp2.Models;
using WpfApp2.Services;

namespace WpfApp2.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly IPlaylistDialogService _playlistDialogService;
    private readonly ILibraryService _libraryService;
    private readonly RelayCommand _openAddPlaylistCommand;
    private readonly RelayCommand _showSongListCommand;
    private readonly RelayCommand _saveLibraryCommand;
    private bool _isHydratingLibrary;
    private bool _isLibraryLoaded;
    private object? _currentPage;
    private Playlist? _selectedPlaylist;
    private Song? _selectedSong;
    private string _currentTimeText = "0:00";
    private readonly SongListViewModel _songListPage;

    public MainWindowViewModel(IPlaylistDialogService playlistDialogService, ILibraryService libraryService, string? initialLibraryPath = null)
    {
        _playlistDialogService = playlistDialogService;
        _libraryService = libraryService;

        Songs = new ObservableCollection<Song>();
        Playlists = new ObservableCollection<Playlist>();
        _songListPage = new SongListViewModel(Songs, OnSelectedSongChangedFromPage);

        CreateLibraryCommand = new RelayCommand(CreateLibrary);
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        _saveLibraryCommand = new RelayCommand(SaveLibrary, CanSaveLibrary);
        SaveLibraryCommand = _saveLibraryCommand;
        _openAddPlaylistCommand = new RelayCommand(OpenAddPlaylist, () => IsLibraryLoaded);
        OpenAddPlaylistCommand = _openAddPlaylistCommand;
        _showSongListCommand = new RelayCommand(ShowSongList, () => IsLibraryLoaded);
        ShowSongListCommand = _showSongListCommand;
        WelcomePage = new WelcomeViewModel(CreateLibraryCommand, OpenLibraryCommand);
        CurrentPage = WelcomePage;

        Songs.CollectionChanged += OnLibraryCollectionChanged;
        Playlists.CollectionChanged += OnLibraryCollectionChanged;

        SelectedSong = null;
        IsLibraryLoaded = false;

        if (!string.IsNullOrWhiteSpace(initialLibraryPath) && File.Exists(initialLibraryPath))
        {
            TryLoadLibrary(initialLibraryPath);
        }
    }

    public ObservableCollection<Playlist> Playlists { get; }
    public ObservableCollection<Song> Songs { get; }
    public ICommand CreateLibraryCommand { get; }
    public ICommand OpenLibraryCommand { get; }
    public ICommand SaveLibraryCommand { get; }
    public ICommand OpenAddPlaylistCommand { get; }
    public ICommand ShowSongListCommand { get; }
    public WelcomeViewModel WelcomePage { get; }

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            if (SetProperty(ref _selectedPlaylist, value))
            {
                if (!IsLibraryLoaded)
                {
                    return;
                }

                CurrentPage = value is null
                    ? _songListPage
                    : new PlaylistViewModel(value, Songs, OnSelectedSongChangedFromPage);
            }
        }
    }

    public bool IsLibraryLoaded
    {
        get => _isLibraryLoaded;
        private set
        {
            if (SetProperty(ref _isLibraryLoaded, value))
            {
                _openAddPlaylistCommand.RaiseCanExecuteChanged();
                _showSongListCommand.RaiseCanExecuteChanged();
                _saveLibraryCommand.RaiseCanExecuteChanged();
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
                OnPropertyChanged(nameof(CurrentSongTitle));
                OnPropertyChanged(nameof(CurrentSongArtist));
                OnPropertyChanged(nameof(CurrentSongCoverPath));
                OnPropertyChanged(nameof(TotalTimeText));
            }
        }
    }

    public string CurrentSongTitle => SelectedSong?.Title ?? "-";
    public string CurrentSongArtist => SelectedSong?.Artist ?? "-";
    public string CurrentSongCoverPath => SelectedSong?.CoverPath ?? string.Empty;
    public string TotalTimeText => SelectedSong is null ? "0:00" : $"{SelectedSong.Duration:m\\:ss}";

    public string CurrentTimeText
    {
        get => _currentTimeText;
        set => SetProperty(ref _currentTimeText, value);
    }

    private void OpenAddPlaylist()
    {
        if (!IsLibraryLoaded)
        {
            return;
        }

        var playlist = _playlistDialogService.ShowCreatePlaylistDialog();
        if (playlist is not null)
        {
            Playlists.Add(playlist);
        }
    }

    private void CreateLibrary()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Musica library (*.musica.json)|*.musica.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "musica.json",
            AddExtension = true
        };

        if (saveFileDialog.ShowDialog() != true)
        {
            return;
        }

        _libraryService.CreateNewLibrary(saveFileDialog.FileName);
        _isHydratingLibrary = true;
        Songs.Clear();
        Playlists.Clear();
        SelectedPlaylist = null;
        SelectedSong = null;
        _isHydratingLibrary = false;
        IsLibraryLoaded = true;
        CurrentPage = _songListPage;
        SaveCurrentStateToLibrary();
        _saveLibraryCommand.RaiseCanExecuteChanged();
    }

    private void OpenLibrary()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Musica library (*.musica.json)|*.musica.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }

        TryLoadLibrary(openFileDialog.FileName);
    }

    private void TryLoadLibrary(string filePath)
    {
        _libraryService.OpenLibrary(filePath);
        LoadStateFromLibrary();
        IsLibraryLoaded = true;
        CurrentPage = _songListPage;
        _saveLibraryCommand.RaiseCanExecuteChanged();
    }

    private void LoadStateFromLibrary()
    {
        var library = _libraryService.CurrentLibrary;
        if (library is null)
        {
            return;
        }

        _isHydratingLibrary = true;
        Songs.Clear();
        foreach (var song in library.Songs)
        {
            Songs.Add(new Song(
                song.Id,
                song.Title,
                song.Artist,
                song.Album,
                song.Genre,
                TimeSpan.FromSeconds(song.DurationSeconds),
                "/Resources/vinyl.ico"));
        }

        Playlists.Clear();
        foreach (var playlist in library.Playlists)
        {
            Playlists.Add(new Playlist(playlist.Name, "/Resources/vinyl.ico", playlist.SongIds));
        }

        SelectedPlaylist = null;
        SelectedSong = Songs.Count > 0 ? Songs[0] : null;
        _isHydratingLibrary = false;
    }

    private void ShowSongList()
    {
        if (!IsLibraryLoaded)
        {
            return;
        }

        SelectedPlaylist = null;
        CurrentPage = _songListPage;
    }

    private void OnSelectedSongChangedFromPage(Song? song)
    {
        SelectedSong = song;
    }

    private bool CanSaveLibrary()
    {
        return IsLibraryLoaded && !string.IsNullOrWhiteSpace(_libraryService.CurrentLibraryPath);
    }

    private void SaveLibrary()
    {
        if (!CanSaveLibrary())
        {
            return;
        }

        SaveCurrentStateToLibrary();
    }

    private void OnLibraryCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isHydratingLibrary || !IsLibraryLoaded || !CanSaveLibrary())
        {
            return;
        }

        SaveCurrentStateToLibrary();
        _libraryService.MarkDirty();
    }

    private void SaveCurrentStateToLibrary()
    {
        var document = new LibraryDocument
        {
            Songs = Songs
                .Select(song => new SongDocument
                {
                    Id = song.Id,
                    Title = song.Title,
                    Artist = song.Artist,
                    Album = song.Album,
                    Genre = song.Genre,
                    Year = 0,
                    DurationSeconds = song.Duration.TotalSeconds,
                    CoverDataBase64 = TryReadFileToBase64(song.CoverPath),
                    AudioDataBase64 = string.Empty
                })
                .ToList(),
            Playlists = Playlists
                .Select(playlist => new PlaylistDocument
                {
                    Name = playlist.Name,
                    CoverDataBase64 = TryReadFileToBase64(playlist.CoverPath),
                    SongIds = playlist.SongIds.ToList()
                })
                .ToList()
        };

        _libraryService.SetLibrary(document);
        _libraryService.SaveLibrary();
    }

    private static string? TryReadFileToBase64(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        return Convert.ToBase64String(File.ReadAllBytes(path));
    }
}
