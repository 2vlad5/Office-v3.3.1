using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using officeApp.Models;

namespace officeApp.DataAccess
{
    public static class ProductRepository
    {
        public static List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    // Исправленный запрос для правильного получения типов товаров
                    string query = @"SELECT os.id, os.name, os.volume, os.quantity, os.status, 
                           COALESCE(os.group_name, '') as group_name,
                           COALESCE(os.count_in_total, 1) as count_in_total,
                           COALESCE(os.additional_info, '') as additional_info,
                           COALESCE(os.msg_send, 0) as msg_send,
                           COALESCE(os.product_type, '') as product_type 
                           FROM office_storage os
                           ORDER BY os.name";

                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Volume = reader.GetString("volume"),
                                Quantity = reader.GetInt32("quantity"),
                                Status = reader.GetString("status"),
                                Group = reader.GetString("group_name"), // Убедитесь что поле есть
                                CountInTotal = reader.GetBoolean("count_in_total"),
                                AdditionalInfo = reader.GetString("additional_info"),
                                Type = reader.GetString("product_type"),
                                MsgSend = reader.GetBoolean("msg_send")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке продуктов: {ex.Message}");
            }

            return products;
        }

        public static List<StorageOption> GetStorageOptions(string optionType)
        {
            List<StorageOption> options = new List<StorageOption>();

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    // Фильтруем только активные опции (enable = 1)
                    string query = "SELECT value FROM office_storage_options WHERE option_type = @OptionType AND `enable` = 1 ORDER BY value";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@OptionType", optionType);

                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            options.Add(new StorageOption
                            {
                                OptionType = optionType,
                                Value = reader.GetString("value")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке опций: {ex.Message}");
            }

            return options;
        }

        public static bool UpdateProductQuantity(int productId, int newQuantity)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "UPDATE office_storage SET quantity = @Quantity WHERE id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                    cmd.Parameters.AddWithValue("@Id", productId);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении количества: {ex.Message}");
                return false;
            }
        }

        public static bool AddProduct(Product product)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"INSERT INTO office_storage 
                                   (name, volume, quantity, status, group_name, product_type, count_in_total, additional_info, msg_send) 
                                   VALUES (@Name, @Volume, @Quantity, @Status, @Group, @ProductType, @CountInTotal, @AdditionalInfo, @MsgSend)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Name", product.Name);
                    cmd.Parameters.AddWithValue("@Volume", product.Volume);
                    cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                    cmd.Parameters.AddWithValue("@Status", product.Status);
                    cmd.Parameters.AddWithValue("@Group", string.IsNullOrEmpty(product.Group) ? (object)DBNull.Value : product.Group);
                    cmd.Parameters.AddWithValue("@ProductType", string.IsNullOrEmpty(product.Type) ? (object)DBNull.Value : product.Type);
                    cmd.Parameters.AddWithValue("@CountInTotal", product.CountInTotal);
                    cmd.Parameters.AddWithValue("@AdditionalInfo", string.IsNullOrEmpty(product.AdditionalInfo) ? (object)DBNull.Value : product.AdditionalInfo);
                    cmd.Parameters.AddWithValue("@MsgSend", product.MsgSend);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении продукта: {ex.Message}");
                return false;
            }
        }

        public static bool UpdateProduct(Product product)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"UPDATE office_storage SET 
                                    name = @Name, 
                                    volume = @Volume, 
                                    quantity = @Quantity, 
                                    status = @Status,
                                    group_name = @Group,
                                    product_type = @ProductType,
                                    count_in_total = @CountInTotal,
                                    additional_info = @AdditionalInfo,
                                    msg_send = @MsgSend
                                    WHERE id = @Id";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Id", product.Id);
                    cmd.Parameters.AddWithValue("@Name", product.Name);
                    cmd.Parameters.AddWithValue("@Volume", product.Volume);
                    cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                    cmd.Parameters.AddWithValue("@Status", product.Status);
                    cmd.Parameters.AddWithValue("@Group", string.IsNullOrEmpty(product.Group) ? (object)DBNull.Value : product.Group);
                    cmd.Parameters.AddWithValue("@ProductType", string.IsNullOrEmpty(product.Type) ? (object)DBNull.Value : product.Type);
                    cmd.Parameters.AddWithValue("@CountInTotal", product.CountInTotal);
                    cmd.Parameters.AddWithValue("@AdditionalInfo", string.IsNullOrEmpty(product.AdditionalInfo) ? (object)DBNull.Value : product.AdditionalInfo);
                    cmd.Parameters.AddWithValue("@MsgSend", product.MsgSend);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении продукта: {ex.Message}");
                return false;
            }
        }

        public static bool DeleteProduct(int productId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "DELETE FROM office_storage WHERE id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Id", productId);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении продукта: {ex.Message}");
                return false;
            }
        }

        public static bool UpdateProductGroup(int productId, string groupName, bool countInTotal)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "UPDATE office_storage SET group_name = @GroupName, count_in_total = @CountInTotal WHERE id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@GroupName", groupName);
                    cmd.Parameters.AddWithValue("@CountInTotal", countInTotal);
                    cmd.Parameters.AddWithValue("@Id", productId);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении группы: {ex.Message}");
                return false;
            }
        }

        public static List<Product> GetProductsByGroup(string groupName)
        {
            List<Product> products = new List<Product>();

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT os.*, 
                                    COALESCE(opt.value, '') as product_type
                                    FROM office_storage os
                                    LEFT JOIN office_storage_options opt ON os.name = opt.value 
                                    AND opt.option_type = 'type' AND opt.enable = 1
                                    WHERE os.group_name = @GroupName ORDER BY os.name";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@GroupName", groupName);

                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Volume = reader.GetString("volume"),
                                Quantity = reader.GetInt32("quantity"),
                                Status = reader.GetString("status"),
                                Group = reader.IsDBNull(reader.GetOrdinal("group_name")) ? "" : reader.GetString("group_name"),
                                CountInTotal = reader.IsDBNull(reader.GetOrdinal("count_in_total")) ? true : reader.GetBoolean("count_in_total"),
                                AdditionalInfo = reader.IsDBNull(reader.GetOrdinal("additional_info")) ? "" : reader.GetString("additional_info"),
                                Type = reader.IsDBNull(reader.GetOrdinal("product_type")) ? "" : reader.GetString("product_type"),
                                MsgSend = reader.IsDBNull(reader.GetOrdinal("msg_send")) ? false : reader.GetBoolean("msg_send")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке продуктов группы: {ex.Message}");
            }

            return products;
        }

        public static List<string> GetProductGroups()
        {
            List<string> groups = new List<string>();

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    // Получаем группы из office_storage_options с фильтром enable=1
                    string query = "SELECT value FROM office_storage_options WHERE option_type = 'group' AND `enable` = 1 ORDER BY value";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groups.Add(reader.GetString("value"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке групп: {ex.Message}");
            }

            return groups;
        }

        public static string GetProductType(int productId)
        {
            string productType = "";

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT COALESCE(opt.value, '') as product_type 
                                   FROM office_storage os 
                                   LEFT JOIN office_storage_options opt ON os.name = opt.value 
                                   WHERE os.id = @ProductId 
                                   AND (opt.option_type = 'type' AND opt.enable = 1 OR opt.option_type IS NULL)";
                    
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    connection.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        productType = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении типа продукта: {ex.Message}");
            }

            return productType;
        }

        public static bool IsProductTypeObraz(int productId)
        {
            string productType = GetProductType(productId);
            return productType.Equals("Образ", StringComparison.OrdinalIgnoreCase);
        }

        public static bool AddGroupToOptions(string groupName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    // Проверяем, существует ли уже такая группа
                    string checkQuery = "SELECT COUNT(*) FROM office_storage_options WHERE option_type = 'group' AND value = @GroupName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@GroupName", groupName);

                    connection.Open();
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (exists > 0)
                    {
                        // Если группа уже существует, активируем её
                        string updateQuery = "UPDATE office_storage_options SET `enable` = 1 WHERE option_type = 'group' AND value = @GroupName";
                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@GroupName", groupName);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // Если группа не существует, добавляем новую
                        string insertQuery = "INSERT INTO office_storage_options (option_type, value, `enable`) VALUES ('group', @GroupName, 1)";
                        MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@GroupName", groupName);
                        insertCmd.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении группы: {ex.Message}");
                return false;
            }
        }

        public static bool RemoveGroupFromOptions(string groupName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "UPDATE office_storage_options SET `enable` = 0 WHERE option_type = 'group' AND value = @GroupName";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@GroupName", groupName);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении группы: {ex.Message}");
                return false;
            }
        }

        // МЕТОДЫ ДЛЯ УПРАВЛЕНИЯ ОБНОВЛЕНИЯМИ
        
        /// <summary>
        /// Получает текущую версию приложения из базы данных
        /// </summary>
        public static string GetAppVersionFromDatabase()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT setting_value FROM office_app_settings WHERE setting_key = 'app_version' LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    connection.Open();
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "1.0.0.0";
                }
            }
            catch (Exception)
            {
                // Если таблица не существует или возникла ошибка, возвращаем версию по умолчанию
                return "1.0.0.0";
            }
        }

        /// <summary>
        /// Обновляет версию приложения в базе данных
        /// </summary>
        public static bool UpdateAppVersionInDatabase(string newVersion)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"INSERT INTO office_app_settings (setting_key, setting_value) 
                                   VALUES ('app_version', @Version) 
                                   ON DUPLICATE KEY UPDATE setting_value = @Version";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Version", newVersion);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении версии: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получает текущую версию приложения из AssemblyInfo
        /// </summary>
        public static string GetCurrentAppVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
        }

        // МЕТОДЫ ДЛЯ РЕДАКТИРОВАНИЯ ТИПОВ В СКЛАДЕ

        /// <summary>
        /// Получает все доступные типы для редактирования в складе
        /// </summary>
        public static List<StorageOption> GetEditableStorageTypes()
        {
            List<StorageOption> types = new List<StorageOption>();

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT value FROM office_storage_options WHERE option_type = 'type' AND `enable` = 1 ORDER BY value";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            types.Add(new StorageOption
                            {
                                OptionType = "type",
                                Value = reader.GetString("value")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке типов: {ex.Message}");
            }

            return types;
        }

        /// <summary>
        /// Добавляет новый тип в систему
        /// </summary>
        public static bool AddStorageType(string typeName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    // Проверяем, не существует ли уже такой тип
                    string checkQuery = "SELECT COUNT(*) FROM office_storage_options WHERE option_type = 'type' AND value = @TypeName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@TypeName", typeName);

                    connection.Open();
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Тип с таким названием уже существует");
                        return false;
                    }

                    // Добавляем новый тип
                    string insertQuery = "INSERT INTO office_storage_options (option_type, value, `enable`) VALUES ('type', @TypeName, 1)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@TypeName", typeName);

                    int rowsAffected = insertCmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении типа: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Изменяет название типа
        /// </summary>
        public static bool UpdateStorageType(string oldTypeName, string newTypeName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Обновляем тип в справочнике опций
                            string updateOptionsQuery = "UPDATE office_storage_options SET value = @NewTypeName WHERE option_type = 'type' AND value = @OldTypeName";
                            MySqlCommand updateOptionsCmd = new MySqlCommand(updateOptionsQuery, connection, transaction);
                            updateOptionsCmd.Parameters.AddWithValue("@NewTypeName", newTypeName);
                            updateOptionsCmd.Parameters.AddWithValue("@OldTypeName", oldTypeName);
                            updateOptionsCmd.ExecuteNonQuery();

                            // Обновляем тип у всех товаров, которые используют этот тип
                            string updateProductsQuery = "UPDATE office_storage SET product_type = @NewTypeName WHERE product_type = @OldTypeName";
                            MySqlCommand updateProductsCmd = new MySqlCommand(updateProductsQuery, connection, transaction);
                            updateProductsCmd.Parameters.AddWithValue("@NewTypeName", newTypeName);
                            updateProductsCmd.Parameters.AddWithValue("@OldTypeName", oldTypeName);
                            updateProductsCmd.ExecuteNonQuery();

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении типа: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Удаляет тип (делает неактивным)
        /// </summary>
        public static bool RemoveStorageType(string typeName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Удаляем тип из справочника
                            string disableTypeQuery = "UPDATE office_storage_options SET `enable` = 0 WHERE option_type = 'type' AND value = @TypeName";
                            MySqlCommand disableTypeCmd = new MySqlCommand(disableTypeQuery, connection, transaction);
                            disableTypeCmd.Parameters.AddWithValue("@TypeName", typeName);
                            disableTypeCmd.ExecuteNonQuery();

                            // Очищаем тип у товаров, которые его использовали (опционально)
                            string clearProductTypesQuery = "UPDATE office_storage SET product_type = '' WHERE product_type = @TypeName";
                            MySqlCommand clearProductTypesCmd = new MySqlCommand(clearProductTypesQuery, connection, transaction);
                            clearProductTypesCmd.Parameters.AddWithValue("@TypeName", typeName);
                            clearProductTypesCmd.ExecuteNonQuery();

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении типа: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Назначает тип товару
        /// </summary>
        public static bool AssignTypeToProduct(int productId, string typeName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "UPDATE office_storage SET product_type = @TypeName WHERE id = @ProductId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@TypeName", typeName);
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при назначении типа товару: {ex.Message}");
                return false;
            }
        }
    }
}
