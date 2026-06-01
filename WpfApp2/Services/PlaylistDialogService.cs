using System;
using System.Windows;
using WpfApp2.Models;
using WpfApp2.ViewModels;

namespace WpfApp2.Services;

public class PlaylistDialogService : IPlaylistDialogService
{
    public Playlist? ShowCreatePlaylistDialog()
    {
        var viewModel = new AddPlaylistViewModel();
        var dialog = new AddWindow
        {
            Owner = Application.Current?.MainWindow,
            DataContext = viewModel
        };

        void OnRequestClose(bool? result)
        {
            dialog.DialogResult = result;
        }

        viewModel.RequestClose += OnRequestClose;

        try
        {
            var result = dialog.ShowDialog();
            return result == true ? viewModel.CreatedPlaylist : null;
        }
        finally
        {
            viewModel.RequestClose -= OnRequestClose;
        }
    }
}
