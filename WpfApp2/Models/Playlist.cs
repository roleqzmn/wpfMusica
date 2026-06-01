namespace WpfApp2.Models;

public class Playlist
{
    public Playlist(string name, string coverPath)
    {
        Name = name;
        CoverPath = coverPath;
    }

    public string Name { get; }
    public string CoverPath { get; }
}
