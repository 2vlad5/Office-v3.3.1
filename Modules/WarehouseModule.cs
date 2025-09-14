using officeApp.DataAccess;
using officeApp.Forms;
using officeApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace officeApp.Modules
{
    public class WarehouseModule : IOfficeModule
    {
        private TabPage tabPage;
        private SplitContainer mainSplitContainer;
        
        // Left panel - Groups TreeView
        private TreeView treeGroups;
        private Button btnManageGroups, btnRefreshGroups;
        private Label lblGroupsTitle;
        
        // Right panel - Products grid and controls  
        private DataGridView dgvProducts;
        private Button btnAddProduct, btnEditProduct, btnDeleteProduct, btnAssignGroup, btnManageTypes;
        private TextBox txtSearch;
        private ComboBox cmbFilterVolume, cmbFilterStatus, cmbFilterType, cmbSortBy;
        private CheckBox chkSortDesc, chkSelectAll;
        private Panel headerPanel, searchPanel, statsPanel, actionPanel, massActionsPanel;
        private Button btnMassChangeType, btnMassChangeStatus, btnMassDelete;
        private Label lblTitle, lblTotalItems, lblLowStock, lblOutOfStock, lblSelectedGroup;

        private List<Product> allProducts;
        private List<Product> filteredProducts;
        private string currentSelectedGroup = "";

        public WarehouseModule()
        {
            InitializeComponent();
            LoadProductsAndGroups();
            LoadFilterOptions();
            UpdateStatistics();
        }

        private void InitializeComponent()
        {
            tabPage = new TabPage("📦 Склад");
            tabPage.Size = new Size(1200, 800);
            tabPage.BackColor = Color.FromArgb(245, 245, 248);

            // Create main split container
            mainSplitContainer = new SplitContainer();
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.SplitterDistance = 280;
            mainSplitContainer.FixedPanel = FixedPanel.Panel1;
            mainSplitContainer.BorderStyle = BorderStyle.FixedSingle;

            InitializeLeftPanel();
            InitializeRightPanel();

            tabPage.Controls.Add(mainSplitContainer);
        }

        private void InitializeLeftPanel()
        {
            // Left panel setup
            mainSplitContainer.Panel1.BackColor = Color.FromArgb(248, 249, 250);

            // Groups title
            lblGroupsTitle = new Label();
            lblGroupsTitle.Text = "📁 Группы товаров";
            lblGroupsTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblGroupsTitle.ForeColor = Color.FromArgb(73, 80, 87);
            lblGroupsTitle.Location = new Point(15, 15);
            lblGroupsTitle.AutoSize = true;

            // TreeView for groups - make it responsive
            treeGroups = new TreeView();
            treeGroups.Location = new Point(15, 50);
            treeGroups.Size = new Size(250, 495);
            treeGroups.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            treeGroups.Font = new Font("Segoe UI", 10);
            treeGroups.BorderStyle = BorderStyle.FixedSingle;
            treeGroups.BackColor = Color.White;
            treeGroups.ShowLines = true;
            treeGroups.ShowPlusMinus = true;
            treeGroups.ShowRootLines = false;
            treeGroups.HideSelection = false;
            treeGroups.AfterSelect += TreeGroups_AfterSelect;

            // Group management buttons - make them anchored to bottom
            btnManageGroups = CreateGroupButton("⚙️ Управление", 15, 565, Color.FromArgb(108, 117, 125));
            btnRefreshGroups = CreateGroupButton("🔄 Обновить", 140, 565, Color.FromArgb(23, 162, 184));
            btnManageGroups.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRefreshGroups.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            btnManageGroups.Click += BtnManageGroups_Click;
            btnRefreshGroups.Click += BtnRefreshGroups_Click;

            mainSplitContainer.Panel1.Controls.AddRange(new Control[] {
                lblGroupsTitle, treeGroups, btnManageGroups, btnRefreshGroups
            });
        }

        private void InitializeRightPanel()
        {
            // Header Panel
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.FromArgb(52, 73, 94);

            lblTitle = new Label();
            lblTitle.Text = "Управление складом";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 15);
            lblTitle.AutoSize = true;

            lblSelectedGroup = new Label();
            lblSelectedGroup.Text = "Показаны: Все товары";
            lblSelectedGroup.Font = new Font("Segoe UI", 10);
            lblSelectedGroup.ForeColor = Color.FromArgb(189, 195, 199);
            lblSelectedGroup.Location = new Point(20, 45);
            lblSelectedGroup.AutoSize = true;

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSelectedGroup });

            // Statistics Panel
            statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 70;
            statsPanel.BackColor = Color.White;
            statsPanel.Padding = new Padding(10);

            lblTotalItems = CreateStatLabel("Общее количество: 0", 20, Color.FromArgb(52, 73, 94));
            lblLowStock = CreateStatLabel("Мало на складе: 0", 220, Color.FromArgb(230, 126, 34));
            lblOutOfStock = CreateStatLabel("Нет на складе: 0", 420, Color.FromArgb(231, 76, 60));

            statsPanel.Controls.AddRange(new Control[] { lblTotalItems, lblLowStock, lblOutOfStock });

            // Search and Filter Panel
            searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 60;
            searchPanel.BackColor = Color.White;
            searchPanel.Padding = new Padding(10);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(15, 15);
            txtSearch.Size = new Size(250, 30);
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Text = "🔍 Поиск по названию...";
            txtSearch.Enter += TxtSearch_Enter;
            txtSearch.Leave += TxtSearch_Leave;
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbFilterVolume = CreateFilterComboBox("Все объемы", 280);
            cmbFilterStatus = CreateFilterComboBox("Все статусы", 420);
            cmbFilterType = CreateFilterComboBox("Все типы", 560);

            Label lblSort = new Label()
            {
                Text = "Сортировка:",
                Location = new Point(720, 18),
                Size = new Size(70, 20),
                Font = new Font("Segoe UI", 9)
            };

            cmbSortBy = new ComboBox()
            {
                Location = new Point(795, 15),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSortBy.Items.AddRange(new string[] {
                "Наименование", "Объем", "Количество", "Статус", "Группа", "Тип"
            });
            cmbSortBy.SelectedIndex = 0;
            cmbSortBy.SelectedIndexChanged += Filter_Changed;

            chkSortDesc = new CheckBox()
            {
                Text = "Убыв.",
                Location = new Point(925, 17),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 9)
            };
            chkSortDesc.CheckedChanged += Filter_Changed;

            Button btnClearFilters = new Button();
            btnClearFilters.Text = "❌ Сбросить";
            btnClearFilters.Font = new Font("Segoe UI", 9);
            btnClearFilters.Size = new Size(80, 30);
            btnClearFilters.Location = new Point(995, 15);
            btnClearFilters.FlatStyle = FlatStyle.Flat;
            btnClearFilters.BackColor = Color.FromArgb(108, 117, 125);
            btnClearFilters.ForeColor = Color.White;
            btnClearFilters.Click += BtnClearFilters_Click;

            searchPanel.Controls.AddRange(new Control[] {
                txtSearch, cmbFilterVolume, cmbFilterStatus, cmbFilterType, 
                lblSort, cmbSortBy, chkSortDesc, btnClearFilters
            });

            // Mass Actions Panel
            massActionsPanel = new Panel();
            massActionsPanel.Dock = DockStyle.Top;
            massActionsPanel.Height = 50;
            massActionsPanel.BackColor = Color.FromArgb(248, 249, 250);
            massActionsPanel.Padding = new Padding(10);
            massActionsPanel.Visible = false; // Скрываем по умолчанию

            chkSelectAll = new CheckBox()
            {
                Text = "Выделить все",
                Location = new Point(15, 12),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9)
            };
            chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;

            btnMassChangeType = new Button()
            {
                Text = "🏷️ Изменить тип",
                Location = new Point(125, 10),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMassChangeType.Click += BtnMassChangeType_Click;

            btnMassChangeStatus = new Button()
            {
                Text = "🔄 Изменить статус",
                Location = new Point(255, 10),
                Size = new Size(130, 30),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnMassChangeStatus.Click += BtnMassChangeStatus_Click;

            btnMassDelete = new Button()
            {
                Text = "🗑️ Удалить выделенные",
                Location = new Point(395, 10),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMassDelete.Click += BtnMassDelete_Click;

            massActionsPanel.Controls.AddRange(new Control[] {
                chkSelectAll, btnMassChangeType, btnMassChangeStatus, btnMassDelete
            });

            // DataGridView
            dgvProducts = new DataGridView();
            dgvProducts.Dock = DockStyle.Fill;
            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.ReadOnly = false; // Разрешаем редактирование для checkbox
            dgvProducts.BackgroundColor = Color.White;
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.Font = new Font("Segoe UI", 10);
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.AllowUserToResizeRows = false;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.MultiSelect = false;

            // Enhanced grid styling
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProducts.DefaultCellStyle.BackColor = Color.White;
            dgvProducts.DefaultCellStyle.ForeColor = Color.FromArgb(44, 62, 80);
            dgvProducts.GridColor = Color.FromArgb(236, 240, 241);
            dgvProducts.CellFormatting += DgvProducts_CellFormatting;
            dgvProducts.CellContentClick += DgvProducts_CellContentClick;
            dgvProducts.CurrentCellDirtyStateChanged += DgvProducts_CurrentCellDirtyStateChanged;

            // Setup enhanced columns
            SetupDataGridViewColumns();

            dgvProducts.CellClick += DgvProducts_CellClick;
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;

            // Action Panel
            actionPanel = new Panel();
            actionPanel.Dock = DockStyle.Bottom;
            actionPanel.Height = 80;
            actionPanel.BackColor = Color.FromArgb(248, 249, 250);
            actionPanel.Padding = new Padding(20);

            btnAddProduct = CreateActionButton("➕ Добавить", Color.FromArgb(40, 167, 69), 20, 15);
            btnEditProduct = CreateActionButton("✏️ Редактировать", Color.FromArgb(23, 162, 184), 180, 15);
            btnDeleteProduct = CreateActionButton("🗑️ Удалить", Color.FromArgb(220, 53, 69), 360, 15);
            btnAssignGroup = CreateActionButton("📁 Назначить группу", Color.FromArgb(108, 117, 125), 520, 15);
            btnManageTypes = CreateActionButton("🏷️ Типы", Color.FromArgb(156, 39, 176), 680, 15);
            Button btnExportExcel = CreateActionButton("📊 Экспорт Excel", Color.FromArgb(75, 192, 192), 840, 15);

            btnAddProduct.Click += BtnAddProduct_Click;
            btnEditProduct.Click += BtnEditProduct_Click;
            btnDeleteProduct.Click += BtnDeleteProduct_Click;
            btnExportExcel.Click += BtnExportExcel_Click;
            btnAssignGroup.Click += BtnAssignGroup_Click;
            btnManageTypes.Click += BtnManageTypes_Click;

            actionPanel.Controls.AddRange(new Control[] {
                btnAddProduct, btnEditProduct, btnDeleteProduct, btnAssignGroup, btnManageTypes, btnExportExcel
            });

            // Add all panels to right panel
            mainSplitContainer.Panel2.Controls.AddRange(new Control[] {
                dgvProducts, actionPanel, massActionsPanel, searchPanel, statsPanel, headerPanel
            });
        }

        private void SetupDataGridViewColumns()
        {
            dgvProducts.Columns.Clear();
            
            dgvProducts.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn() {
                    Name = "Selected", HeaderText = "☑",
                    Width = 40, ReadOnly = false
                },
                new DataGridViewTextBoxColumn() {
                    Name = "Id", DataPropertyName = "Id", HeaderText = "ID",
                    Width = 60, ReadOnly = true, Visible = false
                },
                new DataGridViewTextBoxColumn() {
                    Name = "Name", DataPropertyName = "Name", HeaderText = "📦 Наименование",
                    Width = 180, ReadOnly = true
                },
                new DataGridViewTextBoxColumn() {
                    Name = "Volume", DataPropertyName = "Volume", HeaderText = "📏 Объем",
                    Width = 90, ReadOnly = true
                },
                new DataGridViewTextBoxColumn() {
                    Name = "Quantity", DataPropertyName = "Quantity", HeaderText = "📊 Количество",
                    Width = 90, ReadOnly = true
                },
                new DataGridViewTextBoxColumn() {
                    Name = "Status", DataPropertyName = "Status", HeaderText = "🔄 Статус",
                    Width = 100, ReadOnly = true
                },
                new DataGridViewTextBoxColumn() {
                    Name = "Group", DataPropertyName = "Group", HeaderText = "📁 Группа",
                    Width = 110, ReadOnly = true
                },
                new DataGridViewTextBoxColumn() {
                    Name = "Type", DataPropertyName = "Type", HeaderText = "🏷️ Тип",
                    Width = 90, ReadOnly = true
                }
            });

            // Add action buttons columns
            var increaseColumn = new DataGridViewButtonColumn()
            {
                HeaderText = "➕",
                Text = "+1",
                UseColumnTextForButtonValue = true,
                Width = 60,
                FlatStyle = FlatStyle.Flat,
                Name = "IncreaseColumn"
            };

            var decreaseColumn = new DataGridViewButtonColumn()
            {
                HeaderText = "➖",
                Text = "-1",
                UseColumnTextForButtonValue = true,
                Width = 60,
                FlatStyle = FlatStyle.Flat,
                Name = "DecreaseColumn"
            };

            var notifyColumn = new DataGridViewButtonColumn()
            {
                HeaderText = "📧",
                Text = "Уведомить",
                UseColumnTextForButtonValue = false,
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                Name = "NotifyColumn"
            };

            dgvProducts.Columns.Add(increaseColumn);
            dgvProducts.Columns.Add(decreaseColumn);
            dgvProducts.Columns.Add(notifyColumn);

            // Style headers
            foreach (DataGridViewColumn column in dgvProducts.Columns)
            {
                column.HeaderCell.Style.BackColor = Color.FromArgb(52, 73, 94);
                column.HeaderCell.Style.ForeColor = Color.White;
                column.HeaderCell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        private Button CreateGroupButton(string text, int x, int y, Color color)
        {
            return new Button()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 35),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private Label CreateStatLabel(string text, int x, Color color)
        {
            return new Label()
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = color,
                AutoSize = true,
                Location = new Point(x, 25)
            };
        }

        private ComboBox CreateFilterComboBox(string text, int x)
        {
            var cmb = new ComboBox()
            {
                Location = new Point(x, 15),
                Size = new Size(130, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmb.Items.Add(text);
            cmb.SelectedIndex = 0;
            cmb.SelectedIndexChanged += Filter_Changed;
            return cmb;
        }

        private Button CreateActionButton(string text, Color backColor, int x, int y)
        {
            return new Button()
            {
                Text = text,
                Size = new Size(150, 40),
                Location = new Point(x, y),
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void LoadProductsAndGroups()
        {
            try
            {
                allProducts = ProductRepository.GetAllProducts();
                filteredProducts = new List<Product>(allProducts);
                LoadGroupsTree();
                RefreshProductsDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGroupsTree()
        {
            treeGroups.Nodes.Clear();

            // Add "All products" node
            var allNode = new TreeNode("📦 Все товары");
            allNode.Tag = "";
            allNode.ForeColor = Color.FromArgb(52, 73, 94);
            allNode.NodeFont = new Font("Segoe UI", 10, FontStyle.Bold);
            treeGroups.Nodes.Add(allNode);

            // Add "No group" node
            var ungroupedProducts = allProducts.Where(p => string.IsNullOrEmpty(p.Group)).ToList();
            var noGroupNode = new TreeNode($"📂 Без группы ({ungroupedProducts.Count})");
            noGroupNode.Tag = "NO_GROUP";
            noGroupNode.ForeColor = Color.FromArgb(149, 165, 166);
            treeGroups.Nodes.Add(noGroupNode);

            // Add group nodes
            var groups = ProductRepository.GetProductGroups();
            foreach (var group in groups.OrderBy(g => g))
            {
                var productsInGroup = allProducts.Where(p => p.Group == group).ToList();
                var groupNode = new TreeNode($"📁 {group} ({productsInGroup.Count})");
                groupNode.Tag = group;
                groupNode.ForeColor = Color.FromArgb(52, 73, 94);
                treeGroups.Nodes.Add(groupNode);
            }

            // Select "All products" by default
            if (treeGroups.Nodes.Count > 0)
            {
                treeGroups.SelectedNode = allNode;
                currentSelectedGroup = "";
            }
        }

        private void RefreshProductsDisplay()
        {
            ApplyFilters();
            dgvProducts.DataSource = filteredProducts;
            UpdateStatistics();
        }

        private void TreeGroups_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag != null)
            {
                string selectedGroup = e.Node.Tag.ToString();
                currentSelectedGroup = selectedGroup;

                if (selectedGroup == "")
                {
                    lblSelectedGroup.Text = "Показаны: Все товары";
                    filteredProducts = new List<Product>(allProducts);
                }
                else if (selectedGroup == "NO_GROUP")
                {
                    lblSelectedGroup.Text = "Показаны: Товары без группы";
                    filteredProducts = allProducts.Where(p => string.IsNullOrEmpty(p.Group)).ToList();
                }
                else
                {
                    lblSelectedGroup.Text = $"Показаны: Группа \"{selectedGroup}\"";
                    filteredProducts = allProducts.Where(p => p.Group == selectedGroup).ToList();
                }

                ApplyFilters();
                dgvProducts.DataSource = filteredProducts;
                UpdateStatistics();
            }
        }

        private void ApplyFilters()
        {
            var baseProducts = GetBaseProductsByGroup();

            // Apply search filter
            if (!string.IsNullOrEmpty(txtSearch.Text) && txtSearch.Text != "🔍 Поиск по названию...")
            {
                baseProducts = baseProducts.Where(p => 
                    p.Name.ToLower().Contains(txtSearch.Text.ToLower()) ||
                    p.Volume.ToLower().Contains(txtSearch.Text.ToLower())).ToList();
            }

            // Apply volume filter
            if (cmbFilterVolume.SelectedIndex > 0)
            {
                string selectedVolume = cmbFilterVolume.SelectedItem.ToString();
                baseProducts = baseProducts.Where(p => p.Volume == selectedVolume).ToList();
            }

            // Apply status filter
            if (cmbFilterStatus.SelectedIndex > 0)
            {
                string selectedStatus = cmbFilterStatus.SelectedItem.ToString();
                baseProducts = baseProducts.Where(p => p.Status == selectedStatus).ToList();
            }

            // Apply type filter
            if (cmbFilterType.SelectedIndex > 0)
            {
                string selectedType = cmbFilterType.SelectedItem.ToString();
                baseProducts = baseProducts.Where(p => p.Type == selectedType).ToList();
            }

            filteredProducts = baseProducts;
            
            // Apply sorting
            ApplySorting();
        }

        private void ApplySorting()
        {
            if (filteredProducts == null || filteredProducts.Count == 0)
                return;

            string sortBy = cmbSortBy.SelectedItem?.ToString() ?? "Наименование";
            bool descending = chkSortDesc.Checked;

            switch (sortBy)
            {
                case "Наименование":
                    filteredProducts = descending ? 
                        filteredProducts.OrderByDescending(p => p.Name).ToList() :
                        filteredProducts.OrderBy(p => p.Name).ToList();
                    break;
                case "Объем":
                    filteredProducts = descending ? 
                        filteredProducts.OrderByDescending(p => p.Volume).ToList() :
                        filteredProducts.OrderBy(p => p.Volume).ToList();
                    break;
                case "Количество":
                    filteredProducts = descending ? 
                        filteredProducts.OrderByDescending(p => p.Quantity).ToList() :
                        filteredProducts.OrderBy(p => p.Quantity).ToList();
                    break;
                case "Статус":
                    filteredProducts = descending ? 
                        filteredProducts.OrderByDescending(p => p.Status).ToList() :
                        filteredProducts.OrderBy(p => p.Status).ToList();
                    break;
                case "Группа":
                    filteredProducts = descending ? 
                        filteredProducts.OrderByDescending(p => p.Group ?? "").ToList() :
                        filteredProducts.OrderBy(p => p.Group ?? "").ToList();
                    break;
                case "Тип":
                    filteredProducts = descending ? 
                        filteredProducts.OrderByDescending(p => p.Type ?? "").ToList() :
                        filteredProducts.OrderBy(p => p.Type ?? "").ToList();
                    break;
            }
        }

        private List<Product> GetBaseProductsByGroup()
        {
            if (currentSelectedGroup == "")
            {
                return new List<Product>(allProducts);
            }
            else if (currentSelectedGroup == "NO_GROUP")
            {
                return allProducts.Where(p => string.IsNullOrEmpty(p.Group)).ToList();
            }
            else
            {
                return allProducts.Where(p => p.Group == currentSelectedGroup).ToList();
            }
        }

        private void UpdateStatistics()
        {
            if (filteredProducts != null)
            {
                int total = filteredProducts.Sum(p => p.Quantity);
                int lowStock = filteredProducts.Count(p => p.Quantity > 0 && p.Quantity < 10);
                int outOfStock = filteredProducts.Count(p => p.Quantity == 0);

                lblTotalItems.Text = $"Общее количество: {total}";
                lblLowStock.Text = $"Мало на складе: {lowStock}";
                lblOutOfStock.Text = $"Нет на складе: {outOfStock}";
            }
        }

        private void LoadFilterOptions()
        {
            try
            {
                var volumeOptions = ProductRepository.GetStorageOptions("volume");
                var statusOptions = ProductRepository.GetStorageOptions("status");
                var typeOptions = ProductRepository.GetStorageOptions("type");

                cmbFilterVolume.Items.Clear();
                cmbFilterStatus.Items.Clear();
                cmbFilterType.Items.Clear();

                cmbFilterVolume.Items.Add("Все объемы");
                cmbFilterStatus.Items.Add("Все статусы");
                cmbFilterType.Items.Add("Все типы");

                foreach (var option in volumeOptions)
                    cmbFilterVolume.Items.Add(option.Value);

                foreach (var option in statusOptions)
                    cmbFilterStatus.Items.Add(option.Value);

                foreach (var option in typeOptions)
                    cmbFilterType.Items.Add(option.Value);

                cmbFilterVolume.SelectedIndex = 0;
                cmbFilterStatus.SelectedIndex = 0;
                cmbFilterType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке фильтров: {ex.Message}");
            }
        }

        // Event Handlers
        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "🔍 Поиск по названию...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "🔍 Поиск по названию...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text != "🔍 Поиск по названию...")
            {
                RefreshProductsDisplay();
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            RefreshProductsDisplay();
        }

        private void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "🔍 Поиск по названию...";
            txtSearch.ForeColor = Color.Gray;
            cmbFilterVolume.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterType.SelectedIndex = 0;
            cmbSortBy.SelectedIndex = 0;
            chkSortDesc.Checked = false;
            RefreshProductsDisplay();
        }

        private void DgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvProducts.Rows[e.RowIndex].DataBoundItem is Product product)
            {
                // Проверяем, является ли тип продукта "Образ" - для таких товаров не применяем цветовое выделение
                bool isObrazType = product.Type.Equals("Образ", StringComparison.OrdinalIgnoreCase);

                if (isObrazType)
                {
                    // Для товаров типа "Образ" всегда используем стандартные цвета
                    e.CellStyle.BackColor = Color.White;
                    e.CellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                }
                else
                {
                    // Color coding based on stock levels - use e.CellStyle to avoid persistent formatting issues
                    if (product.Quantity == 0)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(253, 237, 237);
                        e.CellStyle.ForeColor = Color.FromArgb(183, 28, 28);
                    }
                    else if (product.Quantity < 10)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 248, 225);
                        e.CellStyle.ForeColor = Color.FromArgb(191, 144, 0);
                    }
                    else
                    {
                        // Reset to normal colors for adequate stock levels
                        e.CellStyle.BackColor = Color.White;
                        e.CellStyle.ForeColor = Color.FromArgb(44, 62, 80);
                    }
                }

                // Управление видимостью кнопки уведомления на основе MsgSend
                if (e.ColumnIndex == dgvProducts.Columns["NotifyColumn"].Index)
                {
                    var cell = dgvProducts.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (product.MsgSend && product.Quantity < 10)
                    {
                        // Показываем кнопку для товаров с MsgSend=true и малым количеством
                        cell.Style.BackColor = Color.FromArgb(255, 193, 7); // Желтый цвет
                        cell.Style.ForeColor = Color.FromArgb(33, 37, 41);
                        cell.Value = "📧 Уведомить";
                    }
                    else
                    {
                        // Скрываем кнопку для остальных товаров
                        cell.Style.BackColor = Color.LightGray;
                        cell.Style.ForeColor = Color.Gray;
                        cell.Value = "-";
                    }
                }
            }
        }

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dgvProducts.Columns["IncreaseColumn"].Index)
            {
                ChangeQuantity(e.RowIndex, 1);
            }
            else if (e.ColumnIndex == dgvProducts.Columns["DecreaseColumn"].Index)
            {
                ChangeQuantity(e.RowIndex, -1);
            }
            else if (e.ColumnIndex == dgvProducts.Columns["NotifyColumn"].Index)
            {
                var product = dgvProducts.Rows[e.RowIndex].DataBoundItem as Product;
                if (product != null && product.MsgSend && product.Quantity < 10)
                {
                    SendLowStockNotification(e.RowIndex);
                }
                else
                {
                    MessageBox.Show("Уведомления доступны только для товаров с малыми запасами (меньше 10) и включенной отправкой уведомлений.", 
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvProducts.SelectedRows.Count > 0;
            btnEditProduct.Enabled = hasSelection;
            btnDeleteProduct.Enabled = hasSelection;
            btnAssignGroup.Enabled = hasSelection;
        }

        private void ChangeQuantity(int rowIndex, int change)
        {
            try
            {
                var product = dgvProducts.Rows[rowIndex].DataBoundItem as Product;
                if (product != null)
                {
                    int newQuantity = product.Quantity + change;
                    if (newQuantity < 0)
                    {
                        MessageBox.Show("Количество не может быть отрицательным", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (ProductRepository.UpdateProductQuantity(product.Id, newQuantity))
                    {
                        product.Quantity = newQuantity;
                        dgvProducts.InvalidateRow(rowIndex);
                        UpdateStatistics();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении количества: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendLowStockNotification(int rowIndex)
        {
            try
            {
                var product = dgvProducts.Rows[rowIndex].DataBoundItem as Product;
                if (product != null && product.MsgSend && product.Quantity < 10)
                {
                    // Показываем диалог для ввода email адреса
                    string email = ShowEmailInputDialog($"Введите email для отправки уведомления о малых запасах товара '{product.Name}':");

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        // Создаем сообщение уведомления
                        string subject = $"Уведомление о малых запасах: {product.Name}";
                        string message = $@"Уведомление о малых запасах

Товар: {product.Name}
Объем: {product.Volume}
Текущий остаток: {product.Quantity}
Статус: {product.Status}
Группа: {product.Group}

Необходимо пополнить запасы данного товара.

Сообщение отправлено из системы управления складом.";

                        // Здесь можно добавить реальную отправку email через SMTP или API
                        // Пока показываем уведомление об успешной "отправке"
                        MessageBox.Show($@"Уведомление подготовлено к отправке на {email}

{subject}

{message}", 
                            "Уведомление готово", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Для данного товара уведомления отключены или количество не требует уведомления", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке уведомления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnManageGroups_Click(object sender, EventArgs e)
        {
            // Open group management dialog
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var selectedProduct = dgvProducts.SelectedRows[0].DataBoundItem as Product;
                if (selectedProduct != null)
                {
                    using (var groupForm = new GroupManagementForm(selectedProduct))
                    {
                        if (groupForm.ShowDialog() == DialogResult.OK)
                        {
                            // Refresh data after group management
                            LoadProductsAndGroups();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для управления группой", "Информация", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefreshGroups_Click(object sender, EventArgs e)
        {
            LoadProductsAndGroups();
            MessageBox.Show("Данные обновлены", "Информация", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        }

        private void ShowProductForm(Product product)
        {
            ProductForm form = new ProductForm(product);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadProductsAndGroups();
            }
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var product = dgvProducts.SelectedRows[0].DataBoundItem as Product;
                if (MessageBox.Show($"Удалить товар '{product.Name}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (ProductRepository.DeleteProduct(product.Id))
                    {
                        LoadProductsAndGroups();
                    }
                }
            }
        }

        private void BtnAssignGroup_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                var product = dgvProducts.SelectedRows[0].DataBoundItem as Product;
                GroupManagementForm groupForm = new GroupManagementForm(product);
                if (groupForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProductsAndGroups();
                }
            }
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем данные для экспорта
                var products = (List<Product>)dgvProducts.DataSource;
                
                if (products == null || products.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Диалог сохранения файла
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV файлы (*.csv)|*.csv|Excel файлы (*.xlsx)|*.xlsx";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.FileName = $"warehouse_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCSV(products, saveFileDialog.FileName);
                    
                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveFileDialog.FileName}", 
                        "Экспорт завершен",
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(List<Product> products, string filePath)
        {
            using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Заголовки столбцов
                writer.WriteLine("ID,Наименование,Объем,Количество,Статус,Группа,Тип,Учитывать в итогах,Дополнительная информация,Отправлять уведомления");

                // Данные
                foreach (var product in products)
                {
                    var line = $"{EscapeCSV(product.Id.ToString())}," +
                              $"{EscapeCSV(product.Name)}," +
                              $"{EscapeCSV(product.Volume)}," +
                              $"{EscapeCSV(product.Quantity.ToString())}," +
                              $"{EscapeCSV(product.Status)}," +
                              $"{EscapeCSV(product.Group)}," +
                              $"{EscapeCSV(product.Type)}," +
                              $"{EscapeCSV(product.CountInTotal ? "Да" : "Нет")}," +
                              $"{EscapeCSV(product.AdditionalInfo)}," +
                              $"{EscapeCSV(product.MsgSend ? "Да" : "Нет")}";
                    
                    writer.WriteLine(line);
                }
            }
        }

        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            // Экранируем кавычки и переносы строк
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }

        private string ShowEmailInputDialog(string prompt)
        {
            Form inputForm = new Form()
            {
                Text = "Отправка уведомления",
                Width = 400,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new Label()
            {
                Text = prompt,
                Location = new Point(12, 15),
                Size = new Size(360, 40),
                Font = new Font("Segoe UI", 9)
            };

            TextBox textBox = new TextBox()
            {
                Location = new Point(12, 55),
                Size = new Size(360, 20),
                Font = new Font("Segoe UI", 9)
            };

            Button okButton = new Button()
            {
                Text = "OK",
                Location = new Point(217, 85),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new Button()
            {
                Text = "Отмена",
                Location = new Point(297, 85),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };

            inputForm.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }

        /// <summary>
        /// Обработчик кнопки управления типами
        /// </summary>
        private void BtnManageTypes_Click(object sender, EventArgs e)
        {
            ShowTypeManagementDialog();
        }

        /// <summary>
        /// Показывает диалог управления типами
        /// </summary>
        private void ShowTypeManagementDialog()
        {
            Form typeForm = new Form()
            {
                Text = "Управление типами товаров",
                Width = 600,
                Height = 500,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // ListBox для отображения типов
            ListBox lstTypes = new ListBox()
            {
                Location = new Point(20, 20),
                Size = new Size(400, 300),
                Font = new Font("Segoe UI", 10)
            };

            // Кнопки управления
            Button btnAddType = new Button()
            {
                Text = "➕ Добавить",
                Location = new Point(440, 20),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnEditType = new Button()
            {
                Text = "✏️ Изменить",
                Location = new Point(440, 60),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnDeleteType = new Button()
            {
                Text = "🗑️ Удалить",
                Location = new Point(440, 100),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnRefresh = new Button()
            {
                Text = "🔄 Обновить",
                Location = new Point(440, 140),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnClose = new Button()
            {
                Text = "Закрыть",
                Location = new Point(440, 390),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            // Загрузка типов
            Action loadTypes = () =>
            {
                try
                {
                    var types = ProductRepository.GetEditableStorageTypes();
                    lstTypes.Items.Clear();
                    foreach (var type in types)
                    {
                        lstTypes.Items.Add(type.Value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке типов: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Локальная функция для ввода текста
            string LocalShowInputDialog(string prompt, string title, string defaultValue = "")
            {
                Form inputForm = new Form()
                {
                    Text = title,
                    Width = 400,
                    Height = 160,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                Label label = new Label()
                {
                    Text = prompt,
                    Location = new Point(12, 15),
                    Size = new Size(360, 40),
                    Font = new Font("Segoe UI", 9)
                };

                TextBox textBox = new TextBox()
                {
                    Location = new Point(12, 55),
                    Size = new Size(360, 20),
                    Font = new Font("Segoe UI", 9),
                    Text = defaultValue
                };

                Button okButton = new Button()
                {
                    Text = "OK",
                    Location = new Point(217, 85),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button()
                {
                    Text = "Отмена",
                    Location = new Point(297, 85),
                    Size = new Size(75, 23),
                    DialogResult = DialogResult.Cancel
                };

                inputForm.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
                inputForm.AcceptButton = okButton;
                inputForm.CancelButton = cancelButton;

                return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
            }

            // Обработчики событий
            btnAddType.Click += (s, e) =>
            {
                string typeName = LocalShowInputDialog("Введите название нового типа:", "Добавление типа");
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    if (ProductRepository.AddStorageType(typeName.Trim()))
                    {
                        loadTypes();
                        MessageBox.Show("Тип успешно добавлен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            btnEditType.Click += (s, e) =>
            {
                if (lstTypes.SelectedItem != null)
                {
                    string oldTypeName = lstTypes.SelectedItem.ToString();
                    string newTypeName = LocalShowInputDialog($"Изменить название типа '{oldTypeName}':", "Редактирование типа", oldTypeName);
                    
                    if (!string.IsNullOrWhiteSpace(newTypeName) && newTypeName != oldTypeName)
                    {
                        if (ProductRepository.UpdateStorageType(oldTypeName, newTypeName.Trim()))
                        {
                            loadTypes();
                            MessageBox.Show("Тип успешно изменен!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите тип для редактирования", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnDeleteType.Click += (s, e) =>
            {
                if (lstTypes.SelectedItem != null)
                {
                    string typeName = lstTypes.SelectedItem.ToString();
                    var result = MessageBox.Show($"Удалить тип '{typeName}'?\n\nВнимание: Тип будет деактивирован, но связанные товары останутся.", 
                        "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        if (ProductRepository.RemoveStorageType(typeName))
                        {
                            loadTypes();
                            MessageBox.Show("Тип успешно удален!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите тип для удаления", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnRefresh.Click += (s, e) => loadTypes();

            typeForm.Controls.AddRange(new Control[] {
                lstTypes, btnAddType, btnEditType, btnDeleteType, btnRefresh, btnClose
            });

            typeForm.CancelButton = btnClose;

            // Загружаем типы при открытии формы
            loadTypes();

            typeForm.ShowDialog();
        }

        // Массовые операции
        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool isSelected = chkSelectAll.Checked;
                foreach (DataGridViewRow row in dgvProducts.Rows)
                {
                    if (row.Cells["Selected"] != null)
                        row.Cells["Selected"].Value = isSelected;
                }
                
                UpdateMassActionsVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе элементов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateMassActionsVisibility()
        {
            bool hasSelected = GetSelectedProducts().Count > 0;
            massActionsPanel.Visible = hasSelected;
        }

        private List<Product> GetSelectedProducts()
        {
            List<Product> selectedProducts = new List<Product>();
            
            try
            {
                foreach (DataGridViewRow row in dgvProducts.Rows)
                {
                    if (row.Cells["Selected"]?.Value is bool isSelected && isSelected)
                    {
                        if (row.DataBoundItem is Product product)
                            selectedProducts.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении выбранных товаров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return selectedProducts;
        }

        private void BtnMassChangeType_Click(object sender, EventArgs e)
        {
            var selectedProducts = GetSelectedProducts();
            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Выберите товары для изменения типа", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var types = ProductRepository.GetStorageOptions("type");
            var typeNames = types.Select(t => t.Value).ToList();
            typeNames.Insert(0, "");

            string selectedType = ShowComboDialog("Выберите новый тип для выделенных товаров:", 
                "Массовое изменение типа", typeNames);

            if (selectedType != null)
            {
                int successCount = 0;
                foreach (var product in selectedProducts)
                {
                    product.Type = selectedType;
                    if (ProductRepository.UpdateProduct(product))
                        successCount++;
                }

                MessageBox.Show($"Тип изменен у {successCount} из {selectedProducts.Count} товаров", 
                    "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadProductsAndGroups();
                RefreshProductsDisplay();
            }
        }

        private void BtnMassChangeStatus_Click(object sender, EventArgs e)
        {
            var selectedProducts = GetSelectedProducts();
            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Выберите товары для изменения статуса", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var statuses = ProductRepository.GetStorageOptions("status");
            var statusNames = statuses.Select(s => s.Value).ToList();

            string selectedStatus = ShowComboDialog("Выберите новый статус для выделенных товаров:", 
                "Массовое изменение статуса", statusNames);

            if (selectedStatus != null)
            {
                int successCount = 0;
                foreach (var product in selectedProducts)
                {
                    product.Status = selectedStatus;
                    if (ProductRepository.UpdateProduct(product))
                        successCount++;
                }

                MessageBox.Show($"Статус изменен у {successCount} из {selectedProducts.Count} товаров", 
                    "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadProductsAndGroups();
                RefreshProductsDisplay();
            }
        }

        private void BtnMassDelete_Click(object sender, EventArgs e)
        {
            var selectedProducts = GetSelectedProducts();
            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Выберите товары для удаления", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить {selectedProducts.Count} товаров?\n\nЭто действие нельзя отменить!", 
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int successCount = 0;
                foreach (var product in selectedProducts)
                {
                    if (ProductRepository.DeleteProduct(product.Id))
                        successCount++;
                }

                MessageBox.Show($"Удалено {successCount} из {selectedProducts.Count} товаров", 
                    "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadProductsAndGroups();
                RefreshProductsDisplay();
                massActionsPanel.Visible = false;
            }
        }

        private string ShowComboDialog(string prompt, string title, List<string> items)
        {
            Form dialog = new Form()
            {
                Text = title,
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new Label()
            {
                Text = prompt,
                Location = new Point(12, 15),
                Size = new Size(360, 40),
                Font = new Font("Segoe UI", 9)
            };

            ComboBox comboBox = new ComboBox()
            {
                Location = new Point(12, 45),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (string item in items)
                comboBox.Items.Add(item);

            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;

            Button okButton = new Button()
            {
                Text = "OK",
                Location = new Point(217, 80),
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new Button()
            {
                Text = "Отмена",
                Location = new Point(297, 80),
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };

            dialog.Controls.AddRange(new Control[] { label, comboBox, okButton, cancelButton });
            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            return dialog.ShowDialog() == DialogResult.OK ? comboBox.SelectedItem?.ToString() : null;
        }

        // Обработчики для checkbox выбора
        private void DgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvProducts.Columns["Selected"].Index && e.RowIndex >= 0)
            {
                dgvProducts.CommitEdit(DataGridViewDataErrorContexts.Commit);
                UpdateMassActionsVisibility();
            }
        }

        private void DgvProducts_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvProducts.IsCurrentCellDirty && dgvProducts.CurrentCell.ColumnIndex == dgvProducts.Columns["Selected"].Index)
            {
                dgvProducts.CommitEdit(DataGridViewDataErrorContexts.Commit);
                UpdateMassActionsVisibility();
            }
        }

        public TabPage GetTabPage()
        {
            return tabPage;
        }
    }
}