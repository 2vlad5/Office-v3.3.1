using officeApp.DataAccess;
using officeApp.Forms;
using officeApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace officeApp.Modules
{
    public class TaskModule : IOfficeModule
    {
        private TabPage tabPage;
        private DataGridView dgvProducts;
        private Button btnAddProduct, btnEditProduct, btnDeleteProduct, btnRefresh;
        private TextBox txtSearch;
        private ComboBox cmbFilterName, cmbFilterVolume, cmbFilterStatus;
        private Panel headerPanel, statsPanel;
        private Label lblTitle, lblTotalItems, lblLowStock, lblOutOfStock;

        public TaskModule()
        {
            InitializeComponent();
            LoadProducts();
            LoadFilterOptions();
            UpdateStatistics();
        }

        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Поиск по названию...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Поиск по названию...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void InitializeComponent()
        {
            tabPage = new TabPage("🌤 Задачи");
            tabPage.Size = new Size(1000, 700);
            tabPage.BackColor = Color.FromArgb(245, 245, 248);

            // Header Panel
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(65, 48, 110);

            lblTitle = new Label();
            lblTitle.Text = "Список задач";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);

            headerPanel.Controls.Add(lblTitle);

            // Statistics Panel
            statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 70;
            statsPanel.BackColor = Color.White;
            statsPanel.Padding = new Padding(10);

            lblTotalItems = CreateStatLabel("Общее количество: 0", 20, Color.FromArgb(59, 89, 152));
            lblLowStock = CreateStatLabel("Мало на складе: 0", 200, Color.FromArgb(255, 153, 51));
            lblOutOfStock = CreateStatLabel("Нет на складе: 0", 380, Color.FromArgb(220, 53, 69));

            statsPanel.Controls.AddRange(new Control[] { lblTotalItems, lblLowStock, lblOutOfStock });

            // Search and Filter Panel
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 60;
            searchPanel.BackColor = Color.White;
            searchPanel.Padding = new Padding(10);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(10, 15);
            txtSearch.Size = new Size(200, 30);
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Text = "Поиск по названию...";

            // Добавляем обработчики событий для placeholder эффекта
            txtSearch.Enter += TxtSearch_Enter;
            txtSearch.Leave += TxtSearch_Leave;

            cmbFilterName = CreateFilterComboBox("Все наименования", 220);
            cmbFilterVolume = CreateFilterComboBox("Все объемы", 350);
            cmbFilterStatus = CreateFilterComboBox("Все статусы", 480);

            btnRefresh = new Button();
            btnRefresh.Text = "🔄";
            btnRefresh.Font = new Font("Segoe UI", 12);
            btnRefresh.Size = new Size(40, 30);
            btnRefresh.Location = new Point(610, 15);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.BackColor = Color.FromArgb(59, 89, 152);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Click += BtnRefresh_Click;

            searchPanel.Controls.AddRange(new Control[] {
                txtSearch, cmbFilterName, cmbFilterVolume, cmbFilterStatus, btnRefresh
            });

            // DataGridView
            dgvProducts = new DataGridView();
            dgvProducts.Dock = DockStyle.Fill;
            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.ReadOnly = true;
            dgvProducts.BackgroundColor = Color.White;
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.Font = new Font("Segoe UI", 10);

            // Стилизация DataGridView
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 240, 245);
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;

            // Настройка колонок
            dgvProducts.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn() { Name = "Id", DataPropertyName = "Id", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn() { Name = "Name", DataPropertyName = "Name", HeaderText = "Наименование", Width = 180 },
                new DataGridViewTextBoxColumn() { Name = "Volume", DataPropertyName = "Volume", HeaderText = "Объем", Width = 120 },
                new DataGridViewTextBoxColumn() { Name = "Quantity", DataPropertyName = "Quantity", HeaderText = "Количество", Width = 100 },
                new DataGridViewTextBoxColumn() { Name = "Status", DataPropertyName = "Status", HeaderText = "Статус", Width = 140 }
            });

            // Добавляем колонки с кнопками +/-
            var increaseColumn = new DataGridViewButtonColumn()
            {
                HeaderText = "+",
                Text = "+",
                UseColumnTextForButtonValue = true,
                Width = 50,
                FlatStyle = FlatStyle.Flat
            };

            var decreaseColumn = new DataGridViewButtonColumn()
            {
                HeaderText = "-",
                Text = "-",
                UseColumnTextForButtonValue = true,
                Width = 50,
                FlatStyle = FlatStyle.Flat
            };

            dgvProducts.Columns.Add(increaseColumn);
            dgvProducts.Columns.Add(decreaseColumn);

            // Action Buttons Panel
            Panel actionPanel = new Panel();
            actionPanel.Dock = DockStyle.Bottom;
            actionPanel.Height = 70;
            actionPanel.BackColor = Color.White;
            actionPanel.Padding = new Padding(20);

            btnAddProduct = CreateActionButton("➕ Добавить", Color.FromArgb(40, 167, 69));
            btnEditProduct = CreateActionButton("✏️ Изменить", Color.FromArgb(23, 162, 184));
            btnDeleteProduct = CreateActionButton("🗑️ Удалить", Color.FromArgb(220, 53, 69));

            btnAddProduct.Location = new Point(20, 15);
            btnEditProduct.Location = new Point(150, 15);
            btnDeleteProduct.Location = new Point(320, 15);

            actionPanel.Controls.AddRange(new Control[] { btnAddProduct, btnEditProduct, btnDeleteProduct });

            // Добавляем обработчики событий
            btnAddProduct.Click += BtnAddProduct_Click;
            btnEditProduct.Click += BtnEditProduct_Click;
            btnDeleteProduct.Click += BtnDeleteProduct_Click;
            dgvProducts.CellClick += DgvProducts_CellClick;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cmbFilterName.SelectedIndexChanged += Filter_Changed;
            cmbFilterVolume.SelectedIndexChanged += Filter_Changed;
            cmbFilterStatus.SelectedIndexChanged += Filter_Changed;

            // Добавляем все панели на tabPage
            tabPage.Controls.AddRange(new Control[] {
                dgvProducts, actionPanel, searchPanel, statsPanel, headerPanel
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

        private void LoadProducts()
        {
            try
            {
                List<Product> products = ProductRepository.GetAllProducts();
                dgvProducts.DataSource = products;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке продуктов: {ex.Message}");
            }
        }

        private void UpdateStatistics()
        {
            if (dgvProducts.DataSource is List<Product> products)
            {
                int total = 0;
                int lowStock = 0;
                int outOfStock = 0;

                foreach (var product in products)
                {
                    total += product.Quantity;
                    if (product.Quantity == 0) outOfStock++;
                    else if (product.Quantity < 10) lowStock++;
                }

                lblTotalItems.Text = $"Общее количество: {total}";
                lblLowStock.Text = $"Мало на складе: {lowStock}";
                lblOutOfStock.Text = $"Нет на складе: {outOfStock}";
            }
        }

        private void LoadFilterOptions()
        {
            try
            {
                var nameOptions = ProductRepository.GetStorageOptions("name");
                var volumeOptions = ProductRepository.GetStorageOptions("volume");
                var statusOptions = ProductRepository.GetStorageOptions("status");

                cmbFilterName.Items.Clear();
                cmbFilterVolume.Items.Clear();
                cmbFilterStatus.Items.Clear();

                cmbFilterName.Items.Add("Все наименования");
                cmbFilterVolume.Items.Add("Все объемы");
                cmbFilterStatus.Items.Add("Все статусы");

                foreach (var option in nameOptions)
                    cmbFilterName.Items.Add(option.Value);

                foreach (var option in volumeOptions)
                    cmbFilterVolume.Items.Add(option.Value);

                foreach (var option in statusOptions)
                    cmbFilterStatus.Items.Add(option.Value);

                cmbFilterName.SelectedIndex = 0;
                cmbFilterVolume.SelectedIndex = 0;
                cmbFilterStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке фильтров: {ex.Message}");
            }
        }

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == 5) ChangeQuantity(e.RowIndex, 1);
            else if (e.ColumnIndex == 6) ChangeQuantity(e.RowIndex, -1);
        }

        private void ChangeQuantity(int rowIndex, int change)
        {
            var product = dgvProducts.Rows[rowIndex].DataBoundItem as Product;
            if (product != null)
            {
                int newQuantity = product.Quantity + change;
                if (newQuantity < 0) newQuantity = 0;

                if (ProductRepository.UpdateProductQuantity(product.Id, newQuantity))
                {
                    product.Quantity = newQuantity;
                    dgvProducts.InvalidateRow(rowIndex);
                    UpdateStatistics();
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
            LoadFilterOptions();
        }

        // Также обновите метод TxtSearch_TextChanged
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text != "Поиск по названию...")
            {
                ApplyFilters();
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            // Реализация фильтрации
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            ShowProductForm(null);
        }

        private void BtnEditProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var product = dgvProducts.SelectedRows[0].DataBoundItem as Product;
                ShowProductForm(product);
            }
            else
            {
                MessageBox.Show("Выберите продукт для редактирования");
            }
        }

        private void ShowProductForm(Product product)
        {
            ProductForm form = new ProductForm(product);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var product = dgvProducts.SelectedRows[0].DataBoundItem as Product;
                if (MessageBox.Show($"Удалить продукт '{product.Name}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (ProductRepository.DeleteProduct(product.Id))
                    {
                        LoadProducts();
                    }
                }
            }
        }

        public TabPage GetTabPage()
        {
            return tabPage;
        }
    }
}