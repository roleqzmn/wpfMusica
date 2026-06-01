namespace WpfApp2.Models;

public class SongImportData
{
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Year { get; set; }
    public TimeSpan Duration { get; set; }
    public byte[]? CoverData { get; set; }
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
}
