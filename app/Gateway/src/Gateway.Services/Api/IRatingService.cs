using Common.Models.DTO;

namespace Gateway.Services;

public interface IRatingService
{
    Task<UserRatingResponse?> GetUserRating(string accessToken);
    Task<UserRatingResponse?> IncreaseRating(string accessToken);
    Task<UserRatingResponse?> DecreaseRating(string accessToken);
}