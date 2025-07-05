namespace SeaBattle.Backend.Application.DTOs.Auth;

/// <summary>
/// Ответ на запрос отзыва Refresh Token.
/// Информирует об успешности операции отзыва.
/// </summary>
public class RevokeTokenResponse
{
    /// <summary>
    /// Указывает, была ли операция отзыва токена успешной.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Сообщение, предоставляющее дополнительную информацию о результате операции
    /// (например, сообщение об ошибке, если Success = false).
    /// </summary>
    public string Message { get; set; } = string.Empty;
}