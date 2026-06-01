using System.IO;
using Microsoft.Win32;
using TagLib;
using WpfApp2.Models;
using IOFile = System.IO.File;

namespace WpfApp2.Services;

public class SongImportService : ISongImportService
{
    public IReadOnlyList<SongImportData> ImportFromDialog()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Audio files (*.mp3;*.wav;*.wma;*.flac;*.aac)|*.mp3;*.wav;*.wma;*.flac;*.aac"
        };

        if (dialog.ShowDialog() != true)
        {
            return Array.Empty<SongImportData>();
        }

        var result = new List<SongImportData>();
        foreach (var filePath in dialog.FileNames)
        {
            result.Add(ReadSongMetadata(filePath));
        }

        return result;
    }

    private static SongImportData ReadSongMetadata(string filePath)
    {
        var fallbackTitle = Path.GetFileNameWithoutExtension(filePath);
        var audioData = IOFile.ReadAllBytes(filePath);

        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var tag = tagFile.Tag;

            var coverData = tag.Pictures is { Length: > 0 }
                ? tag.Pictures[0].Data.Data
                : null;

            return new SongImportData
            {
                Title = string.IsNullOrWhiteSpace(tag.Title) ? fallbackTitle : tag.Title,
                Artist = tag.FirstPerformer ?? string.Empty,
                Album = tag.Album ?? string.Empty,
                Genre = tag.FirstGenre ?? string.Empty,
                Year = (int)tag.Year,
                Duration = tagFile.Properties.Duration,
                AudioFileExtension = Path.GetExtension(filePath),
                CoverData = coverData,
                AudioData = audioData
            };
        }
        catch
        {
            return new SongImportData
            {
                Title = fallbackTitle,
                Artist = string.Empty,
                Album = string.Empty,
                Genre = string.Empty,
                Year = 0,
                Duration = TimeSpan.Zero,
                AudioFileExtension = Path.GetExtension(filePath),
                CoverData = null,
                AudioData = audioData
            };
        }
    }
}
