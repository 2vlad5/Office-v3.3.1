using System.Drawing;
using System.Windows.Forms;

namespace officeApp.Modules
{
    public class ReportsModule : IOfficeModule
    {
        private TabPage tabPage;
        private Label lblTitle;

        public ReportsModule()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            tabPage = new TabPage("📈 Отчеты");
            tabPage.BackColor = Color.FromArgb(245, 245, 248);

            // Header
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(23, 162, 184);

            lblTitle = new Label();
            lblTitle.Text = "Отчеты и аналитика";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);

            headerPanel.Controls.Add(lblTitle);

            // Content
            Label contentLabel = new Label();
            contentLabel.Text = "Здесь будут отчеты по всем модулям системы";
            contentLabel.Font = new Font("Segoe UI", 12);
            contentLabel.AutoSize = true;
            contentLabel.Location = new Point(20, 100);

            tabPage.Controls.AddRange(new Control[] { contentLabel, headerPanel });
        }

        public TabPage GetTabPage()
        {
            return tabPage;
        }
    }
}