using System;
using System.Windows;
using WpfApp2.Models;
using WpfApp2.ViewModels;

namespace WpfApp2.Services;

public class PlaylistDialogService : IPlaylistDialogService
{
    public Playlist? ShowCreatePlaylistDialog()
    {
        return ShowPlaylistDialog(null);
    }

    public Playlist? ShowEditPlaylistDialog(Playlist playlist)
    {
        return ShowPlaylistDialog(playlist);
    }

    private static Playlist? ShowPlaylistDialog(Playlist? playlist)
    {
        var viewModel = new PlaylistEditViewModel(playlist);
        var dialog = new AddWindow
        {
            Owner = Application.Current?.MainWindow,
            DataContext = viewModel
        };

        var result = dialog.ShowDialog();
        return result == true ? viewModel.CreatedPlaylist : null;
    }
}
