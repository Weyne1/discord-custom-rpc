using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Discord_Custom_RPC
{
    /// <summary>
    /// Логика взаимодействия для AddButtons.xaml
    /// </summary>
    public partial class AddButtons : Window
    {
        private MainWindow? mainWindow;

        public AddButtons(MainWindow main)
        {
            InitializeComponent();
            ReadJsonData();
            mainWindow = main;
        }

        private void titleBarButtons_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Обработчик закрытия неосновного окна
        private void AddButtons_Closing(object sender, EventArgs e)
        {
            // Активирует кнопку на основном окне при закрытии
            mainWindow?.ActivateButtonOnMainWindow();
        }

        private void ReadJsonData()
        {
            TextBoxButton1Text.Text = ReadJsonValue("buttonFText");
            TextBoxButton1Link.Text = ReadJsonValue("buttonFLink");
            TextBoxButton2Text.Text = ReadJsonValue("buttonSText");
            TextBoxButton2Link.Text = ReadJsonValue("buttonSLink");
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


        // ВЗАИМОДЕЙСТВИЯ С ПОЛЯМИ


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


        // НАЖАТИЕ НА КНОПКУ "SAVE"

        private void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            string jsonFilePath = "lastConfig.json";

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

            string applicationId = ReadJsonValue("applicationId");
            string details = ReadJsonValue("details");
            string state = ReadJsonValue("state");
            string timestamp = ReadJsonValue("timestamp");
            string largeImageKey = ReadJsonValue("largeImageKey");
            string largeImageText = ReadJsonValue("largeImageText");
            string smallImageKey = ReadJsonValue("smallImageKey");
            string smallImageText = ReadJsonValue("smallImageText");
            string buttonFText = TextBoxButton1Text.Text;
            string buttonFLink = TextBoxButton1Link.Text;
            string buttonSText = TextBoxButton2Text.Text;
            string buttonSLink = TextBoxButton2Link.Text;

            configData = new LastConfigData
            {
                applicationId = applicationId,
                details = details,
                state = state,
                timestamp = timestamp,
                largeImageKey = largeImageKey,
                largeImageText = largeImageText,
                smallImageKey = smallImageKey,
                smallImageText = smallImageText,
                buttonFText = TextBoxButton1Text.Text,
                buttonFLink = TextBoxButton1Link.Text,
                buttonSText = TextBoxButton2Text.Text,
                buttonSLink = TextBoxButton2Link.Text
            };

            string updatedJson = JsonConvert.SerializeObject(configData, Formatting.Indented);
            File.WriteAllText(jsonFilePath, updatedJson);

            mainWindow?.ButtonCheckToEnabled();
            Close();
        }



    // Закрытие окна
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

        private void TextBoxLargeImageKey_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
