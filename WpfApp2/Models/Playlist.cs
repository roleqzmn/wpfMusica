using System.Collections.ObjectModel;

namespace WpfApp2.Models;

public class Playlist
{
    public Playlist(string name, string coverPath, IEnumerable<int>? songIds = null)
    {
        Name = name;
        CoverPath = coverPath;
        SongIds = new ObservableCollection<int>(songIds ?? Array.Empty<int>());
    }

    public string Name { get; }
    public string CoverPath { get; }
    public ObservableCollection<int> SongIds { get; }
}
