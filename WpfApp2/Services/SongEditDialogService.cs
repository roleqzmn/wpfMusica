using System.Windows;
using WpfApp2.Models;
using WpfApp2.ViewModels;

namespace WpfApp2.Services;

public class SongEditDialogService : ISongEditDialogService
{
    public Song? ShowEditSongDialog(Song song)
    {
        var viewModel = new SongEditViewModel(song);
        var dialog = new SongEditWindow
        {
            Owner = Application.Current?.MainWindow,
            DataContext = viewModel
        };

        var result = dialog.ShowDialog();
        return result == true ? viewModel.EditedSong : null;
    }
}
