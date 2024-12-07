using Common.Models.DTO;

namespace Gateway.Services;

public interface IReservationService
{
    Task<List<RawBookReservationResponse>?> GetUserReservationsAsync(string accessToken);
    Task<RawBookReservationResponse?> TakeBook(string accessToken, TakeBookRequest body);
    Task TakeBookRollback(Guid reservationGuid, string accessToken);
    Task<RawBookReservationResponse?>  ReturnBook(Guid reservationUid, DateOnly date, string accessToken);
}