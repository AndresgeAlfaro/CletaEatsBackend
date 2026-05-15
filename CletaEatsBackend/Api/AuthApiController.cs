using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CletaEatsBackend.Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CletaEatsBackend.Api;

[ApiController]
[Route("api/Auth")]
public class AuthApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly SupabaseConnectionOptions _opt;

    public AuthApiController(IHttpClientFactory httpFactory, IOptions<SupabaseConnectionOptions> opt)
    {
        _httpFactory = httpFactory;
        _opt = opt.Value;
    }

    /// <summary>Proxy de login Supabase Auth (password); no devuelve tokens al cliente.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthEmailPasswordRequest? req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { mensaje = "Email y contraseña requeridos." });

        var baseUrl = _opt.Url.TrimEnd('/');
        var client = _httpFactory.CreateClient();
        using var httpReq = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/auth/v1/token?grant_type=password");
        httpReq.Headers.TryAddWithoutValidation("apikey", _opt.AnonKey);
        httpReq.Content = new StringContent(
            JsonSerializer.Serialize(new { email = req.Email.Trim().ToLowerInvariant(), password = req.Password }),
            Encoding.UTF8,
            "application/json");

        var res = await client.SendAsync(httpReq);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            return BadRequest(new { mensaje = ParseAuthError(body) });

        return Ok(new { ok = true });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthEmailPasswordRequest? req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { mensaje = "Email y contraseña requeridos." });

        var baseUrl = _opt.Url.TrimEnd('/');
        var client = _httpFactory.CreateClient();
        using var httpReq = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/auth/v1/signup");
        httpReq.Headers.TryAddWithoutValidation("apikey", _opt.AnonKey);
        httpReq.Content = new StringContent(
            JsonSerializer.Serialize(new { email = req.Email.Trim().ToLowerInvariant(), password = req.Password }),
            Encoding.UTF8,
            "application/json");

        var res = await client.SendAsync(httpReq);
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            return BadRequest(new { mensaje = ParseAuthError(body) });

        return Ok(new { ok = true });
    }

    private static string ParseAuthError(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Error de autenticación.";
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.TryGetProperty("msg", out var msg))
                return msg.GetString() ?? raw.Trim();
            if (root.TryGetProperty("error_description", out var ed))
                return ed.GetString() ?? raw.Trim();
            if (root.TryGetProperty("message", out var m))
                return m.GetString() ?? raw.Trim();
        }
        catch { /* ignore */ }

        return raw.Trim().Length > 300 ? raw.Trim()[..300] : raw.Trim();
    }
}

public class AuthEmailPasswordRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
