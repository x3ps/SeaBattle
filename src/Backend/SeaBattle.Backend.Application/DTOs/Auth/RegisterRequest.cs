using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Запрос на регистрацию нового пользователя.
/// Содержит имя пользователя, пароль и подтверждение пароля.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Имя пользователя (логин) для регистрации.
    /// Является обязательным полем и должно иметь длину от 3 до 50 символов.
    /// </summary>
    [Required(ErrorMessage = "Имя пользователя является обязательным полем.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя для регистрации. Должен соответствовать заданным требованиям сложности.
    /// </summary>
    [Required(ErrorMessage = "Пароль является обязательным полем.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть не менее 8 символов.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Пароль должен содержать как минимум одну заглавную букву, одну строчную букву, одну цифру и один специальный символ.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение пароля. Должно совпадать с полем Password.
    /// </summary>
    [Required(ErrorMessage = "Подтверждение пароля является обязательным полем.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}