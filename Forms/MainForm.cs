using System;
using System.Drawing;
using System.Windows.Forms;
using officeApp.Models;
using officeApp.Modules;

namespace officeApp.Forms
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private TabControl tabControlMain;

        public MainForm(User user)
        {
            currentUser = user;
            InitializeComponent();
            InitializeModules();
            UpdateUserInterface();
        }

        private void InitializeComponent()
        {
            // Основные настройки формы
            this.Text = $"Офисное приложение, (Пользователь: {currentUser.FullName})";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // TabControl
            tabControlMain = new TabControl();
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Appearance = TabAppearance.FlatButtons;
            tabControlMain.ItemSize = new Size(100, 30);
            tabControlMain.SizeMode = TabSizeMode.Fixed;

            // StatusStrip
            var statusStrip = new StatusStrip();
            statusStrip.Dock = DockStyle.Bottom;

            var lblWelcome = new ToolStripStatusLabel();
            lblWelcome.Spring = true;
            lblWelcome.TextAlign = ContentAlignment.MiddleLeft;

            var lblRole = new ToolStripStatusLabel();
            lblRole.Spring = true;
            lblRole.TextAlign = ContentAlignment.MiddleRight;

            statusStrip.Items.AddRange(new ToolStripItem[] { lblWelcome, lblRole });

            // MenuStrip
            var menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;

            var fileMenu = new ToolStripMenuItem("Управление");
            var exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, e) => Application.Exit();

            var helpMenu = new ToolStripMenuItem("Помощь");
            var infoItem = new ToolStripMenuItem("Инструкция");

            var serviseMenu = new ToolStripMenuItem("Сервис");
            var settingsItem = new ToolStripMenuItem("Настройки");
            var aboutItem = new ToolStripMenuItem("О программе");
            aboutItem.Click += (s, e) => ShowAboutDialog();

            fileMenu.DropDownItems.Add(exitItem);
            helpMenu.DropDownItems.Add(infoItem);
            serviseMenu.DropDownItems.Add(settingsItem);
            serviseMenu.DropDownItems.Add(aboutItem);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, helpMenu, serviseMenu });

            // Добавляем элементы на форму
            this.Controls.AddRange(new Control[] {
                tabControlMain,
                statusStrip,
                menuStrip
            });

            // Сохраняем ссылки для обновления
            this.Tag = new { WelcomeLabel = lblWelcome, RoleLabel = lblRole };
        }

        private void InitializeModules()
        {
            try
            {
                // Создание всех модулей
                var warehouseModule = new WarehouseModule();
                var taskModule = new TaskModule();
                var financeModule = new FinanceModule();
                var hrModule = new HRModule();
                var salesModule = new SalesModule();
                var reportsModule = new ReportsModule();
                var updatesModule = new UpdatesModule();
                var settingsModule = new SettingsModule();

                // Добавление вкладок
                tabControlMain.TabPages.Add(warehouseModule.GetTabPage());
                tabControlMain.TabPages.Add(taskModule.GetTabPage());
                tabControlMain.TabPages.Add(financeModule.GetTabPage());
                tabControlMain.TabPages.Add(hrModule.GetTabPage());
                tabControlMain.TabPages.Add(salesModule.GetTabPage());
                tabControlMain.TabPages.Add(reportsModule.GetTabPage());
                tabControlMain.TabPages.Add(updatesModule.GetTabPage());
                tabControlMain.TabPages.Add(settingsModule.GetTabPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации модулей: {ex.Message}");
            }
        }

        private void UpdateUserInterface()
        {
            var labels = this.Tag as dynamic;
            if (labels != null)
            {
                labels.WelcomeLabel.Text = $"Добро пожаловать, {currentUser.Username}";
                if (!string.IsNullOrEmpty(currentUser.FullName))
                {
                    labels.WelcomeLabel.Text += $" ({currentUser.FullName})";
                }
                labels.RoleLabel.Text = $"Роль: {currentUser.Role}";
            }
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show(
                "Офисное програмное изделие\n\nВерсия 1.0\n\nСистема управления офисом",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}