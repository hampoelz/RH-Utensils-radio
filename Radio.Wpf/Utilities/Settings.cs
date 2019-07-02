using MahApps.Metro;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Radio.Wpf.Pages;
using Radio.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Radio.Wpf.Utilities
{
    public static class Settings
    {
        public static bool HasProperty(this Type obj, string propertyName)
        {
            return obj.GetProperty(propertyName) != null;
        }

        private static string theme = "dark";

        public static string Theme
        {
            get => theme;
            set
            {
                value = value.ToLower();

                if (theme == value || string.IsNullOrEmpty(value)) return;

                List<string> Themes = new List<string> { "dark", "light" };

                if (!Themes.Contains(value)) return;

                theme = value;

                try
                {
                    if (value == "light")
                    {
                        new PaletteHelper().SetLightDark(false);

                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("blue"), ThemeManager.GetAppTheme("BaseLight"));

                        Pages.Settings.ThemeProperty.IsChecked = true;
                    }
                    else
                    {
                        new PaletteHelper().SetLightDark(true);

                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("blue"), ThemeManager.GetAppTheme("BaseDark"));

                        Pages.Settings.ThemeProperty.IsChecked = false;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex);
                }
            }
        }

        private static string test;

        public static string Test
        {
            get => test;
            set
            {
                if (test == value) return;

                if (bool.TryParse(value, out bool result))
                {
                    Pages.Settings.TestProperty.IsChecked = result;
                    test = value;
                }
            }
        }

        private static List<PinItem> pinned;

        public static List<PinItem> Pinned
        {
            get => pinned;
            set
            {
                if (pinned == value) return;

                pinned = value;
            }
        }
    }
}