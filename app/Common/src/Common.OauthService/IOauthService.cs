namespace Common.OauthService;

public interface IOauthService
{
    string GetAuthorizeUrl();
    Task<string> GetAccessToken(string code);
}