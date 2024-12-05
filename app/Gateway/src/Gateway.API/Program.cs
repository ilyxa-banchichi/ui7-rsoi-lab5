using System.Text;
using Common.CircuitBreaker;
using Common.Models.Serialization;
using Gateway.RequestQueueService;
using Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

builder.Services.AddSwaggerGen(options =>
{
    var basePath = AppContext.BaseDirectory;
    var xmlPath = Path.Combine(basePath, "Gateway.API.xml");
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        corsBuilder => corsBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

builder.Services.AddSingleton<ICircuitBreaker<ILibraryService>, CircuitBreaker<ILibraryService>>();
builder.Services.AddSingleton<ICircuitBreaker<IReservationService>, CircuitBreaker<IReservationService>>();
builder.Services.AddSingleton<ICircuitBreaker<IRatingService>, CircuitBreaker<IRatingService>>();

var GetLibraryService = (IServiceProvider provider) =>
{
    return new LibraryService(
        httpClientFactory: provider.GetRequiredService<IHttpClientFactory>(), 
        baseUrl: "http://library-service:8060",
        circuitBreaker: provider.GetRequiredService<ICircuitBreaker<ILibraryService>>(),
        logger: provider.GetRequiredService<ILogger<LibraryService>>(),
        queueService: provider.GetRequiredService<IRequestQueueService>()
    );
};
builder.Services.AddTransient<ILibraryService, LibraryService>(provider => GetLibraryService(provider));
builder.Services.AddTransient<IRequestQueueUser, LibraryService>(provider => GetLibraryService(provider));

var GetReservationService = (IServiceProvider provider) =>
{
    return new ReservationService(
        httpClientFactory: provider.GetRequiredService<IHttpClientFactory>(),
        baseUrl: "http://reservation-service:8070",
        circuitBreaker: provider.GetRequiredService<ICircuitBreaker<IReservationService>>(),
        logger: provider.GetRequiredService<ILogger<ReservationService>>(),
        queueService: provider.GetRequiredService<IRequestQueueService>()
    );
};
builder.Services.AddTransient<IReservationService, ReservationService>(provider => GetReservationService(provider));
builder.Services.AddTransient<IRequestQueueUser, ReservationService>(provider => GetReservationService(provider));

var GetRatingService = (IServiceProvider provider) =>
{
    return new RatingService(httpClientFactory: provider.GetRequiredService<IHttpClientFactory>(),
        baseUrl: "http://rating-service:8050",
        circuitBreaker: provider.GetRequiredService<ICircuitBreaker<IRatingService>>(),
        logger: provider.GetRequiredService<ILogger<RatingService>>(),
        queueService: provider.GetRequiredService<IRequestQueueService>());
};
builder.Services.AddTransient<IRatingService, RatingService>(provider => GetRatingService(provider));
builder.Services.AddTransient<IRequestQueueUser, RatingService>(provider => GetRatingService(provider));

var redisConnection = builder.Configuration.GetConnectionString("RedisQueue");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddTransient<IRequestQueueService, RequestQueueService>();
builder.Services.AddHostedService<RequestQueueJob>();

builder.Services.Configure<RequestQueueConfig>(builder.Configuration.GetSection("RequestQueueConfig"));
builder.Services.Configure<CircuitBreakerConfig>(builder.Configuration.GetSection("CircuitBreakerConfig"));

// Добавьте JWT аутентификацию
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = "https://dev-u1gvfc78k3kezmpp.us.auth0.com/"; // Ваш домен
        options.Audience = "https://rsoi-lab5-api"; // Ваш идентификатор API
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://dev-u1gvfc78k3kezmpp.us.auth0.com/",
            ValidateAudience = true,
            ValidAudience = "https://rsoi-lab5-api",
            ValidateLifetime = true,
            IssuerSigningKeyResolver = (token, securityToken, keyIdentifier, parameters) =>
            {
                var client = new HttpClient();
                var jwksUri = $"{options.Authority}.well-known/jwks.json";
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

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAllOrigins");

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

app.Run();