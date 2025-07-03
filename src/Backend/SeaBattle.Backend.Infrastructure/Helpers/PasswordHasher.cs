using System.Security.Cryptography;
using SeaBattle.Backend.Domain.Helpers;

namespace SeaBattle.Backend.Infrastructure.Helpers;

public class PasswordHasher : IPasswordHasher
{
    // Количество итераций для PBKDF2. Больше итераций = выше безопасность, но медленнее.
    // Это значение можно регулировать.
    private const int Iterations = 10000;

    // Размер соли в байтах. Рекомендуется 16 байт (128 бит) или более.
    private const int SaltSize = 16;

    // Размер хеша в байтах. Рекомендуется 32 байта (256 бит) или более.
    private const int HashSize = 32;

    // Разделитель для соли и хеша в итоговой строке.
    private const char Delimiter = ':';

    /// <summary>
    /// Хеширует пароль, используя PBKDF2 и случайную соль.
    /// </summary>
    /// <param name="password">Пароль для хеширования.</param>
    /// <returns>Хешированный пароль в формате "соль:хеш".</returns>
    public string HashPassword(string password)
    {
        // Генерируем случайную соль.
        byte[] salt;
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt = new byte[SaltSize]);
        }

        // Вычисляем хеш пароля с использованием PBKDF2.
        // Rfc2898DeriveBytes рекомендуемый класс для PBKDF2.
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Объединяем соль и хеш в одну строку для хранения.
            // Используем Base64 для преобразования байтов в строки.
            return $"{Convert.ToBase64String(salt)}{Delimiter}{Convert.ToBase64String(hash)}";
        }
    }

    /// <summary>
    /// Проверяет, соответствует ли введенный пароль хешированному.
    /// </summary>
    /// <param name="password">Введенный пользователем пароль.</param>
    /// <param name="hashedPassword">Хешированный пароль, полученный из базы данных (формат "соль:хеш").</param>
    /// <returns>True, если пароли совпадают; в противном случае False.</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        // Проверяем формат хешированного пароля.
        var parts = hashedPassword.Split(Delimiter);
        if (parts.Length != 2)
        {
            // Неверный формат хешированного пароля.
            return false;
        }

        try
        {
            // Извлекаем соль и хеш из хешированной строки.
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);

            // Вычисляем хеш введенного пароля с извлеченной солью.
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);

                // Сравниваем вычисленный хеш с хешем, хранящимся в базе данных.
                // Используем SequenceEqual для безопасного сравнения массивов байтов.
                return computedHash.SequenceEqual(hash);
            }
        }
        catch
        {
            // Ошибка при декодировании Base64 или другая ошибка.
            return false;
        }
    }
}