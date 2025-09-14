namespace officeApp.Utilities
{
    public static class PasswordHasher
    {
        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            // Простая проверка пароля без хэширования
            return inputPassword == storedPassword;
        }
    }
}