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
    private readonly ISongImportService _songImportService;
    private readonly ISongEditDialogService _songEditDialogService;
    private readonly RelayCommand _openAddPlaylistCommand;
    private readonly RelayCommand _renamePlaylistCommand;
    private readonly RelayCommand _deletePlaylistCommand;
    private readonly RelayCommand _showSongListCommand;
    private readonly RelayCommand _saveLibraryCommand;
    private readonly RelayCommand _editSongCommand;
    private readonly RelayCommand _deleteSongCommand;
    private bool _isHydratingLibrary;
    private bool _isLibraryLoaded;
    private object? _currentPage;
    private Playlist? _selectedPlaylist;
    private Song? _selectedSong;
    private string _currentTimeText = "0:00";
    private readonly SongListViewModel _songListPage;

    public MainWindowViewModel(
        IPlaylistDialogService playlistDialogService,
        ILibraryService libraryService,
        ISongImportService songImportService,
        ISongEditDialogService songEditDialogService,
        string? initialLibraryPath = null)
    {
        _playlistDialogService = playlistDialogService;
        _libraryService = libraryService;
        _songImportService = songImportService;
        _songEditDialogService = songEditDialogService;

        Songs = new ObservableCollection<Song>();
        Playlists = new ObservableCollection<Playlist>();
        _songListPage = new SongListViewModel(
            Songs,
            Playlists,
            OnSelectedSongChangedFromPage,
            OnPlaylistContentChanged,
            DeleteSong);

        CreateLibraryCommand = new RelayCommand(CreateLibrary);
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
        _saveLibraryCommand = new RelayCommand(SaveLibrary, CanSaveLibrary);
        SaveLibraryCommand = _saveLibraryCommand;
        ImportSongsCommand = new RelayCommand(ImportSongs, () => IsLibraryLoaded);
        _editSongCommand = new RelayCommand(EditSong, CanEditSong);
        EditSongCommand = _editSongCommand;
        _deleteSongCommand = new RelayCommand(DeleteSong, CanDeleteSong);
        DeleteSongCommand = _deleteSongCommand;
        _openAddPlaylistCommand = new RelayCommand(OpenAddPlaylist, () => IsLibraryLoaded);
        OpenAddPlaylistCommand = _openAddPlaylistCommand;
        _renamePlaylistCommand = new RelayCommand(RenamePlaylist, CanManagePlaylist);
        RenamePlaylistCommand = _renamePlaylistCommand;
        _deletePlaylistCommand = new RelayCommand(DeletePlaylist, CanManagePlaylist);
        DeletePlaylistCommand = _deletePlaylistCommand;
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
    public ICommand ImportSongsCommand { get; }
    public ICommand EditSongCommand { get; }
    public ICommand DeleteSongCommand { get; }
    public ICommand OpenAddPlaylistCommand { get; }
    public ICommand RenamePlaylistCommand { get; }
    public ICommand DeletePlaylistCommand { get; }
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
                    : new PlaylistViewModel(value, Songs, OnSelectedSongChangedFromPage, OnPlaylistContentChanged);
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
                _renamePlaylistCommand.RaiseCanExecuteChanged();
                _deletePlaylistCommand.RaiseCanExecuteChanged();
                _showSongListCommand.RaiseCanExecuteChanged();
                ((RelayCommand)ImportSongsCommand).RaiseCanExecuteChanged();
                _editSongCommand.RaiseCanExecuteChanged();
                _deleteSongCommand.RaiseCanExecuteChanged();
                _saveLibraryCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private void ImportSongs()
    {
        if (!IsLibraryLoaded)
        {
            return;
        }

        var imported = _songImportService.ImportFromDialog();
        if (imported.Count == 0)
        {
            return;
        }

        var nextId = Songs.Count == 0 ? 1 : Songs.Max(s => s.Id) + 1;
        foreach (var item in imported)
        {
            Songs.Add(new Song(
                nextId++,
                item.Title,
                item.Artist,
                item.Album,
                item.Genre,
                item.Year,
                item.Duration,
                CreateCoverPath(item.CoverData),
                item.CoverData,
                item.AudioData));
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
                _editSongCommand.RaiseCanExecuteChanged();
                _deleteSongCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool CanEditSong(object? parameter)
    {
        return IsLibraryLoaded && ResolveSongForEdit(parameter) is not null;
    }

    private void EditSong(object? parameter)
    {
        if (!IsLibraryLoaded)
        {
            return;
        }

        var song = ResolveSongForEdit(parameter);
        if (song is null)
        {
            return;
        }

        var edited = _songEditDialogService.ShowEditSongDialog(song);
        if (edited is null)
        {
            return;
        }

        var index = Songs.IndexOf(song);
        if (index < 0)
        {
            index = Songs.ToList().FindIndex(s => s.Id == edited.Id);
        }

        if (index < 0)
        {
            return;
        }

        Songs[index] = edited;
        SelectedSong = edited;

        if (SelectedPlaylist is not null && CurrentPage is PlaylistViewModel)
        {
            CurrentPage = new PlaylistViewModel(SelectedPlaylist, Songs, OnSelectedSongChangedFromPage, OnPlaylistContentChanged);
        }

        SaveCurrentStateToLibrary();
        _libraryService.MarkDirty();
    }

    private Song? ResolveSongForEdit(object? parameter)
    {
        if (parameter is Song song)
        {
            return song;
        }

        return SelectedSong;
    }

    private bool CanDeleteSong(object? parameter)
    {
        return IsLibraryLoaded && ResolveSongForEdit(parameter) is not null;
    }

    private void DeleteSong(object? parameter)
    {
        if (!IsLibraryLoaded)
        {
            return;
        }

        var song = ResolveSongForEdit(parameter);
        if (song is null)
        {
            return;
        }

        if (!Songs.Remove(song))
        {
            return;
        }

        foreach (var playlist in Playlists)
        {
            while (playlist.SongIds.Contains(song.Id))
            {
                playlist.SongIds.Remove(song.Id);
            }
        }

        if (SelectedSong == song)
        {
            SelectedSong = Songs.FirstOrDefault();
        }

        if (SelectedPlaylist is not null && CurrentPage is PlaylistViewModel)
        {
            CurrentPage = new PlaylistViewModel(SelectedPlaylist, Songs, OnSelectedSongChangedFromPage, OnPlaylistContentChanged);
        }

        SaveCurrentStateToLibrary();
        _libraryService.MarkDirty();
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

    private bool CanManagePlaylist(object? parameter)
    {
        return IsLibraryLoaded && parameter is Playlist;
    }

    private void RenamePlaylist(object? parameter)
    {
        if (parameter is not Playlist playlist)
        {
            return;
        }

        var edited = _playlistDialogService.ShowEditPlaylistDialog(playlist);
        if (edited is null)
        {
            return;
        }

        var index = Playlists.IndexOf(playlist);
        if (index < 0)
        {
            return;
        }

        var updated = new Playlist(edited.Name, edited.CoverPath, playlist.SongIds);
        Playlists[index] = updated;

        if (SelectedPlaylist == playlist)
        {
            SelectedPlaylist = updated;
        }
    }

    private void DeletePlaylist(object? parameter)
    {
        if (parameter is not Playlist playlist)
        {
            return;
        }

        var wasSelected = SelectedPlaylist == playlist;
        if (!Playlists.Remove(playlist))
        {
            return;
        }

        if (wasSelected)
        {
            SelectedPlaylist = null;
            CurrentPage = _songListPage;
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
            var coverData = DecodeBase64(song.CoverDataBase64);
            Songs.Add(new Song(
                song.Id,
                song.Title,
                song.Artist,
                song.Album,
                song.Genre,
                song.Year,
                TimeSpan.FromSeconds(song.DurationSeconds),
                CreateCoverPath(coverData),
                coverData,
                DecodeBase64(song.AudioDataBase64) ?? Array.Empty<byte>()));
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
        if (ReferenceEquals(sender, Playlists))
        {
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems.OfType<Playlist>())
                {
                    item.SongIds.CollectionChanged -= OnPlaylistSongIdsCollectionChanged;
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems.OfType<Playlist>())
                {
                    item.SongIds.CollectionChanged += OnPlaylistSongIdsCollectionChanged;
                }
            }
        }

        if (_isHydratingLibrary || !IsLibraryLoaded || !CanSaveLibrary())
        {
            return;
        }

        SaveCurrentStateToLibrary();
        _libraryService.MarkDirty();
    }

    private void OnPlaylistSongIdsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPlaylistContentChanged();
    }

    private void OnPlaylistContentChanged()
    {
        if (_isHydratingLibrary || !IsLibraryLoaded || !CanSaveLibrary())
        {
            return;
        }

        SaveCurrentStateToLibrary();
        _libraryService.MarkDirty();

        if (SelectedPlaylist is not null && CurrentPage is PlaylistViewModel)
        {
            CurrentPage = new PlaylistViewModel(SelectedPlaylist, Songs, OnSelectedSongChangedFromPage, OnPlaylistContentChanged);
        }
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
                    Year = song.Year,
                    DurationSeconds = song.Duration.TotalSeconds,
                    CoverDataBase64 = ToBase64(song.CoverData),
                    AudioDataBase64 = ToBase64(song.AudioData) ?? string.Empty
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

    private static string? ToBase64(byte[]? data)
    {
        if (data is null || data.Length == 0)
        {
            return null;
        }

        return Convert.ToBase64String(data);
    }

    private static byte[]? DecodeBase64(string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(data);
        }
        catch
        {
            return null;
        }
    }

    private static string CreateCoverPath(byte[]? coverData)
    {
        if (coverData is null || coverData.Length == 0)
        {
            return "/Resources/vinyl.ico";
        }

        var coversDirectory = Path.Combine(Path.GetTempPath(), "Musica", "covers");
        Directory.CreateDirectory(coversDirectory);
        var tempPath = Path.Combine(coversDirectory, $"{Guid.NewGuid():N}.img");
        File.WriteAllBytes(tempPath, coverData);
        return tempPath;
    }
}
