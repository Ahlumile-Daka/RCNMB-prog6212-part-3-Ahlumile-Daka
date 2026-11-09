using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Services;

public class RaceDayApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
    {
        _factory = factory;
        _httpContextAccessor = accessor;
    }

    private HttpClient CreateClient() => _factory.CreateClient("RaceDayApi");

    private void AttachToken(HttpRequestMessage request)
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(method, url);
        AttachToken(request);
        if (body != null)
            request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> SendMultipartAsync(HttpMethod method, string url, MultipartFormDataContent content)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(method, url);
        AttachToken(request);
        request.Content = content;
        return await client.SendAsync(request);
    }

    public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) return default;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString();
            return string.IsNullOrWhiteSpace(json) ? response.ReasonPhrase : json;
        }
        catch { return response.ReasonPhrase; }
    }

    // ========== AUTH ==========
    public async Task<(bool Success, LoginResponse? Data, string? Error)> LoginAsync(LoginRequest request)
    {
        var response = await SendAsync(HttpMethod.Post, "api/auth/login", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<LoginResponse>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var response = await SendAsync(HttpMethod.Post, "api/auth/register", request);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadErrorAsync(response));
    }

    // ========== EVENTS ==========
    public async Task<List<EventDto>?> GetEventsAsync(string? status = null)
    {
        var url = "api/events";
        if (!string.IsNullOrEmpty(status)) url += $"?status={Uri.EscapeDataString(status)}";
        var response = await SendAsync(HttpMethod.Get, url);
        return await ReadAsync<List<EventDto>>(response);
    }

    public async Task<EventDto?> GetEventAsync(int id)
    {
        var response = await SendAsync(HttpMethod.Get, $"api/events/{id}");
        return await ReadAsync<EventDto>(response);
    }

    public async Task<(bool Success, EventDto? Data, string? Error)> CreateEventAsync(CreateEventRequest request)
    {
        var response = await SendAsync(HttpMethod.Post, "api/events", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<EventDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, EventDto? Data, string? Error)> UpdateEventAsync(int id, UpdateEventRequest request)
    {
        var response = await SendAsync(HttpMethod.Put, $"api/events/{id}", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<EventDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, string? Error)> DeleteEventAsync(int id)
    {
        var response = await SendAsync(HttpMethod.Delete, $"api/events/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, EventDto? Data, string? Error)> UploadEventBannerAsync(int eventId, IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        var response = await SendMultipartAsync(HttpMethod.Post, $"api/events/{eventId}/banner", content);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<EventDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    // ========== CATEGORIES ==========
    public async Task<List<CategoryDto>?> GetEventCategoriesAsync(int eventId)
    {
        var response = await SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");
        return await ReadAsync<List<CategoryDto>>(response);
    }

    public async Task<(bool Success, CategoryDto? Data, string? Error)> CreateCategoryAsync(int eventId, CreateCategoryRequest request)
    {
        var response = await SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<CategoryDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, CategoryDto? Data, string? Error)> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var response = await SendAsync(HttpMethod.Put, $"api/categories/{id}", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<CategoryDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, string? Error)> DeleteCategoryAsync(int id)
    {
        var response = await SendAsync(HttpMethod.Delete, $"api/categories/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadErrorAsync(response));
    }

    // ========== ENROLMENTS ==========
    public async Task<(bool Success, EnrolmentDto? Data, string? Error)> CreateEnrolmentAsync(CreateEnrolmentRequest request)
    {
        var response = await SendAsync(HttpMethod.Post, "api/enrolments", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<EnrolmentDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<List<EnrolmentDto>?> GetMyEnrolmentsAsync()
    {
        var response = await SendAsync(HttpMethod.Get, "api/enrolments/me");
        return await ReadAsync<List<EnrolmentDto>>(response);
    }

    public async Task<List<EnrolmentDto>?> GetEventEnrolmentsAsync(int eventId)
    {
        var response = await SendAsync(HttpMethod.Get, $"api/events/{eventId}/enrolments");
        return await ReadAsync<List<EnrolmentDto>>(response);
    }

    // ========== RESULTS ==========
    public async Task<(bool Success, ResultDto? Data, string? Error)> CreateResultAsync(CreateResultRequest request)
    {
        var response = await SendAsync(HttpMethod.Post, "api/results", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<ResultDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, ResultDto? Data, string? Error)> UpdateResultAsync(int id, UpdateResultRequest request)
    {
        var response = await SendAsync(HttpMethod.Put, $"api/results/{id}", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<ResultDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<List<ResultDto>?> GetMyResultsAsync()
    {
        var response = await SendAsync(HttpMethod.Get, "api/results/me");
        return await ReadAsync<List<ResultDto>>(response);
    }

    public async Task<List<ResultDto>?> GetEventResultsAsync(int eventId)
    {
        var response = await SendAsync(HttpMethod.Get, $"api/events/{eventId}/results");
        return await ReadAsync<List<ResultDto>>(response);
    }

    // ========== USERS / PROFILE ==========
    public async Task<UserProfileDto?> GetMyProfileAsync()
    {
        var response = await SendAsync(HttpMethod.Get, "api/users/me");
        return await ReadAsync<UserProfileDto>(response);
    }

    public async Task<(bool Success, UserProfileDto? Data, string? Error)> UpdateMyProfileAsync(UpdateProfileRequest request)
    {
        var response = await SendAsync(HttpMethod.Put, "api/users/me", request);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<UserProfileDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, UserProfileDto? Data, string? Error)> UploadProfileImageAsync(IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        var response = await SendMultipartAsync(HttpMethod.Post, "api/users/me/profile-image", content);
        if (response.IsSuccessStatusCode)
            return (true, await ReadAsync<UserProfileDto>(response), null);
        return (false, null, await ReadErrorAsync(response));
    }
}
