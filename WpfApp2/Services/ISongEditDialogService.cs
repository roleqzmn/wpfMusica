using WpfApp2.Models;

namespace WpfApp2.Services;

public interface ISongEditDialogService
{
    Song? ShowEditSongDialog(Song song);
}
