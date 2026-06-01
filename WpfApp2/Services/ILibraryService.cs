using WpfApp2.Models;

namespace WpfApp2.Services;

public interface ILibraryService
{
    LibraryDocument? CurrentLibrary { get; }
    string? CurrentLibraryPath { get; }

    void CreateNewLibrary(string filePath);
    void OpenLibrary(string filePath);
    void SetLibrary(LibraryDocument library);
    void SaveLibrary();
    void MarkDirty();
}
