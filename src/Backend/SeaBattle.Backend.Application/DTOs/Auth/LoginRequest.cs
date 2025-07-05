using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Запрос на аутентификацию пользователя. Содержит учетные данные,
/// необходимые для входа в систему.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Имя пользователя (логин) для аутентификации.
    /// Является обязательным полем и должно иметь длину от 3 до 50 символов.
    /// </summary>
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя для аутентификации.
    /// Является обязательным полем и должен иметь длину не менее 6 символов.
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов.")]
    public string Password { get; set; } = string.Empty;
}