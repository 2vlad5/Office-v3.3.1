using System;
using System.Windows.Forms;
using officeApp.DataAccess;
using officeApp.Forms;

namespace officeApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Показываем информацию о подключении
            string connectionInfo = DatabaseHelper.GetConnectionInfo();
            MessageBox.Show($"Попытка подключения:\n{connectionInfo}", "Информация о подключении");

            // Проверка подключения к базе данных
            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Приложение будет закрыто.\n\n" +
                               "Проверьте:\n" +
                               "1. Правильность строки подключения в App.config\n" +
                               "2. Доступность сервера базы данных\n" +
                               "3. Правильность логина и пароля\n" +
                               "4. Разрешения пользователя базы данных",
                               "Ошибка подключения",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Подключение к базе данных успешно установлено!", "Успех",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Run(new LoginForm());
        }
    }
}