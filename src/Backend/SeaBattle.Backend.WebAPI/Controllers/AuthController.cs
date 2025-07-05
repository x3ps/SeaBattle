using Microsoft.AspNetCore.Mvc;
using SeaBattle.Backend.Application.DTOs.Auth;
using SeaBattle.Backend.Application.Interfaces;
using System.Net;
using System.Security.Claims;

namespace SeaBattle.Backend.WebAPI.Controllers;

/// <summary>
/// Контроллер авторизации
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger; // Добавим логгер для контроллера

    /// <summary>
    /// Конструктор контроллера. Внедряет IUserService.
    /// </summary>
    /// <param name="userService">Сервис для работы с пользователями и аутентификацией.</param>
    /// <param name="logger">Логгер для контроллера.</param>
    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    /// <param name="request">Данные для регистрации (имя пользователя, пароль).</param>
    /// <returns>AuthResponse с токенами при успехе или сообщение об ошибке.</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Получен запрос на регистрацию пользователя: {Username}", request.Username);
        
        // Получаем IP-адрес клиента
        var ipAddress = GetClientIpAddress();

        var response = await _userService.RegisterUserAsync(request, ipAddress);

        if (response == null)
        {
            _logger.LogWarning("Регистрация пользователя {Username} не удалась.", request.Username);
            return BadRequest(new { message = "Имя пользователя уже занято или произошла ошибка при регистрации." });
        }

        _logger.LogInformation("Пользователь {Username} успешно зарегистрирован.", request.Username);
        return Ok(response);
    }

    /// <summary>
    /// Аутентифицирует пользователя (вход в систему).
    /// </summary>
    /// <param name="request">Данные для входа (имя пользователя, пароль).</param>
    /// <returns>AuthResponse с токенами при успехе или сообщение об ошибке.</returns>
    [HttpPost("authenticate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Получен запрос на аутентификацию пользователя: {Username}", request.Username);

        // Получаем IP-адрес клиента
        var ipAddress = GetClientIpAddress();

        var response = await _userService.AuthenticateUserAsync(request, ipAddress);

        if (response == null)
        {
            _logger.LogWarning("Аутентификация пользователя {Username} не удалась.", request.Username);
            return BadRequest(new { message = "Неверное имя пользователя или пароль." });
        }

        _logger.LogInformation("Пользователь {Username} успешно аутентифицирован.", request.Username);
        return Ok(response);
    }

    /// <summary>
    /// Обновляет токены доступа с использованием Refresh Token.
    /// </summary>
    /// <param name="request">Запрос на обновление токена, содержащий Refresh Token.</param>
    /// <returns>Новая пара токенов при успехе или сообщение об ошибке.</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // IP-адрес для RefreshTokenAsync уже приходит в самом DTO
        // Это важно, так как запрос может быть отправлен с любого устройства
        request.IpAddress = GetClientIpAddress(); // Убеждаемся, что IP из запроса всегда актуален

        _logger.LogInformation("Получен запрос на обновление токена с IP: {IpAddress}", request.IpAddress);

        var response = await _userService.RefreshTokenAsync(request);

        if (response == null)
        {
            _logger.LogWarning("Недействительный или истекший refresh token с IP: {IpAddress}", request.IpAddress);
            return BadRequest(new { message = "Недействительный или истекший refresh token." });
        }

        _logger.LogInformation("Токен успешно обновлен для пользователя ID: {UserId} с IP: {IpAddress}", response.UserId, request.IpAddress);
        return Ok(response);
    }

    /// <summary>
    /// Отзывает Refresh Token (делает его недействительным).
    /// Требует аутентификации.
    /// </summary>
    /// <param name="request">Запрос на отзыв токена, содержащий Refresh Token и причину.</param>
    /// <returns>Сообщение об успехе или ошибке.</returns>
    [HttpPost("revoke-token")]
    // [Authorize] // Возможно, ты захочешь, чтобы этот эндпоинт был защищен (требовал валидного Access Token)
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RevokeTokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        // IP-адрес для RevokeTokenAsync также приходит в DTO
        request.IpAddress = GetClientIpAddress(); // Убеждаемся, что IP из запроса всегда актуален

        _logger.LogInformation("Получен запрос на отзыв токена: {RefreshToken} с IP: {IpAddress}", request.RefreshToken, request.IpAddress);

        var response = await _userService.RevokeRefreshTokenAsync(request);

        if (!response.Success)
        {
            _logger.LogWarning("Ошибка отзыва токена {RefreshToken}: {Message}", request.RefreshToken, response.Message);
            return BadRequest(new { message = response.Message });
        }

        _logger.LogInformation("Токен {RefreshToken} успешно отозван.", request.RefreshToken);
        return Ok(response);
    }

    /// <summary>
    /// Изменяет пароль аутентифицированного пользователя.
    /// Требует аутентификации.
    /// </summary>
    /// <param name="request">Данные для смены пароля (текущий и новый пароли).</param>
    /// <returns>Сообщение об успехе или ошибке.</returns>
    [HttpPut("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize] // Этот эндпоинт ДОЛЖЕН быть защищен
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChangePasswordResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Если токен не предоставлен/невалиден
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        // Получаем ID пользователя из токена аутентификации
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogWarning("Не удалось получить User ID из токена для смены пароля.");
            return Unauthorized(new { message = "Не авторизованный доступ." });
        }
        
        _logger.LogInformation("Получен запрос на смену пароля для пользователя ID: {UserId}", userId);

        var response = await _userService.ChangePasswordAsync(userId, request);

        if (!response.Success)
        {
            _logger.LogWarning("Смена пароля для пользователя ID: {UserId} не удалась: {Message}", userId, response.Message);
            return BadRequest(new { message = response.Message });
        }

        _logger.LogInformation("Пароль для пользователя ID: {UserId} успешно изменен.", userId);
        return Ok(response);
    }

    /// <summary>
    /// Вспомогательный метод для получения IP-адреса клиента из HTTP-контекста.
    /// </summary>
    /// <returns>Строка, содержащая IP-адрес клиента, или "unknown" если не удалось определить.</returns>
    private string GetClientIpAddress()
    {
        // Если HttpContext.Connection.RemoteIpAddress равен null (например, при тестировании без HTTP-контекста),
        // или если адрес IPv6, а тебе нужен IPv4, могут потребоваться дополнительные проверки.
        var ipAddress = HttpContext.Connection.RemoteIpAddress;

        // Важно: если приложение работает за прокси-сервером (например, Nginx, Azure App Service, Cloudflare),
        // реальный IP-адрес клиента будет находиться в заголовке X-Forwarded-For.
        // HttpContext.Connection.RemoteIpAddress в этом случае покажет IP прокси-сервера.
        if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            // X-Forwarded-For может содержать несколько IP, разделенных запятыми. Берем первый.
            var firstIp = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(firstIp) && IPAddress.TryParse(firstIp, out var parsedIp))
            {
                ipAddress = parsedIp;
            }
        }
        
        // Возвращаем IP-адрес в виде строки или "unknown", если не удалось определить
        return ipAddress?.ToString() ?? "unknown";
    }
}