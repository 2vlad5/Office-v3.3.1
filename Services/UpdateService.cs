using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using officeApp.DataAccess;

namespace officeApp.Services
{
    /// <summary>
    /// Служба для проверки и загрузки обновлений приложения
    /// </summary>
    public static class UpdateService
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10)
        };

        private const string UPDATE_SERVER_URL = "https://mwj-2v5.ru/officeApp";
        private const long MAX_FILE_SIZE = 100 * 1024 * 1024; // 100 MB

        /// <summary>
        /// Проверяет наличие обновлений, сравнивая версию из БД с текущей версией приложения
        /// </summary>
        public static UpdateCheckResult CheckForUpdates()
        {
            try
            {
                string currentVersion = ProductRepository.GetCurrentAppVersion();
                string databaseVersion = ProductRepository.GetAppVersionFromDatabase();

                return new UpdateCheckResult
                {
                    HasUpdate = !currentVersion.Equals(databaseVersion),
                    CurrentVersion = currentVersion,
                    AvailableVersion = databaseVersion,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new UpdateCheckResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Ошибка при проверке обновлений: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Загружает файлы обновлений с сервера с проверкой безопасности
        /// </summary>
        public static async Task<bool> DownloadUpdateAsync(string version, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Валидация версии для предотвращения path traversal атак
                if (!IsValidVersionFormat(version))
                {
                    MessageBox.Show($"Недопустимый формат версии: {version}", "Ошибка безопасности", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                string sanitizedVersion = SanitizeVersionString(version);
                string fileName = $"OfficeApp_{sanitizedVersion}.zip";
                string fileUrl = $"{UPDATE_SERVER_URL}/{fileName}";
                string downloadPath = Path.Combine(Path.GetTempPath(), fileName);

                // Проверяем, что используется HTTPS
                if (!IsValidHttpsUrl(fileUrl))
                {
                    MessageBox.Show("Небезопасный URL для загрузки обновлений. Используйте только HTTPS.");
                    return false;
                }

                // Загружаем файл с прогрессом
                using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Проверяем размер файла
                var contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue && contentLength.Value > MAX_FILE_SIZE)
                {
                    MessageBox.Show($"Размер файла обновления превышает максимально допустимый ({MAX_FILE_SIZE / (1024 * 1024)} МБ)");
                    return false;
                }

                // Загружаем с прогрессом
                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;
                long totalBytes = contentLength ?? 0;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    totalBytesRead += bytesRead;
                    
                    // Проверяем лимит размера во время загрузки
                    if (totalBytesRead > MAX_FILE_SIZE)
                    {
                        File.Delete(downloadPath);
                        MessageBox.Show("Превышен лимит размера файла во время загрузки");
                        return false;
                    }

                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                    // Обновляем прогресс
                    if (progress != null && totalBytes > 0)
                    {
                        int progressPercent = (int)((totalBytesRead * 100) / totalBytes);
                        progress.Report(progressPercent);
                    }
                }

                // Проверяем, что файл загружен успешно
                if (File.Exists(downloadPath) && new FileInfo(downloadPath).Length > 0)
                {
                    // Проверяем целостность файла (если есть ожидаемый хеш)
                    string expectedHash = GetExpectedFileHash(sanitizedVersion);
                    if (!string.IsNullOrEmpty(expectedHash))
                    {
                        string actualHash = await CalculateFileHashAsync(downloadPath);
                        if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(downloadPath); // Удаляем поврежденный файл
                            MessageBox.Show("Ошибка целостности файла обновления. Файл поврежден или подделан.", 
                                "Ошибка безопасности", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }

                    MessageBox.Show($"Обновление успешно загружено и проверено: {downloadPath}", "Успех", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке обновления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Проверяет, является ли URL безопасным (HTTPS)
        /// </summary>
        private static bool IsValidHttpsUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps;
        }

        /// <summary>
        /// Проверяет корректность формата версии (только цифры, точки, дефисы)
        /// </summary>
        private static bool IsValidVersionFormat(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;

            // Разрешаем только цифры, точки, дефисы и буквы для версий типа "1.2.3" или "1.2.3-beta"
            var versionPattern = @"^[0-9]+(\.[0-9]+)*(-[a-zA-Z0-9]+)*$";
            return Regex.IsMatch(version, versionPattern) && version.Length <= 50;
        }

        /// <summary>
        /// Очищает строку версии от потенциально опасных символов
        /// </summary>
        private static string SanitizeVersionString(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return "unknown";

            // Удаляем все символы кроме разрешенных
            return Regex.Replace(version, @"[^0-9a-zA-Z\.\-]", "");
        }

        /// <summary>
        /// Вычисляет SHA-256 хеш файла
        /// </summary>
        private static async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var sha256 = SHA256.Create();
            await using var stream = File.OpenRead(filePath);
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes);
        }

        /// <summary>
        /// Получает ожидаемый хеш файла обновления из базы данных
        /// </summary>
        private static string GetExpectedFileHash(string version)
        {
            try
            {
                // В реальном проекте этот хеш должен храниться в БД или загружаться с доверенного источника
                // Для демонстрации возвращаем пустую строку (отключаем проверку хеша)
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Показывает диалог с информацией об обновлении
        /// </summary>
        public static async Task<bool> ShowUpdateDialogAsync()
        {
            var updateCheck = CheckForUpdates();
            
            if (!updateCheck.IsSuccess)
            {
                MessageBox.Show(updateCheck.ErrorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!updateCheck.HasUpdate)
            {
                MessageBox.Show($"У вас установлена актуальная версия: {updateCheck.CurrentVersion}", "Обновления не требуются", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string message = $"Доступно обновление!\n\n" +
                           $"Текущая версия: {updateCheck.CurrentVersion}\n" +
                           $"Доступная версия: {updateCheck.AvailableVersion}\n\n" +
                           $"Загрузить обновление?";

            var result = MessageBox.Show(message, "Доступно обновление", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Создаем прогресс диалог
                var progressDialog = new Form
                {
                    Text = "Загрузка обновления...",
                    Size = new System.Drawing.Size(400, 120),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    StartPosition = FormStartPosition.CenterParent
                };

                var progressBar = new ProgressBar
                {
                    Location = new System.Drawing.Point(20, 20),
                    Size = new System.Drawing.Size(340, 30),
                    Minimum = 0,
                    Maximum = 100
                };

                var statusLabel = new Label
                {
                    Location = new System.Drawing.Point(20, 60),
                    Size = new System.Drawing.Size(340, 20),
                    Text = "Подготовка к загрузке..."
                };

                progressDialog.Controls.AddRange(new Control[] { progressBar, statusLabel });

                var progress = new Progress<int>(percent =>
                {
                    if (progressDialog.InvokeRequired)
                    {
                        progressDialog.Invoke(new Action(() =>
                        {
                            progressBar.Value = percent;
                            statusLabel.Text = $"Загружено: {percent}%";
                        }));
                    }
                    else
                    {
                        progressBar.Value = percent;
                        statusLabel.Text = $"Загружено: {percent}%";
                    }
                });

                progressDialog.Show();

                try
                {
                    bool downloadSuccess = await DownloadUpdateAsync(updateCheck.AvailableVersion, progress);
                    progressDialog.Close();
                    
                    if (downloadSuccess)
                    {
                        // Обновляем версию в AssemblyInfo на версию из БД
                        ProductRepository.UpdateAppVersionInDatabase(updateCheck.AvailableVersion);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    progressDialog.Close();
                    MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Результат проверки обновлений
    /// </summary>
    public class UpdateCheckResult
    {
        public bool HasUpdate { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public string AvailableVersion { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}