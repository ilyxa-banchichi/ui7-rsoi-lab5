using Common.Models.DTO;
using Common.Models.Enums;

namespace Gateway.Services;

public interface ILibraryService
{
    Task<LibraryPaginationResponse?> GetLibrariesInCityAsync(string city, int page, int size, string accessToken);
    Task<LibraryBookPaginationResponse?> GetBooksInLibraryAsync(
        string libraryUid, int page, int size, string accessToken, bool showAll = false);

    Task<List<LibraryResponse>?> GetLibrariesListAsync(IEnumerable<Guid> librariesUid, string accessToken);
    Task<List<BookInfo>?> GetBooksListAsync(IEnumerable<Guid> booksUid, string accessToken);
    Task TakeBookAsync(Guid libraryUid, Guid bookUid, string accessToken);
    Task<UpdateBookConditionResponse?> ReturnBookAsync(Guid libraryUid, Guid bookUid, BookCondition condition, string accessToken);
}