using WpfApp2.Models;

namespace WpfApp2.Services;

public interface ISongImportService
{
    IReadOnlyList<SongImportData> ImportFromDialog();
}
