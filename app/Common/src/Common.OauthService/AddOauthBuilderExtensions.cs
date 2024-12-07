using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Common.OauthService;

public static class AddOauthBuilderExtensions
{
    public static IServiceCollection AddOauth(this IServiceCollection builder,
        ConfigurationManager configuration)
    {
        builder.Configure<OauthConfiguration>(configuration.GetSection("OauthConfiguration"));
        builder.AddTransient<IOauthService, OauthService>();

        var logger = builder.BuildServiceProvider().GetRequiredService<ILogger<AuthenticationBuilder>>();
        var config = builder.BuildServiceProvider().GetRequiredService<IOptions<OauthConfiguration>>().Value;
        
        builder.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = config.Domain;
                options.Audience = config.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config.Domain,
                    ValidateAudience = true,
                    ValidAudience = config.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKeyResolver = (token, securityToken, keyIdentifier, parameters) =>
                    {
                        var client = new HttpClient();
                        var jwksUri = $"{options.Authority}/.well-known/jwks.json";
                        var jwks = client.GetStringAsync(jwksUri).Result;
                        var keys = JsonWebKeySet.Create(jwks);
        
                        return keys.GetSigningKeys();
                    }
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        logger.LogError("Authentication failed: {0}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        logger.LogInformation("Token validated successfully for user: {0}", context.Principal.Identity.Name);
                        return Task.CompletedTask;
                    }
                };
            });
        
        return builder;
    }
}