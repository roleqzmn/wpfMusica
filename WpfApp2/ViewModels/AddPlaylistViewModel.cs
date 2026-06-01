using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp2.Core;
using WpfApp2.Models;

namespace WpfApp2.ViewModels;

public class PlaylistEditViewModel : BaseViewModel
{
    private readonly RelayCommand _saveCommand;
    private readonly int[] _songIds;
    private string _name = string.Empty;
    private string _coverPath = string.Empty;

    public PlaylistEditViewModel(Playlist? playlist = null)
    {
        _songIds = playlist?.SongIds.ToArray() ?? Array.Empty<int>();
        _name = playlist?.Name ?? string.Empty;
        _coverPath = playlist?.CoverPath ?? string.Empty;

        BrowseCoverCommand = new RelayCommand(BrowseCover);
        _saveCommand = new RelayCommand(Save, CanSave);
        SaveCommand = _saveCommand;
        CancelCommand = new RelayCommand(Cancel);
    }

    public event Action<bool?>? CloseRequested;

    public ICommand BrowseCoverCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public Playlist? CreatedPlaylist { get; private set; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                _saveCommand.RaiseCanExecuteChanged();
            }
        }
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

        if (openFileDialog.ShowDialog() == true)
        {
            CoverPath = openFileDialog.FileName;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    private void Save()
    {
        CreatedPlaylist = new Playlist(Name.Trim(), CoverPath, _songIds);
        CloseRequested?.Invoke(true);
    }

    private void Cancel()
    {
        CloseRequested?.Invoke(false);
    }
}
