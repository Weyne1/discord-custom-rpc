using System.IO;
using System.Windows;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Discord_Custom_RPC
{
    public partial class App : Application
    {
        readonly Logger logger = new();
        private Mutex? mutex;

        protected override void OnStartup(StartupEventArgs e)
        {

            const string mutexName = "Discord Custom RPC";
            bool createdNew;
            mutex = new(true, mutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Application is already running.", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
            }

            base.OnStartup(e);

            string appDataFolder = AppDataFolder.Path();
            string iconFilePath = Path.Combine(appDataFolder, "tray_icon.ico");

            // Копировать tray_icon.ico в AppData
            if (!File.Exists(iconFilePath)) 
            {
                string sourceIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tray_icon.ico");

                if (File.Exists(sourceIconPath))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.Combine(appDataFolder, "Discord Custom RPC"));
                        File.Copy(sourceIconPath, iconFilePath);
                        File.Delete(sourceIconPath);
                        logger.Log("Successful copying of tray icon in AppData folder");
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Error when copying tray icon in AppData folder: {ex.Message}");
                    }
                }
            }

            // Логгирование ошибок
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception exception = (Exception)args.ExceptionObject;
                logger.Log($"An error has occurred: {exception.Message}\n\nLocation: {exception.StackTrace}");
            };

            CreateJSON();
            DiscordConnect();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            mutex?.Dispose();
            base.OnExit(e);
        }

        private static void CreateJSON()
        {
            string appDataFolder = AppDataFolder.Path();
            // Полный путь к файлу lastConfig.json
            string jsonFilePath = Path.Combine(appDataFolder, "lastConfig.json");
            
            
            if (!Directory.Exists(appDataFolder))
                Directory.CreateDirectory(appDataFolder);

            if (!File.Exists(jsonFilePath))
            {

                // Объект, который будет сериализован в JSON
                LastConfigData configData = new()
                {
                    ApplicationId = "",
                    Details = "",
                    State = "",
                    Timestamp = "",
                    LargeImageKey = "",
                    LargeImageText = "",
                    SmallImageKey = "",
                    SmallImageText = "",
                    ButtonFText = "",
                    ButtonFLink = "",
                    ButtonSText = "",
                    ButtonSLink = "",
                    Autostart = "0"
                };

                // Запись JSON в файл
                string json = JsonConvert.SerializeObject(configData, Formatting.Indented);
                File.WriteAllText(jsonFilePath, json);
            }
        }

        private static void DiscordConnect()
        {
            DiscordConnect discordConnect = new();
            discordConnect.InjectLogin();
        }
    }

    public class AppDataFolder
    {
        // Путь к директории в AppData
        public static string Path()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Discord Custom RPC");
        }
    }

    public class Logger
    {
        private readonly string logFilePath;

        public Logger()
        {
            string logFolder = AppDataFolder.Path();
            Directory.CreateDirectory(logFolder);

            logFilePath = Path.Combine(logFolder, "logger.log");
        }

        public void Log(string message)
        {
            using StreamWriter writer = File.AppendText(logFilePath);
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:FFF} - {message}";
            writer.WriteLine(logEntry);
        }
    }

}

class LastConfigData
{
    public string? ApplicationId { get; set; }
    public string? Details { get; set; }
    public string? State { get; set; }
    public string? Timestamp { get; set; }
    public string? LargeImageKey { get; set; }
    public string? LargeImageText { get; set; }
    public string? SmallImageKey { get; set; }
    public string? SmallImageText { get; set; }
    public string? ButtonFText { get; set; }
    public string? ButtonFLink { get; set; }
    public string? ButtonSText { get; set; }
    public string? ButtonSLink { get; set; }
    public string? Autostart { get; set; }
}
