using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace officeApp.DataAccess
{
    public static class DatabaseHelper
    {
        private static string connectionString;

        static DatabaseHelper()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["OfficeDB"].ConnectionString;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения строки подключения: {ex.Message}");
                connectionString = string.Empty;
            }
        }

        public static MySqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Строка подключения не настроена");
            }

            return new MySqlConnection(connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    // Проверяем, что база данных доступна
                    using (var command = new MySqlCommand("SELECT 1", connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null && result.ToString() == "1";
                    }
                }
            }
            catch (MySqlException mysqlEx)
            {
                ShowDetailedMySqlError(mysqlEx);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка подключения: {ex.Message}");
                return false;
            }
        }

        private static void ShowDetailedMySqlError(MySqlException mysqlEx)
        {
            string errorMessage = $"Ошибка MySQL:\n";
            errorMessage += $"\n- Сообщение: {mysqlEx.Message}";
            errorMessage += $"\n- Код ошибки: {mysqlEx.Number}";
            errorMessage += $"\n---------------------------";

            MessageBox.Show(errorMessage, "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static string GetConnectionInfo()
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString);
                return $"Сервер: {builder.Server}\nБаза данных: {builder.Database}\nПользователь: {builder.UserID}";
            }
            catch
            {
                return "Не удалось проанализировать строку подключения";
            }
        }

        public static bool CheckUsersTableStructure()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    // Проверяем существование таблицы users
                    string checkTableQuery = "SHOW TABLES LIKE 'users'";
                    MySqlCommand cmd = new MySqlCommand(checkTableQuery, connection);
                    var tableExists = cmd.ExecuteScalar() != null;

                    if (!tableExists)
                    {
                        MessageBox.Show("Таблица 'users' не найдена в базе данных");
                        return false;
                    }

                    // Проверяем существование столбцов
                    string checkColumnsQuery = @"
                SHOW COLUMNS FROM users 
                WHERE Field IN ('username', 'password')";

                    cmd = new MySqlCommand(checkColumnsQuery, connection);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int columnCount = 0;
                        while (reader.Read()) columnCount++;

                        if (columnCount < 2)
                        {
                            MessageBox.Show("В таблице 'users' отсутствуют необходимые столбцы: username, password");
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки структуры таблицы: {ex.Message}");
                return false;
            }
        }
    }
}