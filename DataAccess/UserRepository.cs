using MySql.Data.MySqlClient;
using officeApp.Models;
using System;
using System.Windows.Forms;

namespace officeApp.DataAccess
{
    public static class UserRepository
    {
        public static string GetPassword(string username)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                // Используем правильное имя столбца password
                string query = "SELECT password FROM users WHERE username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    var result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка получения пароля: {ex.Message}");
                    return null;
                }
            }
        }

        public static User GetUserByUsername(string username)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                // Используем правильные имена столбцов из вашей БД
                string query = @"SELECT 
                    username, 
                    role, 
                    first_name, 
                    middle_name, 
                    last_name, 
                    email 
                FROM users WHERE username = @Username";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Формируем полное имя из компонентов
                            string firstName = reader.IsDBNull(reader.GetOrdinal("first_name")) ? "" : reader.GetString("first_name");
                            string middleName = reader.IsDBNull(reader.GetOrdinal("middle_name")) ? "" : reader.GetString("middle_name");
                            string lastName = reader.IsDBNull(reader.GetOrdinal("last_name")) ? "" : reader.GetString("last_name");

                            string fullName = $"{lastName} {firstName} {middleName}".Trim();

                            return new User
                            {
                                Username = reader.GetString("username"),
                                Role = reader.IsDBNull(reader.GetOrdinal("role")) ? "User" : reader.GetString("role"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                                FullName = fullName
                            };
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден в базе данных", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка получения данных пользователя: {ex.Message}\n\n" +
                                  $"Проверьте структуру таблицы users", "Ошибка базы данных",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Diagnostics.Debug.WriteLine($"Ошибка получения пользователя: {ex.Message}");
                }
            }
            return null;
        }

        public static bool UserExists(string username)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM users WHERE username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки пользователя: {ex.Message}");
                    return false;
                }
            }
        }

        public static void UpdateLastLogin(string username)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                // Обновляем время последнего входа
                string query = "UPDATE users SET last_login = NOW() WHERE username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка обновления времени входа: {ex.Message}");
                }
            }
        }

        // Метод для диагностики - показывает все столбцы таблицы users
        public static void CheckTableStructure()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                string query = "DESCRIBE users";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        string columns = "Столбцы таблицы users:\n";
                        while (reader.Read())
                        {
                            columns += $"- {reader.GetString(0)} ({reader.GetString(1)})\n";
                        }
                        MessageBox.Show(columns, "Структура таблицы", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка получения структуры таблицы: {ex.Message}");
                }
            }
        }
    }
}