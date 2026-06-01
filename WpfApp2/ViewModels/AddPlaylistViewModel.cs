using Microsoft.Win32;
using System;
using System.Windows.Input;
using WpfApp2.Core;
using WpfApp2.Models;

namespace WpfApp2.ViewModels;

public class AddPlaylistViewModel : BaseViewModel
{
    private readonly RelayCommand _saveCommand;
    private string _name = string.Empty;
    private string _coverPath = string.Empty;

    public AddPlaylistViewModel()
    {
        BrowseCoverCommand = new RelayCommand(BrowseCover);
        _saveCommand = new RelayCommand(Save, CanSave);
        SaveCommand = _saveCommand;
        CancelCommand = new RelayCommand(Cancel);
    }

    public event Action<bool?>? RequestClose;

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
        CreatedPlaylist = new Playlist(Name.Trim(), CoverPath);
        RequestClose?.Invoke(true);
    }

    private void Cancel()
    {
        RequestClose?.Invoke(false);
    }
}
