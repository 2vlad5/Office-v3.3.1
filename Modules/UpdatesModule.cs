using officeApp.DataAccess;
using officeApp.Forms;
using officeApp.Models;
using officeApp.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace officeApp.Modules
{
    public class UpdatesModule : IOfficeModule
    {
        private TabPage tabPage;
        private Panel headerPanel, contentPanel, actionPanel;
        private Label lblTitle, lblCurrentVersion, lblDatabaseVersion, lblUpdateStatus;
        private Button btnCheckUpdates, btnDownloadUpdate;
        private ProgressBar progressBar;

        public UpdatesModule()
        {
            InitializeComponent();
            CheckAndDisplayUpdateInfo();
        }


        private void InitializeComponent()
        {
            tabPage = new TabPage("🌤 Обновления");
            tabPage.Size = new Size(1000, 700);
            tabPage.BackColor = Color.FromArgb(245, 245, 248);

            // Header Panel
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(65, 48, 110);

            lblTitle = new Label();
            lblTitle.Text = "Управление обновлениями";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);

            headerPanel.Controls.Add(lblTitle);

            // Content Panel for update info
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.White;
            contentPanel.Padding = new Padding(20);

            // Current version info
            lblCurrentVersion = new Label();
            lblCurrentVersion.Text = "Текущая версия: Загрузка...";
            lblCurrentVersion.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblCurrentVersion.ForeColor = Color.FromArgb(52, 73, 94);
            lblCurrentVersion.Location = new Point(20, 20);
            lblCurrentVersion.AutoSize = true;

            // Database version info
            lblDatabaseVersion = new Label();
            lblDatabaseVersion.Text = "Доступная версия: Загрузка...";
            lblDatabaseVersion.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblDatabaseVersion.ForeColor = Color.FromArgb(52, 73, 94);
            lblDatabaseVersion.Location = new Point(20, 60);
            lblDatabaseVersion.AutoSize = true;

            // Update status
            lblUpdateStatus = new Label();
            lblUpdateStatus.Text = "Статус: Проверка...";
            lblUpdateStatus.Font = new Font("Segoe UI", 11);
            lblUpdateStatus.ForeColor = Color.FromArgb(108, 117, 125);
            lblUpdateStatus.Location = new Point(20, 100);
            lblUpdateStatus.AutoSize = true;

            contentPanel.Controls.AddRange(new Control[] { lblCurrentVersion, lblDatabaseVersion, lblUpdateStatus });

            // Action Panel for update buttons
            actionPanel = new Panel();
            actionPanel.Dock = DockStyle.Bottom;
            actionPanel.Height = 80;
            actionPanel.BackColor = Color.FromArgb(248, 249, 250);
            actionPanel.Padding = new Padding(20);

            btnCheckUpdates = new Button();
            btnCheckUpdates.Text = "🔍 Проверить обновления";
            btnCheckUpdates.Size = new Size(200, 40);
            btnCheckUpdates.Location = new Point(20, 20);
            btnCheckUpdates.BackColor = Color.FromArgb(59, 89, 152);
            btnCheckUpdates.ForeColor = Color.White;
            btnCheckUpdates.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCheckUpdates.FlatStyle = FlatStyle.Flat;
            btnCheckUpdates.Cursor = Cursors.Hand;
            btnCheckUpdates.Click += BtnCheckUpdates_Click;

            btnDownloadUpdate = new Button();
            btnDownloadUpdate.Text = "📥 Загрузить обновление";
            btnDownloadUpdate.Size = new Size(200, 40);
            btnDownloadUpdate.Location = new Point(240, 20);
            btnDownloadUpdate.BackColor = Color.FromArgb(40, 167, 69);
            btnDownloadUpdate.ForeColor = Color.White;
            btnDownloadUpdate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnDownloadUpdate.FlatStyle = FlatStyle.Flat;
            btnDownloadUpdate.Cursor = Cursors.Hand;
            btnDownloadUpdate.Enabled = false;
            btnDownloadUpdate.Click += BtnDownloadUpdate_Click;

            // Progress bar for downloads
            progressBar = new ProgressBar();
            progressBar.Location = new Point(460, 30);
            progressBar.Size = new Size(200, 20);
            progressBar.Visible = false;

            actionPanel.Controls.AddRange(new Control[] { btnCheckUpdates, btnDownloadUpdate, progressBar });

            // Добавляем все панели на tabPage
            tabPage.Controls.AddRange(new Control[] {
                contentPanel, actionPanel, headerPanel
            });
        }

        private Label CreateStatLabel(string text, int x, Color color)
        {
            return new Label()
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = color,
                AutoSize = true,
                Location = new Point(x, 20)
            };
        }

        private ComboBox CreateFilterComboBox(string text, int x)
        {
            var cmb = new ComboBox()
            {
                Location = new Point(x, 15),
                Size = new Size(120, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmb.Items.Add(text);
            return cmb;
        }

        private Button CreateActionButton(string text, Color backColor)
        {
            return new Button()
            {
                Text = text,
                Size = new Size(140, 40),
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        /// <summary>
        /// Проверяет и отображает информацию об обновлениях
        /// </summary>
        private void CheckAndDisplayUpdateInfo()
        {
            try
            {
                string currentVersion = ProductRepository.GetCurrentAppVersion();
                string databaseVersion = ProductRepository.GetAppVersionFromDatabase();

                lblCurrentVersion.Text = $"Текущая версия: {currentVersion}";
                lblDatabaseVersion.Text = $"Доступная версия: {databaseVersion}";

                if (currentVersion.Equals(databaseVersion))
                {
                    lblUpdateStatus.Text = "Статус: У вас установлена актуальная версия";
                    lblUpdateStatus.ForeColor = Color.FromArgb(40, 167, 69);
                    btnDownloadUpdate.Enabled = false;
                }
                else
                {
                    lblUpdateStatus.Text = "Статус: Доступно обновление!";
                    lblUpdateStatus.ForeColor = Color.FromArgb(220, 53, 69);
                    btnDownloadUpdate.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblCurrentVersion.Text = "Текущая версия: Ошибка загрузки";
                lblDatabaseVersion.Text = "Доступная версия: Ошибка загрузки";
                lblUpdateStatus.Text = $"Статус: Ошибка - {ex.Message}";
                lblUpdateStatus.ForeColor = Color.FromArgb(220, 53, 69);
            }
        }

        /// <summary>
        /// Обработчик кнопки проверки обновлений
        /// </summary>
        private void BtnCheckUpdates_Click(object sender, EventArgs e)
        {
            CheckAndDisplayUpdateInfo();
            MessageBox.Show("Проверка обновлений завершена", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Обработчик кнопки загрузки обновления
        /// </summary>
        private async void BtnDownloadUpdate_Click(object sender, EventArgs e)
        {
            btnDownloadUpdate.Enabled = false;
            btnCheckUpdates.Enabled = false;
            progressBar.Visible = true;
            progressBar.Value = 0;

            try
            {
                // Показываем диалог загрузки обновления
                bool result = await UpdateService.ShowUpdateDialogAsync();
                
                if (result)
                {
                    CheckAndDisplayUpdateInfo(); // Обновляем информацию после загрузки
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке обновления: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDownloadUpdate.Enabled = true;
                btnCheckUpdates.Enabled = true;
                progressBar.Visible = false;
            }
        }

        public TabPage GetTabPage()
        {
            return tabPage;
        }
    }
}