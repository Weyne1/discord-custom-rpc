using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;


namespace Discord_Custom_RPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region -- Переменные --

        private const string DefaultButtonContent = "Set Activity";
        private const string DiscordDevPortalURL = "https://discord.com/developers/applications";

        private bool isLoading = true;
        private bool isButtonActive = false;
        private bool isStatusSet = false;

        private static readonly string appDataFolder = AppDataFolder.Path();
        private static readonly string jsonFilePath = Path.Combine(appDataFolder, "lastConfig.json");
        private AddButtons? addButtons; // Buttons setup window
        private readonly NotifyIcon notifyIcon;
        DiscordConnect? currentConnectedClient = null;

        #endregion


        #region -- Основные параметры окна --

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация NotifyIcon
            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new System.Drawing.Icon(Path.Combine(appDataFolder, "tray_icon.ico")),
                Text = Title
            };


            // Контекстное меню для NotifyIcon
            ContextMenuStrip contextMenuStrip = new();

            // Элементы меню
            notifyIcon.MouseClick += OnNotifyIconClick;
            ToolStripMenuItem exitMenuItem = new("Exit Custom RPC");

            exitMenuItem.Click += OnExitClick;

            contextMenuStrip.Items.Add(exitMenuItem);
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // Поле с текстом, в котором имя пользователя
            DiscordConnect.UserChanged += UserChangedEventHandler;
            StartLoading();
            ReadJsonData();
        }

        // Перетягивание всего окна
        #pragma warning disable IDE1006 // Стили именования
        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        #pragma warning restore IDE1006 // Стили именования
        {
            DragMove();
        }

        // Правый клик по верхней панели - вызов контекстного меню
        #pragma warning disable IDE1006 // Стили именования
        private void titleBar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        #pragma warning restore IDE1006 // Стили именования
        {
            ContextMenu contextMenu = new();

            MenuItem closeMenuItem = new();
            MenuItem trayMenuItem = new();
            Separator separator = new();

            closeMenuItem.Header = "Close Panel";
            closeMenuItem.Click += OnExitClick;
            trayMenuItem.Header = "Minimize to tray";
            trayMenuItem.Click += ImageCloseClick;

            contextMenu.Items.Add(trayMenuItem);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(closeMenuItem);

            contextMenu.IsOpen = true;
        }

        // Наведение мыши на крестик
        private void RedSquareMouseEnter(object sender, EventArgs e)
        {
            RedSquare.Opacity = 1.0;
        }

        private void RedSquareMouseLeave(object sender, EventArgs e)
        {
            RedSquare.Opacity = 0;
        }

        #endregion


        #region -- JSON --

        private void ReadJsonData()
        {
            TextBoxAppID.Text = Regex.Replace(ReadJsonValue("ApplicationId"), "[^0-9]", "");
            TextBoxDetails.Text = ReadJsonValue("Details");
            TextBoxState.Text = ReadJsonValue("State");
            TextBoxTimestamp.Text = Regex.Replace(ReadJsonValue("Timestamp"), "[^0-9]", "");
            TextBoxLargeImageKey.Text = ReadJsonValue("LargeImageKey");
            TextBoxLargeImageText.Text = ReadJsonValue("LargeImageText");
            TextBoxSmallImageKey.Text = ReadJsonValue("SmallImageKey");
            TextBoxSmallImageText.Text = ReadJsonValue("SmallImageText");

            if (ReadJsonValue("Autostart") == "1")
                SwithAllowAutostart.IsChecked = true;
        }

        // Чтение JSON
        private static string ReadJsonValue(string propertyName)
        {

            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                LastConfigData? configData = JsonConvert.DeserializeObject<LastConfigData>(json);

                // Рефлексия для получения значения свойства по имени
                PropertyInfo? property = typeof(LastConfigData).GetProperty(propertyName);
                if (property != null)
                {
                    object? value = property.GetValue(configData);
                    return value?.ToString() ?? "";
                }
            }
            return "";
        }

        #endregion


        #region -- Загрузка GUI --

        private async void StartLoading()
        {
            // Переключение анимации "Please wait"
            while (isLoading)
            {
                await Task.Delay(500);
                ToggleLoadingAnimation();
            }
            ButtonSetActivity.Content = DefaultButtonContent;

            if (TextBoxAppID.Text == "" || TextBoxAppID.Text.Trim().Length == 0)
                ButtonSetActivity.IsEnabled = false;
            else
                ButtonSetActivity.IsEnabled = true;

        }

        private void ToggleLoadingAnimation()
        {
            // Ожидание загрузки username
            Dispatcher.Invoke(() =>
            {
                if (TextUsername.Text.Contains("Please wait"))
                {
                    if (TextUsername.Text == "Please wait")
                        TextUsername.Text = "Please wait.";
                    else if (TextUsername.Text == "Please wait.")
                        TextUsername.Text = "Please wait..";
                    else if (TextUsername.Text == "Please wait..")
                        TextUsername.Text = "Please wait...";
                    else if (TextUsername.Text == "Please wait...")
                        TextUsername.Text = "Please wait";


                    BitmapImage bitmapImage = new(new Uri("/Assets/ds_logo.png", UriKind.Relative));
                    ImageDSLogo.Source = bitmapImage;
                    ButtonSetActivity.IsEnabled = false;
                    ButtonSetActivity.Content = "Loading...";
                }

                if (!IsDiscordRunning())
                {
                    RectangleWarnings.IsEnabled = true;
                    TextWarnings.Text = "Please open Discord";
                }

                else if (!CheckNet())
                {
                    RectangleWarnings.IsEnabled = true;
                    TextWarnings.Text = "Internet connection lost";
                }

            });
        }

        private void UserChangedEventHandler(string username, string avatarUrl)
        {
            // Остановка анимации и новое значение Username
            isLoading = false;

            Dispatcher.Invoke(() =>
            {
                TextUsername.Text = username;
                TextUsername.Opacity = 1;
                TextConnecting.Text = "CONNECTED TO:";

                // Создание нового Uri для аватарки
                Uri newImageUri = new(avatarUrl);
                BitmapImage newImage = new(newImageUri);
                ImageDSLogo.Source = newImage;

                // Разрыв первоначального соединения
                DiscordConnect discordConnect = new();
                discordConnect.Deinitialize();
            });

            Dispatcher.Invoke(() => DisableWarning());
        }

        private async void ImageMouseRotation(object sender, MouseButtonEventArgs e)
        {
            // Анимация вращения
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1080,
                Duration = TimeSpan.FromSeconds(2),
                EasingFunction = new QuadraticEase(),
                IsAdditive = true,
            };



            // Запуск анимации в фоновом потоке
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                });
            });
        }

        #endregion


        #region -- Поля и их заполнение --
        
        // Ограничение на символы
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // ВЗАИМОДЕЙСТВИЯ С ПОЛЯМИ
        private void TextBoxAppIDTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxAppID.Text = Regex.Replace(TextBoxAppID.Text, "[^0-9]", "");

            if (TextBoxAppID.Text != "")
            {
                TextAppIDWatermark.Opacity = 0;

                if (TextBoxAppID.Text.Length >= 15)
                {
                    isButtonActive = true;
                    ButtonSetActivity.IsEnabled = true;
                }
            }

            else
            {
                TextAppIDWatermark.Opacity = 1;
                isButtonActive = false;
                ButtonSetActivity.IsEnabled = false;
            }
        }

        private void TextBoxDetailsTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxDetails.Text == "")
                TextDetailsWatermark.Opacity = 1;
            else
                TextDetailsWatermark.Opacity = 0;

            ButtonCheckToEnabled();
        }

        private void TextBoxStateTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxState.Text == "")
                TextStateWatermark.Opacity = 1;
            else
                TextStateWatermark.Opacity = 0;

            ButtonCheckToEnabled();
        }

        private void TextBoxTimestampTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxTimestamp.Text = Regex.Replace(TextBoxTimestamp.Text, "[^0-9]", "");

            if (TextBoxTimestamp.Text == "")
                TextTimestampWatermark.Opacity = 1;
            else
                TextTimestampWatermark.Opacity = 0;

            ButtonCheckToEnabled();
        }

        private void TextBoxLargeImageKeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxLargeImageKey.Text == "")
                TextLargeImageKeyWatermark.Opacity = 1;
            else
                TextLargeImageKeyWatermark.Opacity = 0;

            ButtonCheckToEnabled();
        }

        private void TextBoxLargeImageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxLargeImageText.Text == "")
                TextLargeImageTextWatermark.Opacity = 1;
            else
                TextLargeImageTextWatermark.Opacity = 0;

            ButtonCheckToEnabled();
        }

        private void TextBoxSmallImageKeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSmallImageKey.Text == "")
                TextSmallImageKeyWatermark.Opacity = 1;
            else
                TextSmallImageKeyWatermark.Opacity = 0;

            ButtonCheckToEnabled();
        }

        private void TextBoxSmallImageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSmallImageText.Text == "")
                TextSmallImageTextWatermark.Opacity = 1;
            else
                TextSmallImageTextWatermark.Opacity = 0;

            ButtonCheckToEnabled();
        }

        #endregion


        #region -- Переключатель автостарта --

        private void SwithAllowAutostartClick(object sender, RoutedEventArgs e)
        {
            string updatedJson;
            if (SwithAllowAutostart.IsChecked == true) // Включение .exe в автостарт
            {
                if (Autostart.SetAutoStart(true) == true)
                {
                    updatedJson = JsonConvert.SerializeObject(GetConfigdata("1"), Formatting.Indented);
                }

                else // Если не удалось добавить в автозапуск
                {
                    SwithAllowAutostart.IsChecked = false;
                    updatedJson = JsonConvert.SerializeObject(GetConfigdata("0"), Formatting.Indented);
                }

            }

            else
            {
                Autostart.SetAutoStart(false);

                updatedJson = JsonConvert.SerializeObject(GetConfigdata("0"), Formatting.Indented);
            }

            File.WriteAllText(jsonFilePath, updatedJson);
        }

        private static object GetConfigdata(string newValue)
        {
            LastConfigData configData = new()
            {
                ApplicationId = ReadJsonValue("ApplicationId"),
                Details = ReadJsonValue("Details"),
                State = ReadJsonValue("State"),
                Timestamp = ReadJsonValue("Timestamp"),
                LargeImageKey = ReadJsonValue("LargeImageKey"),
                LargeImageText = ReadJsonValue("LargeImageText"),
                SmallImageKey = ReadJsonValue("SmallImageKey"),
                SmallImageText = ReadJsonValue("SmallImageText"),
                ButtonFText = ReadJsonValue("ButtonFText"),
                ButtonFLink = ReadJsonValue("ButtonFLink"),
                ButtonSText = ReadJsonValue("ButtonSText"),
                ButtonSLink = ReadJsonValue("ButtonSLink"),
                Autostart = newValue,
            };

            return configData;
        }

        #endregion


        #region -- Окно настройки кнопок "Add Buttons" --

        // Окно настройки кнопок
        private void ButtonSetupClick(object sender, RoutedEventArgs e)
        {
            addButtons = new AddButtons(this);
            addButtons.Show();
            ButtonSetup.IsEnabled = false;
        }
        public void ActivateButtonOnMainWindow()
        {
            ButtonSetup.IsEnabled = true;
        }

        #endregion


        #region -- Кнопка "Set Activity" --

        // Включение кнопки
        public void ButtonCheckToEnabled()
        {
            if (isButtonActive && !ButtonSetActivity.IsEnabled && !isLoading)
            {
                ButtonSetActivity.IsEnabled = true;
                ButtonSetActivity.Content = DefaultButtonContent;
            }
        }

        private void ButtonDevPortalClick(object sender, RoutedEventArgs e)
        {
            string url = DiscordDevPortalURL;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        // Нажатие на кнопку
        private void ButtonSetActivityClick(object sender, RoutedEventArgs e)
        {
            ButtonSetActivity.IsEnabled = false;

            LastConfigData configData = new()
            {
                ApplicationId = TextBoxAppID.Text,
                Details = TextBoxDetails.Text,
                State = TextBoxState.Text,
                Timestamp = TextBoxTimestamp.Text,
                LargeImageKey = TextBoxLargeImageKey.Text,
                LargeImageText = TextBoxLargeImageText.Text,
                SmallImageKey = TextBoxSmallImageKey.Text,
                SmallImageText = TextBoxSmallImageText.Text,
                ButtonFText = ReadJsonValue("ButtonFText"),
                ButtonFLink = ReadJsonValue("ButtonFLink"),
                ButtonSText = ReadJsonValue("ButtonSText"),
                ButtonSLink = ReadJsonValue("ButtonSLink"),
                Autostart = ReadJsonValue("Autostart"),
            };

            // Перезапись файла
            string updatedJson = JsonConvert.SerializeObject(configData, Formatting.Indented);
            File.WriteAllText(jsonFilePath, updatedJson);

            if (isStatusSet)
            {
                currentConnectedClient?.Deinitialize();
                Thread.Sleep(100);

                isStatusSet = false;
            }

            if (!IsDiscordRunning())
                Reconnect("Please open Discord");
            else if (!CheckNet())
                Reconnect("Internet connection lost");
            else
            {
                ButtonSetActivity.Content = "Activity set!";

                // Connecting
                currentConnectedClient = new DiscordConnect();
                isStatusSet = currentConnectedClient.Initialize(
                    applicationId: configData.ApplicationId,
                    details: configData.Details,
                    state: configData.State,
                    timestamp: configData.Timestamp,
                    largeImageKey: configData.LargeImageKey,
                    largeImageText: configData.LargeImageText,
                    smallImageKey: configData.SmallImageKey,
                    smallImageText: configData.SmallImageText,
                    buttonFText: configData.ButtonFText,
                    buttonFLink: configData.ButtonFLink,
                    buttonSText: configData.ButtonSText,
                    buttonSLink: configData.ButtonSLink);
            }

        }

        private static bool IsDiscordRunning()
        {
            Process[] processes = Process.GetProcessesByName("Discord");
            return processes.Length > 0;
        }

        private void Reconnect(string reason)
        {
            TextConnecting.Text = "RECONNECTING...";
            TextUsername.Text = "Please wait";
            TextUsername.Opacity = 0.5;
            TextWarnings.Text = reason;

            RectangleWarnings.IsEnabled = true;
            isLoading = true;

            DiscordConnect.UserChanged += UserChangedEventHandler;

            StartLoading();

            DiscordConnect discordConnect = new();
            discordConnect.InjectLogin();
            discordConnect.Deinitialize();
        }

        private void DisableWarning()
        {
            RectangleWarnings.IsEnabled = false;
        }


        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static bool CheckNet()
        {
            return InternetGetConnectedState(out _, 0);
        }

        #endregion


        #region -- Трей --

        // Закрытие панели
        private void OnExitClick(object? sender, EventArgs e)
        {
            if (isStatusSet)
            {
                currentConnectedClient?.Deinitialize();
                Thread.Sleep(100);
            }
            Close();
        }
        
        // Сворачивание окон в трей
        private void ImageCloseClick(object sender, EventArgs e)
        {
            addButtons?.Close();
            Hide();
            //notifyIcon.ShowBalloonTip(1, "Minimized to tray", "To close the panel, right-click on the icon", ToolTipIcon.Info);
            notifyIcon.Visible = true;
        }
        
        // Выход из трея
        private void OnNotifyIconClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = WindowState.Normal;
                notifyIcon.Visible = false;
            }
        }

        // Обработчик закрытия основного окна
        private void MainWindow_Closing(object sender, EventArgs e)
        {
            if (isStatusSet)
            {
                currentConnectedClient?.Deinitialize();
                Thread.Sleep(100);
            }
            addButtons?.Close();
        }

        #endregion

    }
}