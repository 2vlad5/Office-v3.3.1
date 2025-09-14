using System;
using System.Drawing;
using System.Windows.Forms;
using officeApp.DataAccess;
using officeApp.Models;

namespace officeApp.Forms
{
    public class ProductForm : Form
    {
        private Product _product;
        private ComboBox cmbName, cmbVolume, cmbStatus, cmbType;
        private NumericUpDown numQuantity;
        private Button btnSave, btnCancel;

        public ProductForm(Product product = null)
        {
            _product = product ?? new Product();
            InitializeForm();
            LoadOptions();
        }

        private void InitializeForm()
        {
            this.Size = new Size(400, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = _product.Id == 0 ? "Добавить продукт" : "Редактировать продукт";
            this.Padding = new Padding(10);

            // Наименование
            Label lblName = new Label()
            {
                Text = "Наименование:",
                Location = new Point(20, 20),
                Width = 100,
                Font = new Font("Segoe UI", 9)
            };

            cmbName = new ComboBox()
            {
                Location = new Point(130, 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            // Объем
            Label lblVolume = new Label()
            {
                Text = "Объем:",
                Location = new Point(20, 60),
                Width = 100,
                Font = new Font("Segoe UI", 9)
            };

            cmbVolume = new ComboBox()
            {
                Location = new Point(130, 60),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            // Количество
            Label lblQuantity = new Label()
            {
                Text = "Количество:",
                Location = new Point(20, 100),
                Width = 100,
                Font = new Font("Segoe UI", 9)
            };

            numQuantity = new NumericUpDown()
            {
                Location = new Point(130, 100),
                Width = 100,
                Minimum = 0,
                Maximum = 10000,
                Font = new Font("Segoe UI", 9)
            };

            // Тип товара
            Label lblType = new Label()
            {
                Text = "Тип товара:",
                Location = new Point(20, 140),
                Width = 100,
                Font = new Font("Segoe UI", 9)
            };

            cmbType = new ComboBox()
            {
                Location = new Point(130, 140),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 9)
            };

            // Статус
            Label lblStatus = new Label()
            {
                Text = "Статус:",
                Location = new Point(20, 180),
                Width = 100,
                Font = new Font("Segoe UI", 9)
            };

            cmbStatus = new ComboBox()
            {
                Location = new Point(130, 180),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            // Кнопки
            btnSave = new Button()
            {
                Text = "Сохранить",
                Location = new Point(130, 230),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9),
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button()
            {
                Text = "Отмена",
                Location = new Point(220, 230),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 9),
                DialogResult = DialogResult.Cancel
            };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] {
                lblName, cmbName, lblVolume, cmbVolume, lblQuantity, numQuantity,
                lblType, cmbType, lblStatus, cmbStatus, btnSave, btnCancel
            });

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadOptions()
        {
            var names = ProductRepository.GetStorageOptions("name");
            var volumes = ProductRepository.GetStorageOptions("volume");
            var statuses = ProductRepository.GetStorageOptions("status");
            var types = ProductRepository.GetStorageOptions("type");

            cmbName.Items.Clear();
            cmbVolume.Items.Clear();
            cmbStatus.Items.Clear();
            cmbType.Items.Clear();

            foreach (var option in names)
                cmbName.Items.Add(option.Value);

            foreach (var option in volumes)
                cmbVolume.Items.Add(option.Value);

            foreach (var option in statuses)
                cmbStatus.Items.Add(option.Value);

            foreach (var option in types)
                cmbType.Items.Add(option.Value);

            // Устанавливаем текущие значения если редактируем
            if (_product.Id != 0)
            {
                cmbName.SelectedItem = _product.Name;
                cmbVolume.SelectedItem = _product.Volume;
                cmbStatus.SelectedItem = _product.Status;
                cmbType.Text = _product.Type; // Используем Text для DropDown стиля
                numQuantity.Value = _product.Quantity;
            }
            else
            {
                cmbName.SelectedIndex = cmbName.Items.Count > 0 ? 0 : -1;
                cmbVolume.SelectedIndex = cmbVolume.Items.Count > 0 ? 0 : -1;
                cmbStatus.SelectedIndex = cmbStatus.Items.Count > 0 ? 0 : -1;
                cmbType.SelectedIndex = -1; // Оставляем пустым для новых товаров
                numQuantity.Value = 0;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbName.SelectedItem == null || cmbVolume.SelectedItem == null || cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Заполните обязательные поля: наименование, объем и статус", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _product.Name = cmbName.SelectedItem.ToString();
            _product.Volume = cmbVolume.SelectedItem.ToString();
            _product.Quantity = (int)numQuantity.Value;
            _product.Status = cmbStatus.SelectedItem.ToString();
            _product.Type = string.IsNullOrWhiteSpace(cmbType.Text) ? "" : cmbType.Text.Trim();

            bool success;
            if (_product.Id == 0)
                success = ProductRepository.AddProduct(_product);
            else
                success = ProductRepository.UpdateProduct(_product);

            if (success)
            {
                // Если это новый тип, добавляем его в справочник типов для будущего использования
                if (!string.IsNullOrWhiteSpace(_product.Type) && !IsTypeInOptions(_product.Type))
                {
                    ProductRepository.AddStorageType(_product.Type);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Проверяет, есть ли тип в списке опций
        /// </summary>
        private bool IsTypeInOptions(string typeName)
        {
            for (int i = 0; i < cmbType.Items.Count; i++)
            {
                if (cmbType.Items[i].ToString().Equals(typeName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}