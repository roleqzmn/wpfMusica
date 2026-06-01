using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using WpfApp2.Models;

namespace WpfApp2.Services;

public class JsonLibraryService : ILibraryService, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly Timer _autosaveTimer;
    private readonly object _sync = new();

    private bool _disposed;

    public JsonLibraryService()
    {
        _autosaveTimer = new Timer(_ => SaveLibrarySafe(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public LibraryDocument? CurrentLibrary { get; private set; }
    public string? CurrentLibraryPath { get; private set; }

    public void CreateNewLibrary(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        CurrentLibraryPath = filePath;
        CurrentLibrary = new LibraryDocument();
        SaveLibrary();
    }

    public void OpenLibrary(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Library file not found.", filePath);
        }

        var json = File.ReadAllText(filePath);
        var library = JsonSerializer.Deserialize<LibraryDocument>(json, SerializerOptions) ?? new LibraryDocument();

        CurrentLibraryPath = filePath;
        CurrentLibrary = library;
    }

    public void SetLibrary(LibraryDocument library)
    {
        CurrentLibrary = library ?? throw new ArgumentNullException(nameof(library));
    }

    public void SaveLibrary()
    {
        EnsureReadyToSave();

        lock (_sync)
        {
            var json = JsonSerializer.Serialize(CurrentLibrary, SerializerOptions);
            File.WriteAllText(CurrentLibraryPath!, json);
        }
    }

    public void MarkDirty()
    {
        EnsureReadyToSave();
        _autosaveTimer.Change(400, Timeout.Infinite);
    }

    private void SaveLibrarySafe()
    {
        try
        {
            SaveLibrary();
        }
        catch
        {
        }
    }

    private void EnsureReadyToSave()
    {
        if (CurrentLibrary is null)
        {
            throw new InvalidOperationException("No library is loaded.");
        }

        if (string.IsNullOrWhiteSpace(CurrentLibraryPath))
        {
            throw new InvalidOperationException("Library path is not set.");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _autosaveTimer.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
