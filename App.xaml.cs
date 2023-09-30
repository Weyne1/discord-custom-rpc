using System.IO;
using System.Diagnostics;
using System.Windows;
using Newtonsoft.Json;

namespace Discord_Custom_RPC
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Проверка, запущен ли Discord. Запуск панели
            if (IsDiscordRunning())
            {
                DiscordConnect discordConnect = new DiscordConnect();
                discordConnect.InjectLogin();

                string jsonFilePath = "lastConfig.json";

                if (!File.Exists(jsonFilePath))
                {
                    // Объект, который будет сериализован в JSON
                    LastConfigData configData = new LastConfigData
                    {
                        applicationId = "",
                        details = "",
                        state = "",
                        timestamp = "",
                        largeImageKey = "",
                        largeImageText = "",
                        smallImageKey = "",
                        smallImageText = "",
                        buttonFText = "",
                        buttonFLink = "",
                        buttonSText = "",
                        buttonSLink = ""
                    };

                    // Запись JSON в файл
                    string json = JsonConvert.SerializeObject(configData, Formatting.Indented);
                    File.WriteAllText(jsonFilePath, json);
                }

            }
            else
            {
                // Вывод уведомления и закрытие приложения
                MessageBox.Show("Please open Discord Client first before launching the panel.", "Discord Not Open", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
            }
        }

        private bool IsDiscordRunning()
        {
            Process[] processes = Process.GetProcessesByName("Discord");
            return processes.Length > 0;
        }
    }
}

class LastConfigData
{
    public string? applicationId { get; set; }
    public string? details { get; set; }
    public string? state { get; set; }
    public string? timestamp { get; set; }
    public string? largeImageKey { get; set; }
    public string? largeImageText { get; set; }
    public string? smallImageKey { get; set; }
    public string? smallImageText { get; set; }
    public string? buttonFText { get; set; }
    public string? buttonFLink { get; set; }
    public string? buttonSText { get; set; }
    public string? buttonSLink { get; set; }
}
