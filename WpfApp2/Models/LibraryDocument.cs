using System.Collections.Generic;

namespace WpfApp2.Models;

public class LibraryDocument
{
    public List<SongDocument> Songs { get; set; } = new();
    public List<PlaylistDocument> Playlists { get; set; } = new();
}

public class SongDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Year { get; set; }
    public double DurationSeconds { get; set; }
    public string? CoverDataBase64 { get; set; }
    public string AudioDataBase64 { get; set; } = string.Empty;
}

public class PlaylistDocument
{
    public string Name { get; set; } = string.Empty;
    public string? CoverDataBase64 { get; set; }
    public List<int> SongIds { get; set; } = new();
}
