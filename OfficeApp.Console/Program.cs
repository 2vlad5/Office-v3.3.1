using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OfficeApp.Console
{
    class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Volume { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.WriteLine("=== Office App Console - Database Test ===");
            System.Console.WriteLine($"Starting at: {DateTime.Now}");
            System.Console.WriteLine();

            // Get connection string from environment - required for security
            string? connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                System.Console.WriteLine("❌ DATABASE_CONNECTION_STRING environment variable not set");
                System.Console.WriteLine("Please set the database connection string in environment variables for security.");
                Environment.Exit(1);
                return;
            }

            try
            {
                await TestDatabaseConnection(connectionString);
                await TestWarehouseFunctionality(connectionString);
                
                System.Console.WriteLine();
                System.Console.WriteLine("=== All tests completed successfully! ===");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Critical Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static async Task TestDatabaseConnection(string connectionString)
        {
            System.Console.WriteLine("🔍 Testing database connection...");
            
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new MySqlCommand("SELECT 1", connection);
            var result = await command.ExecuteScalarAsync();
            
            if (result?.ToString() == "1")
            {
                System.Console.WriteLine("✅ Database connection successful!");
                
                // Connection info (sensitive details masked)
                var builder = new MySqlConnectionStringBuilder(connectionString);
                System.Console.WriteLine($"   Server: {MaskSensitiveString(builder.Server)}");
                System.Console.WriteLine($"   Database: {builder.Database}");
                System.Console.WriteLine($"   Connection successful");
            }
            else
            {
                throw new Exception("Database connection test failed");
            }
        }

        static async Task TestWarehouseFunctionality(string connectionString)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("📦 Testing warehouse functionality...");
            
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // Check office_storage table structure
            await ExamineOfficeStorageTable(connection);
            
            // Check office_storage_options table structure  
            await ExamineOfficeStorageOptionsTable(connection);
            
            // Test office_storage data
            await TestOfficeStorageData(connection);
        }

        static async Task ExamineOfficeStorageTable(MySqlConnection connection)
        {
            System.Console.WriteLine("\n🗄️  Examining office_storage table structure:");
            
            try
            {
                var query = "SHOW COLUMNS FROM office_storage";
                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                System.Console.WriteLine("   Columns:");
                while (await reader.ReadAsync())
                {
                    var field = reader.GetString(0);
                    var type = reader.GetString(1);
                    var nullable = reader.GetString(2);
                    var key = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var defaultValue = reader.IsDBNull(4) ? "NULL" : reader.GetString(4);
                    
                    System.Console.WriteLine($"   • {field}: {type} (Null: {nullable}, Key: {key}, Default: {defaultValue})");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error examining office_storage: {ex.Message}");
            }
        }

        static async Task ExamineOfficeStorageOptionsTable(MySqlConnection connection)
        {
            System.Console.WriteLine("\n⚙️  Examining office_storage_options table structure:");
            
            try
            {
                var query = "SHOW COLUMNS FROM office_storage_options";
                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                System.Console.WriteLine("   Columns:");
                while (await reader.ReadAsync())
                {
                    var field = reader.GetString(0);
                    var type = reader.GetString(1);
                    var nullable = reader.GetString(2);
                    var key = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var defaultValue = reader.IsDBNull(4) ? "NULL" : reader.GetString(4);
                    
                    System.Console.WriteLine($"   • {field}: {type} (Null: {nullable}, Key: {key}, Default: {defaultValue})");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error examining office_storage_options: {ex.Message}");
            }
        }
        
        static async Task TestOfficeStorageData(MySqlConnection connection)
        {
            System.Console.WriteLine("\n📊 Testing office_storage data:");
            
            try
            {
                // Check basic data
                var countQuery = "SELECT COUNT(*) FROM office_storage";
                using var countCmd = new MySqlCommand(countQuery, connection);
                var count = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                System.Console.WriteLine($"   • Total items in storage: {count}");
                
                if (count > 0)
                {
                    // Sample some data
                    var sampleQuery = "SELECT id, name, volume, quantity, status, COALESCE(group_name, 'No Group') as group_name FROM office_storage LIMIT 3";
                    using var sampleCmd = new MySqlCommand(sampleQuery, connection);
                    using var reader = await sampleCmd.ExecuteReaderAsync();
                    
                    System.Console.WriteLine("   Sample items:");
                    while (await reader.ReadAsync())
                    {
                        System.Console.WriteLine($"   • ID: {reader["id"]}, Name: {reader["name"]}, Volume: {reader["volume"]}, Qty: {reader["quantity"]}, Status: {reader["status"]}, Group: {reader["group_name"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error testing office_storage data: {ex.Message}");
            }
            
            System.Console.WriteLine("\n🔧 Testing office_storage_options data:");
            
            try
            {
                // Check options data including enable column and new option types
                var optionsQuery = @"
                    SELECT option_type, 
                           COUNT(*) as total_count,
                           SUM(CASE WHEN `enable` = 1 THEN 1 ELSE 0 END) as enabled_count,
                           SUM(CASE WHEN `enable` = 0 THEN 1 ELSE 0 END) as disabled_count
                    FROM office_storage_options 
                    GROUP BY option_type";
                using var optionsCmd = new MySqlCommand(optionsQuery, connection);
                using var reader = await optionsCmd.ExecuteReaderAsync();
                
                System.Console.WriteLine("   Option types summary:");
                while (await reader.ReadAsync())
                {
                    System.Console.WriteLine($"   • {reader["option_type"]}: {reader["total_count"]} total ({reader["enabled_count"]} enabled, {reader["disabled_count"]} disabled)");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error testing office_storage_options data: {ex.Message}");
            }
            
            try
            {
                // Show samples of each option type
                var sampleQuery = "SELECT * FROM office_storage_options ORDER BY option_type, value LIMIT 10";
                using var sampleCmd = new MySqlCommand(sampleQuery, connection);
                using var reader = await sampleCmd.ExecuteReaderAsync();
                
                System.Console.WriteLine("   Sample options:");
                while (await reader.ReadAsync())
                {
                    var enable = "NULL";
                    try 
                    {
                        var enableIndex = -1;
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.GetName(i).Equals("enable", StringComparison.OrdinalIgnoreCase))
                            {
                                enableIndex = i;
                                break;
                            }
                        }
                        if (enableIndex >= 0 && !reader.IsDBNull(enableIndex))
                        {
                            enable = reader.GetInt32(enableIndex).ToString();
                        }
                    }
                    catch 
                    {
                        enable = "N/A";
                    }
                    System.Console.WriteLine($"   • Type: {reader["option_type"]}, Value: {reader["value"]}, Enabled: {enable}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"   ❌ Error showing sample options: {ex.Message}");
            }
        }
        static string MaskSensitiveString(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= 4)
                return "***";
            
            return input.Substring(0, 2) + new string('*', Math.Max(1, input.Length - 4)) + input.Substring(input.Length - 2);
        }
    }
}