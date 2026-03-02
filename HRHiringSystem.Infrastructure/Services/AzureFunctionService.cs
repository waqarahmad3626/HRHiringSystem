using System.Net.Http.Json;
using System.Text.Json;
using HRHiringSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRHiringSystem.Infrastructure.Services;

/// <summary>
/// HTTP client service for calling Azure Functions
/// </summary>
public class AzureFunctionService : IAzureFunctionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureFunctionService> _logger;
    private readonly string _apiKey;

    public AzureFunctionService(HttpClient httpClient, ILogger<AzureFunctionService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["AzureFunction:ApiKey"] ?? "hr-hiring-function-secret-key-2026";
    }

    public async Task TriggerApplicationProcessingAsync(Guid candidateId, Guid jobId, Guid applicationId, string cvUrl)
    {
        try
        {
            var request = new
            {
                candidateId = candidateId.ToString(),
                jobId = jobId.ToString(),
                applicationId = applicationId.ToString(),
                cvUrl
            };

            _logger.LogInformation(
                "Triggering Azure Function for application {ApplicationId} processing",
                applicationId);

            // Fire and forget - don't await to keep it truly background
            _ = Task.Run(async () =>
            {
                try
                {
                    var json = JsonSerializer.Serialize(request);
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/process-application");
                    httpRequest.Headers.Add("X-Function-Key", _apiKey);
                    httpRequest.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    _logger.LogInformation("Sending request to Azure Function with body: {Body}", json);
                    
                    var response = await _httpClient.SendAsync(httpRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            "Azure Function triggered successfully for application {ApplicationId}",
                            applicationId);
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning(
                            "Azure Function returned {StatusCode} for application {ApplicationId}: {Error}",
                            response.StatusCode,
                            applicationId,
                            error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to call Azure Function for application {ApplicationId}",
                        applicationId);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to trigger Azure Function for application {ApplicationId}",
                applicationId);
            // Don't throw - this is a background process and shouldn't fail the main request
        }
    }
}
