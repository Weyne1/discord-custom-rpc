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
        private AddButtons? addButtons; // Buttons setup window
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private const string defaultButtonContent = "Set Activity";
        private bool isLoading = true;
        private bool IsButtonActive = false;
        private bool isStatusSet = false;
        DiscordConnect? currentConnectedClient = null;


        public MainWindow()
        {
            InitializeComponent();

            // Инициализация NotifyIcon
            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new System.Drawing.Icon("tray_icon.ico"),
                Text = Title
            };


            // Контекстное меню для NotifyIcon
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

            // Элементы меню
            notifyIcon.MouseClick += OnNotifyIconClick;
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit Custom RPC");

            exitMenuItem.Click += OnExitClick;

            contextMenuStrip.Items.Add(exitMenuItem);
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // Поле с текстом, в котором имя пользователя
            DiscordConnect.UserChanged += UserChangedEventHandler;
            StartLoading();
            ReadJsonData();
        }

        // Перетягивание всего окна
        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // Правый клик по верхней панели - вызов контекстного меню
        private void titleBar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();

            MenuItem closeMenuItem = new MenuItem();
            MenuItem trayMenuItem = new MenuItem();
            Separator separator = new Separator();

            closeMenuItem.Header = "Close Panel";
            closeMenuItem.Click += OnExitClick;
            trayMenuItem.Header = "Minimize to tray";
            trayMenuItem.Click += ImageCloseClick;

            contextMenu.Items.Add(trayMenuItem);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(closeMenuItem);

            contextMenu.IsOpen = true;
        }


        private void ReadJsonData()
        {
            TextBoxAppID.Text = Regex.Replace(ReadJsonValue("applicationId"), "[^0-9]", "");
            TextBoxDetails.Text = ReadJsonValue("details");
            TextBoxState.Text = ReadJsonValue("state");
            TextBoxTimestamp.Text = Regex.Replace(ReadJsonValue("timestamp"), "[^0-9]", "");
            TextBoxLargeImageKey.Text = ReadJsonValue("largeImageKey");
            TextBoxLargeImageText.Text = ReadJsonValue("largeImageText");
            TextBoxSmallImageKey.Text = ReadJsonValue("SmallImageKey");
            TextBoxSmallImageText.Text = ReadJsonValue("SmallImageText");
        }

        // ЧТЕНИЕ JSON
        private static string ReadJsonValue(string propertyName)
        {
            string jsonFilePath = "lastConfig.json";

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


        private async void StartLoading()
        {
            // Переключение анимации "Please wait"
            while (isLoading)
            {
                await Task.Delay(500);
                ToggleLoadingAnimation();
            }
            ButtonSetActivity.Content = defaultButtonContent;

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
                if (TextUsername.Text == "Please wait")
                    TextUsername.Text = "Please wait.";
                else if (TextUsername.Text == "Please wait.")
                    TextUsername.Text = "Please wait..";
                else if (TextUsername.Text == "Please wait..")
                    TextUsername.Text = "Please wait...";
                else if (TextUsername.Text == "Please wait...")
                    TextUsername.Text = "Please wait";

                ButtonSetActivity.IsEnabled = false;
                ButtonSetActivity.Content = "Loading...";
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
                Uri newImageUri = new Uri(avatarUrl);
                BitmapImage newImage = new BitmapImage(newImageUri);
                ImageDSLogo.Source = newImage;

                // Разрыв соединения
                DiscordConnect discordConnect = new DiscordConnect();
                discordConnect.Deinitialize();

            });
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


        // Поле "ApplicationID"
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Ограничение на цифры
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }



        // ВЗАИМОДЕЙСТВИЯ С ПОЛЯМИ
        private void TextBoxAppIDTextChanged(object sender, TextChangedEventArgs e)
        {
            bool isWhitespace = TextBoxAppID.Text.Trim().Length == 0;
            TextBoxAppID.Text = Regex.Replace(TextBoxAppID.Text, "[^0-9]", "");

            if (TextBoxAppID.Text != "")
            {
                TextAppIDWatermark.Opacity = 0;

                if (!isWhitespace) // Если строка не из пробелов, кнопка включается
                {
                    IsButtonActive = true;
                    ButtonSetActivity.IsEnabled = true;
                    ButtonSetActivity.Content = defaultButtonContent;
                }
            }

            else
            {
                TextAppIDWatermark.Opacity = 1;
                IsButtonActive = false;
                ButtonSetActivity.IsEnabled = false;
            }
        }

        private void TextBoxDetailsTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxDetails.Text == "")
            {
                TextDetailsWatermark.Opacity = 1;
            }
            else
            {
                TextDetailsWatermark.Opacity = 0;
            }


            ButtonCheckToEnabled();
        }

        private void TextBoxStateTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxState.Text == "")
            {
                TextStateWatermark.Opacity = 1;
            }
            else
            {
                TextStateWatermark.Opacity = 0;
            }

            ButtonCheckToEnabled();
        }

        private void TextBoxTimestampTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxTimestamp.Text = Regex.Replace(TextBoxTimestamp.Text, "[^0-9]", "");

            if (TextBoxTimestamp.Text == "")
            {
                TextTimestampWatermark.Opacity = 1;
            }
            else
            {
                TextTimestampWatermark.Opacity = 0;
            }

            ButtonCheckToEnabled();
        }

        private void TextBoxLargeImageKeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxLargeImageKey.Text == "")
            {
                TextLargeImageKeyWatermark.Opacity = 1;
            }
            else
            {
                TextLargeImageKeyWatermark.Opacity = 0;
            }

            ButtonCheckToEnabled();
        }

        private void TextBoxLargeImageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxLargeImageText.Text == "")
            {
                TextLargeImageTextWatermark.Opacity = 1;
            }
            else
            {
                TextLargeImageTextWatermark.Opacity = 0;
            }

            ButtonCheckToEnabled();
        }

        private void TextBoxSmallImageKeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSmallImageKey.Text == "")
            {
                TextSmallImageKeyWatermark.Opacity = 1;
            }
            else
            {
                TextSmallImageKeyWatermark.Opacity = 0;
            }

            ButtonCheckToEnabled();
        }

        private void TextBoxSmallImageTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSmallImageText.Text == "")
            {
                TextSmallImageTextWatermark.Opacity = 1;
            }
            else
            {
                TextSmallImageTextWatermark.Opacity = 0;
            }

            ButtonCheckToEnabled();

        }

        // ОКНО НАСТРОЙКИ КНОПОК
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


        // ВКЛЮЧЕНИЕ КНОПКИ УСТАНОВКИ СТАТУСА
        public void ButtonCheckToEnabled()
        {
            if (IsButtonActive && !ButtonSetActivity.IsEnabled)
            {
                ButtonSetActivity.IsEnabled = true;
                ButtonSetActivity.Content = defaultButtonContent;
            }
        }

        private void ButtonDevPortalClick(object sender, RoutedEventArgs e)
        {
            string url = "https://discord.com/developers/applications";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }


        // НАЖАТИЕ НА КНОПКУ УСТАНОВКИ СТАТУСА
        private void ButtonSetActivityClick(object sender, RoutedEventArgs e)
        {
            string jsonFilePath = "lastConfig.json";

            ButtonSetActivity.IsEnabled = false;
            ButtonSetActivity.Content = "Activity set!";
            
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
                smallImageText = ""
            };

            // Изменение данных
            string applicationId = TextBoxAppID.Text;
            string details = TextBoxDetails.Text;
            string state = TextBoxState.Text;
            string timestamp = TextBoxTimestamp.Text;
            string largeImageKey = TextBoxLargeImageKey.Text;
            string largeImageText = TextBoxLargeImageText.Text;
            string smallImageKey = TextBoxSmallImageKey.Text;
            string smallImageText = TextBoxSmallImageText.Text;
            string buttonFText = ReadJsonValue("buttonFText");
            string buttonFLink = ReadJsonValue("buttonFLink");
            string buttonSText = ReadJsonValue("buttonSText");
            string buttonSLink = ReadJsonValue("buttonSLink");

            configData = new LastConfigData
            {
                applicationId = TextBoxAppID.Text,
                details = TextBoxDetails.Text,
                state = TextBoxState.Text,
                timestamp = TextBoxTimestamp.Text,
                largeImageKey = TextBoxLargeImageKey.Text,
                largeImageText = TextBoxLargeImageText.Text,
                smallImageKey = TextBoxSmallImageKey.Text,
                smallImageText = TextBoxSmallImageText.Text,
                buttonFText = buttonFText,
                buttonFLink = buttonFLink,
                buttonSText = buttonSText,
                buttonSLink = buttonSLink
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


            currentConnectedClient = new DiscordConnect();
            isStatusSet = currentConnectedClient.Initialize(
                applicationId,
                details,
                state,
                timestamp,
                largeImageKey,
                largeImageText,
                smallImageKey,
                smallImageText,
                buttonFText,
                buttonFLink,
                buttonSText,
                buttonSLink
                );
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



        // ВЫХОД В ТРЕЙ

        private void OnExitClick(object? sender, EventArgs e)
        {
            // Закрытие панели
            if (isStatusSet)
            {
                currentConnectedClient?.Deinitialize();
                Thread.Sleep(50);
            }
            Close();
        }

        private void ImageCloseClick(object sender, EventArgs e)
        {
            // Сворачивание окна в трей
            addButtons?.Close();
            Hide();
            //notifyIcon.ShowBalloonTip(1, "Minimized to tray", "To close the panel, right-click on the icon", ToolTipIcon.Info);
            notifyIcon.Visible = true;
        }

        private void OnNotifyIconClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Выход из трея
                Show();
                WindowState = WindowState.Normal;
                notifyIcon.Visible = false;
            }
        }

        // Обработчик закрытия основного окна
        private void MainWindow_Closing(object sender, EventArgs e)
        {
            addButtons?.Close();
        }

    }
}