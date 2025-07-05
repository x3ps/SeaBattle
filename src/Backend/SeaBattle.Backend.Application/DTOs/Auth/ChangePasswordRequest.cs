using System.ComponentModel.DataAnnotations;

namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Запрос на изменение пароля пользователя.
/// Требует текущий пароль и новый пароль с подтверждением.
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Текущий пароль пользователя. Обязателен для проверки подлинности.
    /// </summary>
    [Required(ErrorMessage = "Текущий пароль является обязательным полем.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Новый пароль пользователя. Должен соответствовать заданным требованиям сложности.
    /// </summary>
    [Required(ErrorMessage = "Новый пароль является обязательным полем.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Новый пароль должен быть не менее 8 символов.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Новый пароль должен содержать как минимум одну заглавную букву, одну строчную букву, одну цифру и один специальный символ.")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение нового пароля. Должно совпадать с полем NewPassword.
    /// </summary>
    [Required(ErrorMessage = "Подтверждение нового пароля является обязательным полем.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}