using Microsoft.Win32;
using System;

namespace Discord_Custom_RPC
{
    internal class Autostart
    {

        //Добавление в автозапуск
        public static bool SetAutoStart(bool set)
        {
            const string appName = "Discord Custom RPC";

            try
            {
                RegistryKey? registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (set)
                {
                    string? exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    
                    if (exePath != null)
                    {
                        registryKey?.SetValue(appName, exePath);
                        return true;
                    }
                    else return false;

                }
                else
                {
                    registryKey?.DeleteValue(appName, false);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
