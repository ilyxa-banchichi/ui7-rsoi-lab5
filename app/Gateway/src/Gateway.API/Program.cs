using Common.CircuitBreaker;
using Common.Models.Serialization;
using Common.OauthService;
using Gateway.RequestQueueService;
using Gateway.Services;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
{
    var basePath = AppContext.BaseDirectory;
    var xmlPath = Path.Combine(basePath, "Gateway.API.xml");
    options.IncludeXmlComments(xmlPath);
    // Добавляем схему безопасности
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите токен"
    });

    // Добавляем требование безопасности для всех операций
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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

builder.Services.AddOauth(builder.Configuration);
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