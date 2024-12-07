using System.Security.Authentication;
using Microsoft.AspNetCore.Http;

namespace Common.OauthService;

public static class TokenUtils
{
    public static string GetToken(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            string token = authorizationHeader.ToString();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            return token;
        }

        throw new AuthenticationException("Cannot get access token");
    }
}