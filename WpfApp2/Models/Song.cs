using System;

namespace WpfApp2.Models;

public class Song
{
    public Song(
        int id,
        string title,
        string artist,
        string album,
        string genre,
        int year,
        TimeSpan duration,
        string audioFileExtension,
        string coverPath,
        byte[]? coverData,
        byte[] audioData)
    {
        Id = id;
        Title = title;
        Artist = artist;
        Album = album;
        Genre = genre;
        Year = year;
        Duration = duration;
        AudioFileExtension = string.IsNullOrWhiteSpace(audioFileExtension) ? ".mp3" : audioFileExtension;
        CoverPath = coverPath;
        CoverData = coverData;
        AudioData = audioData;
    }

    public int Id { get; }
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public string Genre { get; }
    public int Year { get; }
    public TimeSpan Duration { get; }
    public string AudioFileExtension { get; }
    public string CoverPath { get; }
    public byte[]? CoverData { get; }
    public byte[] AudioData { get; }
}
