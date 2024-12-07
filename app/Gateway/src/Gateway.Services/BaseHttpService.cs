using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Common.CircuitBreaker;
using Microsoft.Extensions.Logging;

namespace Gateway.Services;

public abstract class BaseHttpService
{
    protected readonly HttpClient httpClient;
    protected readonly ICircuitBreaker circuitBreaker;
    protected readonly ILogger logger;

    protected BaseHttpService(
        IHttpClientFactory httpClientFactory,
        string baseUrl,
        ICircuitBreaker circuitBreaker,
        ILogger logger)
    {
        httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        this.circuitBreaker = circuitBreaker;
        this.logger = logger;
    }

    protected void AddAuthorizationHeader(HttpRequestHeaders headers, string accessToken)
    {
        //headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
    
    protected async Task<T?> SendAsync<T>(HttpRequestMessage requestMessage)
    {
        httpClient.DefaultRequestHeaders.Clear();

        var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                HttpRequestError.InvalidResponse,
                message: $"StatusCode: {response.StatusCode}", 
                statusCode: response.StatusCode);
        }

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var result = await response.Content.ReadFromJsonAsync<T>();
            if (result == null)
                throw new JsonException("Invalid response");
        
            return result;
        }

        return default;
    }
    
    protected async Task SendAsync(HttpRequestMessage requestMessage)
    {
        httpClient.DefaultRequestHeaders.Clear();

        var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                HttpRequestError.InvalidResponse,
                message: $"StatusCode: {response.StatusCode}", 
                statusCode: response.StatusCode);
        }
    }
}