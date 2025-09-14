using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using officeApp.DataAccess;
using officeApp.Models;

namespace officeApp.Forms
{
    public partial class GroupManagementForm : Form
    {
        private Product _selectedProduct;
        private List<Product> _groupProducts;
        private ComboBox cmbGroups;
        private CheckBox chkCountTogether;
        private ListBox lstGroupProducts;

        public GroupManagementForm(Product product)
        {
            _selectedProduct = product;
            InitializeComponent();
            LoadGroups();
            LoadGroupProducts();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 400);
            this.Text = "Управление группой";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9);

            // Группа
            Label lblGroup = new Label()
            {
                Text = "Группа:",
                Location = new Point(20, 20),
                Width = 100
            };

            cmbGroups = new ComboBox()
            {
                Location = new Point(120, 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Button btnCreateGroup = new Button()
            {
                Text = "Создать",
                Location = new Point(330, 20),
                Size = new Size(80, 23)
            };

            // Настройки группы
            chkCountTogether = new CheckBox()
            {
                Text = "Считать товары группы вместе",
                Location = new Point(20, 60),
                Width = 200,
                Checked = true
            };

            // Список товаров в группе
            Label lblProducts = new Label()
            {
                Text = "Товары в группе:",
                Location = new Point(20, 100),
                Width = 150
            };

            lstGroupProducts = new ListBox()
            {
                Location = new Point(20, 130),
                Size = new Size(400, 150)
            };

            // Кнопки
            Button btnAddToGroup = new Button()
            {
                Text = "Добавить в группу",
                Location = new Point(20, 300),
                Size = new Size(120, 30)
            };

            Button btnRemoveFromGroup = new Button()
            {
                Text = "Удалить из группы",
                Location = new Point(150, 300),
                Size = new Size(120, 30)
            };

            Button btnSave = new Button()
            {
                Text = "Сохранить",
                Location = new Point(300, 300),
                Size = new Size(80, 30)
            };

            Button btnCancel = new Button()
            {
                Text = "Отмена",
                Location = new Point(390, 300),
                Size = new Size(80, 30)
            };

            // Обработчики событий
            btnCreateGroup.Click += BtnCreateGroup_Click;
            btnAddToGroup.Click += BtnAddToGroup_Click;
            btnRemoveFromGroup.Click += BtnRemoveFromGroup_Click;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
            cmbGroups.SelectedIndexChanged += CmbGroups_SelectedIndexChanged;

            this.Controls.AddRange(new Control[] {
                lblGroup, cmbGroups, btnCreateGroup, chkCountTogether,
                lblProducts, lstGroupProducts, btnAddToGroup, btnRemoveFromGroup,
                btnSave, btnCancel
            });
        }

        private void BtnCreateGroup_Click(object sender, EventArgs e)
        {
            // Простая форма для ввода названия группы
            using (var inputForm = new Form())
            {
                inputForm.Text = "Создание группы";
                inputForm.Size = new Size(300, 150);
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.StartPosition = FormStartPosition.CenterParent;

                Label lbl = new Label() { Text = "Введите название группы:", Location = new Point(20, 20), Width = 200 };
                TextBox txt = new TextBox() { Location = new Point(20, 50), Width = 200 };
                Button btnOk = new Button() { Text = "OK", Location = new Point(20, 80), DialogResult = DialogResult.OK };
                Button btnCancel = new Button() { Text = "Отмена", Location = new Point(120, 80), DialogResult = DialogResult.Cancel };

                inputForm.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                inputForm.AcceptButton = btnOk;
                inputForm.CancelButton = btnCancel;

                if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(txt.Text))
                {
                    cmbGroups.Items.Add(txt.Text);
                    cmbGroups.SelectedItem = txt.Text;
                }
            }
        }

        private void BtnAddToGroup_Click(object sender, EventArgs e)
        {
            if (cmbGroups.SelectedItem != null)
            {
                string groupName = cmbGroups.SelectedItem.ToString();
                if (ProductRepository.UpdateProductGroup(_selectedProduct.Id, groupName, chkCountTogether.Checked))
                {
                    _selectedProduct.Group = groupName;
                    _selectedProduct.CountInTotal = chkCountTogether.Checked;
                    LoadGroupProducts(groupName);
                    MessageBox.Show("Товар добавлен в группу", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите или создайте группу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRemoveFromGroup_Click(object sender, EventArgs e)
        {
            if (lstGroupProducts.SelectedIndex >= 0 && _groupProducts != null && lstGroupProducts.SelectedIndex < _groupProducts.Count)
            {
                Product selectedProduct = _groupProducts[lstGroupProducts.SelectedIndex];
                if (MessageBox.Show($"Удалить товар '{selectedProduct.Name}' из группы?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (ProductRepository.UpdateProductGroup(selectedProduct.Id, "", true))
                    {
                        LoadGroupProducts(cmbGroups.SelectedItem?.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления из группы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CmbGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGroups.SelectedItem != null)
            {
                LoadGroupProducts(cmbGroups.SelectedItem.ToString());
            }
        }

        private void LoadGroups()
        {
            cmbGroups.Items.Clear();

            List<string> groups = ProductRepository.GetProductGroups();
            foreach (string group in groups)
            {
                cmbGroups.Items.Add(group);
            }

            // Выбираем группу текущего продукта
            if (!string.IsNullOrEmpty(_selectedProduct.Group) && cmbGroups.Items.Contains(_selectedProduct.Group))
            {
                cmbGroups.SelectedItem = _selectedProduct.Group;
            }
            else if (cmbGroups.Items.Count > 0)
            {
                cmbGroups.SelectedIndex = 0;
            }
        }

        private void LoadGroupProducts(string groupName = null)
        {
            groupName = groupName ?? _selectedProduct.Group;
            if (string.IsNullOrEmpty(groupName)) return;

            lstGroupProducts.Items.Clear();

            _groupProducts = ProductRepository.GetProductsByGroup(groupName);
            foreach (Product product in _groupProducts)
            {
                lstGroupProducts.Items.Add($"{product.Name} ({product.Volume}) - {product.Quantity} шт. {(product.CountInTotal ? "" : "[не считать]")}");
            }

            // Показываем настройки группы
            if (_groupProducts.Count > 0)
            {
                chkCountTogether.Checked = _groupProducts[0].CountInTotal;
            }
        }
    }
}