using System.Drawing;
using System.Windows.Forms;

namespace officeApp.Modules
{
    public class SalesModule : IOfficeModule
    {
        private TabPage tabPage;
        private Label lblTitle;

        public SalesModule()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            tabPage = new TabPage("📊 Продажи");
            tabPage.BackColor = Color.FromArgb(245, 245, 248);

            // Header
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(111, 66, 193);

            lblTitle = new Label();
            lblTitle.Text = "Модуль продаж";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);

            headerPanel.Controls.Add(lblTitle);

            // Content
            Label contentLabel = new Label();
            contentLabel.Text = "Здесь будет управление заказами, клиентами и продажами";
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