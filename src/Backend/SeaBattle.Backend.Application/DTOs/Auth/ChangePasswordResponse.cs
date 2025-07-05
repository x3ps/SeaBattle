namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Ответ на запрос изменения пароля.
/// Информирует об успешности операции и содержит опциональное сообщение.
/// </summary>
public class ChangePasswordResponse
{
    /// <summary>
    /// Указывает, была ли операция изменения пароля успешной.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Сообщение, предоставляющее дополнительную информацию о результате операции
    /// (например, сообщение об ошибке, если Success = false).
    /// </summary>
    public string Message { get; set; } = string.Empty;
}