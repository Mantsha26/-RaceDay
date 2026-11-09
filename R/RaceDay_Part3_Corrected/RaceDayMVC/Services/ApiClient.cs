using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Services;

/// <summary>The API rejected the session token (expired or invalid).</summary>
public class ApiUnauthorizedException : Exception
{
    public ApiUnauthorizedException() : base("Session expired.") { }
}

/// <summary>The API answered with an error. The message is safe to show to users.</summary>
public class ApiException(string message) : Exception(message);

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    private bool ApplyAuthentication()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        var token = _accessor.HttpContext?.Session.GetString("ApiToken");
        if (string.IsNullOrWhiteSpace(token)) return false;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return true;
    }

    private static void ThrowIfSessionRejected(HttpResponseMessage response, bool hadToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized && hadToken)
            throw new ApiUnauthorizedException();
    }

    public async Task<T?> Get<T>(string relativeUrl)
    {
        var hadToken = ApplyAuthentication();
        using var response = await _http.GetAsync(relativeUrl);
        ThrowIfSessionRejected(response, hadToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return default;
        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new ApiException("You do not have access to that page.");
        if (!response.IsSuccessStatusCode)
            throw new ApiException("RaceDay could not load that information. Please try again.");

        return await response.Content.ReadFromJsonAsync<T>(Json);
    }

    public async Task<(bool ok, JsonElement data)> Send(string method, string relativeUrl, object? body = null)
    {
        var hadToken = ApplyAuthentication();
        using var request = new HttpRequestMessage(new HttpMethod(method), relativeUrl);
        if (body is not null) request.Content = JsonContent.Create(body, options: Json);

        using var response = await _http.SendAsync(request);
        ThrowIfSessionRejected(response, hadToken);

        var responseText = await response.Content.ReadAsStringAsync();
        JsonElement data;
        if (string.IsNullOrWhiteSpace(responseText))
            data = JsonDocument.Parse("{}").RootElement.Clone();
        else
        {
            try { data = JsonDocument.Parse(responseText).RootElement.Clone(); }
            catch (JsonException) { data = JsonSerializer.SerializeToElement(new { message = "The request could not be completed." }); }
        }

        return (response.IsSuccessStatusCode, data);
    }

    /// <summary>Returns the API's "message" field when present, otherwise the fallback.</summary>
    public static string Message(JsonElement data, string fallback)
    {
        if (data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("message", out var m) &&
            m.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(m.GetString()))
            return m.GetString()!;
        return fallback;
    }

    private const long MaxUploadBytes = 5_000_000;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<(bool ok, string? message)> Upload(string relativeUrl, IFormFile file)
    {
        // Check here first: the API closes the connection mid-upload if the file is too big,
        // which would otherwise surface as a confusing network error.
        if (file.Length > MaxUploadBytes)
            return (false, $"That image is {file.Length / 1_000_000.0:0.0} MB. Choose a JPG, PNG or WebP under 5 MB.");
        if (!AllowedImageExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            return (false, "Use a JPG, PNG or WebP image.");

        var hadToken = ApplyAuthentication();
        try
        {
            using var form = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            form.Add(content, "file", Path.GetFileName(file.FileName));

            using var response = await _http.PostAsync(relativeUrl, form);
            ThrowIfSessionRejected(response, hadToken);
            if (response.IsSuccessStatusCode) return (true, null);

            string? message = null;
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("message", out var m))
                    message = m.GetString();
            }
            catch (JsonException) { }
            return (false, message ?? "The image could not be uploaded. Please try again.");
        }
        catch (HttpRequestException ex) when (ex.InnerException is IOException)
        {
            return (false, "The upload was interrupted before it finished. Try a smaller image, and check that the API is still running.");
        }
    }
}
