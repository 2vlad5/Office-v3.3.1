using System;
using System.Windows.Forms;
using officeApp.DataAccess;
using officeApp.Models;
using officeApp.Utilities;

namespace officeApp.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            this.AcceptButton = btnLogin; // Enter для входа
        }

        private void CheckDatabaseStructure()
        {
            try
            {
                UserRepository.CheckTableStructure();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось проверить структуру базы данных: {ex.Message}");
            }
        }

        //Кнопка дебага
        private void btnDebug_Click(object sender, EventArgs e)
        {
            CheckDatabaseStructure();
        }
        //Кнопка дебага

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Введите имя пользователя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Показываем индикатор загрузки
            btnLogin.Enabled = false;
            btnLogin.Text = "Подключение...";

            try
            {
                if (AuthenticateUser(username, password))
                {
                    User currentUser = UserRepository.GetUserByUsername(username);
                    if (currentUser != null)
                    {
                        // Обновляем время последнего входа
                        UserRepository.UpdateLastLogin(currentUser.Username);

                        MainForm mainForm = new MainForm(currentUser);
                        mainForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка получения данных пользователя", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль", "Ошибка авторизации",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Восстанавливаем кнопку
                btnLogin.Enabled = true;
                btnLogin.Text = "OK";
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            // Получаем пароль из базы данных (в открытом виде)
            string storedPassword = UserRepository.GetPassword(username);

            if (!string.IsNullOrEmpty(storedPassword))
            {
                // Простая проверка пароля без хэширования
                return PasswordHasher.VerifyPassword(password, storedPassword);
            }

            return false;
        }

        // Обработка нажатия Enter в полях ввода
        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPassword.Focus();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
                e.Handled = e.SuppressKeyPress = true;
            }
        }
    }
}