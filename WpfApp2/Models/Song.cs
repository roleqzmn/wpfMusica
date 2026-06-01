using System;

namespace WpfApp2.Models;

public class Song
{
    public Song(int id, string title, string artist, string album, string genre, TimeSpan duration, string coverPath)
    {
        Id = id;
        Title = title;
        Artist = artist;
        Album = album;
        Genre = genre;
        Duration = duration;
        CoverPath = coverPath;
    }

    public int Id { get; }
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public string Genre { get; }
    public TimeSpan Duration { get; }
    public string CoverPath { get; }
}
