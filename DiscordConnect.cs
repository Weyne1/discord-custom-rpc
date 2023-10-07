using System;
using DiscordRPC;


namespace Discord_Custom_RPC
{
    public class DiscordConnect
    {
        public delegate void UserChangedEventHandler(string username, string avatarUrl);
        public static event UserChangedEventHandler? UserChanged;

        DiscordRpcClient? client;
        const string InjectID = "1155622813902835843"; // Discord Custom RPC default app ID


        public string InjectLogin()
        {
            DiscordConnect script = new();
            return script.GetUserInfo();
        }

        public string GetUserInfo()
        {
            string username = "";
            client = new DiscordRpcClient(InjectID);

            //client.Logger = new FileLogger("logger.log") { Level = LogLevel.Warning };
            client.OnReady += (sender, e) =>
            {
                UserChanged?.Invoke(e.User.Username, e.User.GetAvatarURL(format: User.AvatarFormat.PNG));
            };
            client.Initialize();
            return username;
        }

        // Соединение
        public bool Initialize(
            string applicationId, 
            string? details, 
            string? state, 
            string? timestamp, 
            string? largeImageKey, 
            string? largeImageText, 
            string? smallImageKey,
            string? smallImageText,
            string? buttonFText,
            string? buttonFLink,
            string? buttonSText,
            string? buttonSLink
            )
        {
            client = new DiscordRpcClient(applicationId);
            //client.Logger = new FileLogger("logger.log") {  Level = LogLevel.Warning };
            
            Timestamps customTimestamp;
            bool isFirstButtonActive = false;
            bool isSecondButtonActive = false;

            var button1 = new Button
            {
                Label = null,
                Url = "https://discord.com",
            };
            var button2 = new Button
            {
                Label = null,
                Url = "https://discord.com",
            };
            _ = new Button[] { button1, button2 };

            // работа с переменными
            if (details?.Trim().Length == 0)
                details = null;
            if (state?.Trim().Length == 0)
                state = null;
            if (largeImageKey?.Trim().Length == 0)
                largeImageKey = null;
            if (largeImageText?.Trim().Length == 0)
                largeImageText = "Game image";
            if (smallImageKey?.Trim().Length == 0)
                smallImageKey = "Small game image";
            if (smallImageText?.Trim().Length == 0)
                smallImageText = null;

            // Работа с Timestamp
            if (timestamp == "0" || timestamp?.Trim().Length == 0)
            {
                DateTime currentTimeUtc = DateTime.UtcNow;
                customTimestamp = new Timestamps()
                {
                    StartUnixMilliseconds = (ulong)(currentTimeUtc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                };
            }
            else
            {
                ulong unixTimestamp = Convert.ToUInt64(timestamp?.Trim());
                customTimestamp = new Timestamps()
                {
                    StartUnixMilliseconds = unixTimestamp
                };
            }

            // Работа с кнопками
            // Если ссылка в кнопке рабочая, кнопка отображается
            if (Uri.TryCreate(buttonFLink, UriKind.Absolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                if (buttonFText?.Trim().Length == 0)
                    buttonFText = "Button 1";

                button1 = new Button
                {
                    Label = buttonFText,
                    Url = buttonFLink,
                };
                isFirstButtonActive = true;
            }
                
            if (Uri.TryCreate(buttonSLink, UriKind.Absolute, out Uri? uriResult2) && (uriResult2.Scheme == Uri.UriSchemeHttp || uriResult2.Scheme == Uri.UriSchemeHttps))
            {
                if (buttonSText?.Trim().Length == 0)
                    buttonSText = "Button 2";

                button2 = new Button
                {
                    Label = buttonSText,
                    Url = buttonSLink,
                };
                isSecondButtonActive = true;
            }

            Button[]? buttons;
            if (isFirstButtonActive && isSecondButtonActive)
                buttons = new Button[] { button1, button2 };

            else if (isFirstButtonActive && !isSecondButtonActive)
                buttons = new Button[] { button1 };

            else if (!isFirstButtonActive && isSecondButtonActive)
                buttons = new Button[] { button2 };

            else buttons = null;

            // Подключение к RPC
            client.Initialize();

            // Настройки RPC
            client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Timestamps = customTimestamp,
                Buttons = buttons,
                Assets = new Assets()
                {
                    LargeImageKey = largeImageKey,
                    LargeImageText = largeImageText,
                    SmallImageKey = smallImageKey,
                    SmallImageText = smallImageText
                }
            });
            return true;
        }

        // Разрыв соединения
        public void Deinitialize()
        {
            client?.Dispose();
            client = null;
        }
    }
}