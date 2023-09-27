using System;
using DiscordRPC;
using DiscordRPC.Logging;


namespace Discord_Custom_RPC
{
    public class DiscordConnect
    {
        public delegate void UserChangedEventHandler(string username, string avatarUrl);
        public static event UserChangedEventHandler? UserChanged;

        DiscordRpcClient? client;
        const string idForFirstlaunch = "1155622813902835843";

        public string InjectLogin()
        {
            DiscordConnect script = new DiscordConnect();
            return script.GetUserinfo();
        }

        public string GetUserinfo()
        {
            string username = "";
            client = new DiscordRpcClient(idForFirstlaunch);

            client.Logger = new NullLogger() { Level = LogLevel.Warning };
            client.OnReady += (sender, e) =>
            {
                UserChanged?.Invoke(e.User.Username, e.User.GetAvatarURL(format: User.AvatarFormat.PNG));
            };
            client.Initialize();
            return username;
        }

        public bool Initialize(
            string applicationId, 
            string? details, 
            string? state, 
            string timestamp, 
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
            client.Logger = new NullLogger() { Level = LogLevel.Warning };
            
            Timestamps customTimestamp;
            bool isFirstButtonActive = false;
            bool isSecondButtonActive = false;

            var button1 = new DiscordRPC.Button
            {
                Label = null,
                Url = "https://discord.com",
            };
            var button2 = new DiscordRPC.Button
            {
                Label = null,
                Url = "https://discord.com",
            };
            var buttons = new DiscordRPC.Button[] { button1, button2 };


            // работа с переменными
            if (details == "" || details?.Trim().Length == 0)
                details = null;
            if (state == "" || state?.Trim().Length == 0)
                state = null;
            if (largeImageKey == "" || largeImageKey?.Trim().Length == 0)
                largeImageKey = null;
            if (largeImageText == "" || largeImageText?.Trim().Length == 0)
                largeImageText = null;
            if (smallImageKey == "" || smallImageKey?.Trim().Length == 0)
                smallImageKey = null;
            if (smallImageText == "" || smallImageText?.Trim().Length == 0)
                smallImageText = null;

            // Работа с timestamp
            if (timestamp == "" || timestamp == "0" || state?.Trim().Length == 0)
            {
                DateTime currentTimeUtc = DateTime.UtcNow;
                customTimestamp = new Timestamps()
                {
                    StartUnixMilliseconds = (ulong)(currentTimeUtc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                };
            }
            else
            {
                ulong newtimestamp = Convert.ToUInt64(timestamp.Trim());
                customTimestamp = new Timestamps()
                {
                    StartUnixMilliseconds = newtimestamp
                };
            }

            // Работа с кнопками
            if ((buttonFText != "" || buttonFText?.Trim().Length != 0) && (Uri.TryCreate(buttonFLink, UriKind.Absolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
            {
                button1 = new DiscordRPC.Button
                {
                    Label = buttonFText,
                    Url = buttonFLink,
                };
                isFirstButtonActive = true;
            }
                
            if ((buttonSText != "" || buttonSText?.Trim().Length != 0) && (Uri.TryCreate(buttonSLink, UriKind.Absolute, out Uri? uriResult2) && (uriResult2.Scheme == Uri.UriSchemeHttp || uriResult2.Scheme == Uri.UriSchemeHttps)))
            {
                button2 = new DiscordRPC.Button
                {
                    Label = buttonSText,
                    Url = buttonSLink,
                };
                isSecondButtonActive = true;
            }

            if (isFirstButtonActive && isSecondButtonActive)
                buttons = new DiscordRPC.Button[] { button1, button2 };
            else if (isFirstButtonActive && !isSecondButtonActive)
                buttons = new DiscordRPC.Button[] { button1 };

            else if (!isFirstButtonActive && isSecondButtonActive)
                buttons = new DiscordRPC.Button[] { button2 };

            else if (!isFirstButtonActive && !isSecondButtonActive)
                buttons = null;


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
