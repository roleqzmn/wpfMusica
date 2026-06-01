using System;
using System.IO;
using Microsoft.Win32;
using WpfApp2.Core;
using WpfApp2.Models;

namespace WpfApp2.ViewModels;

public class SongEditViewModel : BaseViewModel
{
    private readonly RelayCommand _saveCommand;
    private readonly int _id;
    private readonly TimeSpan _duration;
    private readonly byte[] _audioData;
    private readonly string _audioFileExtension;

    private string _title;
    private string _artist;
    private string _album;
    private string _genre;
    private int _year;
    private string _coverPath;
    private byte[]? _coverData;

    public SongEditViewModel(Song song)
    {
        _id = song.Id;
        _duration = song.Duration;
        _audioData = song.AudioData;
        _audioFileExtension = song.AudioFileExtension;

        _title = song.Title;
        _artist = song.Artist;
        _album = song.Album;
        _genre = song.Genre;
        _year = song.Year;
        _coverPath = song.CoverPath;
        _coverData = song.CoverData;

        BrowseCoverCommand = new RelayCommand(BrowseCover);
        _saveCommand = new RelayCommand(Save, CanSave);
        SaveCommand = _saveCommand;
        CancelCommand = new RelayCommand(Cancel);
    }

    public event Action<bool?>? CloseRequested;

    public Song? EditedSong { get; private set; }

    public RelayCommand BrowseCoverCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                _saveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Artist
    {
        get => _artist;
        set => SetProperty(ref _artist, value);
    }

    public string Album
    {
        get => _album;
        set => SetProperty(ref _album, value);
    }

    public string Genre
    {
        get => _genre;
        set => SetProperty(ref _genre, value);
    }

    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    public string CoverPath
    {
        get => _coverPath;
        set => SetProperty(ref _coverPath, value);
    }

    private void BrowseCover()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "image files (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg"
        };

        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }

        CoverPath = openFileDialog.FileName;
        _coverData = File.ReadAllBytes(openFileDialog.FileName);
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Title);
    }

    private void Save()
    {
        EditedSong = new Song(
            _id,
            Title.Trim(),
            Artist.Trim(),
            Album.Trim(),
            Genre.Trim(),
            Year,
            _duration,
            _audioFileExtension,
            CoverPath,
            _coverData,
            _audioData);

        CloseRequested?.Invoke(true);
    }

    private void Cancel()
    {
        CloseRequested?.Invoke(false);
    }
}
