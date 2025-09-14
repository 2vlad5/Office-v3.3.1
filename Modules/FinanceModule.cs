using System.Drawing;
using System.Windows.Forms;

namespace officeApp.Modules
{
    public class FinanceModule : IOfficeModule
    {
        private TabPage tabPage;
        private Label lblTitle;

        public FinanceModule()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            tabPage = new TabPage("💰 Финансы");
            tabPage.BackColor = Color.FromArgb(245, 245, 248);

            // Header
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(40, 167, 69);

            lblTitle = new Label();
            lblTitle.Text = "Финансы";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);

            headerPanel.Controls.Add(lblTitle);

            // Content
            Label contentLabel = new Label();
            contentLabel.Text = "Здесь будет личная финансовая отчетность";
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