using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Gateway.OauthService;

public class OauthService : IOauthService
{
    private readonly OauthConfiguration _configuration;

    public OauthService(IOptions<OauthConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public string GetAuthorizeUrl()
    {
        var url = new StringBuilder();
        url.Append(_configuration.Domain);
        url.Append("/authorize?");
        url.Append("response_type=code");
        url.Append("&");
        url.Append($"client_id={_configuration.ClientId}");
        url.Append("&");
        url.Append("redirect_uri=http://localhost:8080/api/v1/callback");
        url.Append("&");
        url.Append("scope=openid profile email");

        return url.ToString();
    }

    public async Task<string> GetAccessToken(string code)
    {
        var client = new HttpClient();

        var url = $"{_configuration.Domain}/oauth/token";

        var requestBody = new
        {
            grant_type = "authorization_code",
            client_id = _configuration.ClientId,
            client_secret = _configuration.ClientSecret,
            code = code,
            redirect_uri = "http://localhost:8080/"
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
    
        var response = await client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                HttpRequestError.InvalidResponse,
                message: errorContent, 
                statusCode: response.StatusCode);
        }
    }
}