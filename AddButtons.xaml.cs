using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Discord_Custom_RPC
{
    /// <summary>
    /// Логика взаимодействия для AddButtons.xaml
    /// </summary>
    public partial class AddButtons : Window
    {
        private readonly MainWindow? mainWindow;

        private static readonly string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Discord Custom RPC");
        private static readonly string jsonFilePath = Path.Combine(directoryPath, "lastConfig.json");

        #region -- Параметры окна --

        public AddButtons(MainWindow main)
        {
            InitializeComponent();
            ReadJsonButtonsData();
            mainWindow = main;
        }

        private void TitleBarButtons_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion


        #region -- JSON --

        private void ReadJsonButtonsData()
        {
            TextBoxButton1Text.Text = ReadJsonValue("ButtonFText");
            TextBoxButton1Link.Text = ReadJsonValue("ButtonFLink");
            TextBoxButton2Text.Text = ReadJsonValue("ButtonSText");
            TextBoxButton2Link.Text = ReadJsonValue("ButtonSLink");
        }

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


        #region -- Взаимодействие с полями --

        private void TextBoxButton1TextChanged(object sender, RoutedEventArgs e)
        {
            if (TextBoxButton1Text.Text == "")
            {
                TextButton1Watermark.Opacity = 1;
            }
            else
            {
                TextButton1Watermark.Opacity = 0;
            }
        }

        private void TextBoxButton1LinkTextChanged(object sender, RoutedEventArgs e)
        {
            if (TextBoxButton1Link.Text == "")
            {
                TextButton1LinkWatermark.Opacity = 1;
            }
            else
            {
                TextButton1LinkWatermark.Opacity = 0;
            }
        }

        private void TextBoxButton2TextChanged(object sender, RoutedEventArgs e)
        {
            if (TextBoxButton2Text.Text == "")
            {
                TextButton2Watermark.Opacity = 1;
            }
            else
            {
                TextButton2Watermark.Opacity = 0;
            }
        }

        private void TextBoxButton2LinkTextChanged(object sender, RoutedEventArgs e)
        {
            if (TextBoxButton2Link.Text == "")
            {
                TextButton2LinkWatermark.Opacity = 1;
            }
            else
            {
                TextButton2LinkWatermark.Opacity = 0;
            }
        }

        #endregion


        #region -- Нажатие на кнопку "Save" --

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
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
                ButtonFText = TextBoxButton1Text.Text,
                ButtonFLink = TextBoxButton1Link.Text,
                ButtonSText = TextBoxButton2Text.Text,
                ButtonSLink = TextBoxButton2Link.Text,
                Autostart = ReadJsonValue("Autostart"),
            };

            string updatedJson = JsonConvert.SerializeObject(configData, Formatting.Indented);
            File.WriteAllText(jsonFilePath, updatedJson);

            mainWindow?.ButtonCheckToEnabled();
            Close();
        }

        #endregion


        #region -- Закрытие окна --

        // Обработчик закрытия неосновного окна
        private void AddButtons_Closing(object sender, EventArgs e)
        {
            // Активирует кнопку на основном окне при закрытии
            mainWindow?.ActivateButtonOnMainWindow();
        }

        private void ImageCloseClick(object? sender, EventArgs e)
        {
            Close();
        }

        // Работа с зоной крестика
        private void RedSquareMouseEnter(object sender, EventArgs e)
        {
            RedSquare.Opacity = 1.0;
        }

        private void RedSquareMouseLeave(object sender, EventArgs e)
        {
            RedSquare.Opacity = 0;
        }

        #endregion

    }
}
