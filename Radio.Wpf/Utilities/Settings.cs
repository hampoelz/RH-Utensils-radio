using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Radio.Wpf.Modules;
using Radio.Wpf.Pages;

namespace Radio.Wpf.Utilities
{
    public static class Settings
    {
        private static string _theme = "dark";

        private static string _test;

        private static string _pinned;

        public static string Theme
        {
            get => _theme;
            set
            {
                value = value.ToLower();

                if (_theme == value || string.IsNullOrEmpty(value)) return;

                var themes = new List<string> {"dark", "light"};

                if (!themes.Contains(value)) return;

                _theme = value;

                try
                {
                    if (value == "light")
                    {
                        new PaletteHelper().SetLightDark(false);

                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("blue"),
                            ThemeManager.GetAppTheme("BaseLight"));

                        UnderConstruction.Image.Source =
                            new BitmapImage(new Uri("/Radio;component/Assets/under-construction-dark.png",
                                UriKind.Relative));

                        Pages.Settings.ThemeProperty.IsChecked = true;
                    }
                    else
                    {
                        new PaletteHelper().SetLightDark(true);

                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("blue"),
                            ThemeManager.GetAppTheme("BaseDark"));

                        UnderConstruction.Image.Source = new BitmapImage(
                            new Uri("/Radio;component/Assets/under-construction-light.png", UriKind.Relative));

                        Pages.Settings.ThemeProperty.IsChecked = false;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex);
                }
            }
        }

        public static string Test
        {
            get => _test;
            set
            {
                if (_test == value) return;

                if (!bool.TryParse(value, out var result)) return;
                Pages.Settings.TestProperty.IsChecked = result;
                _test = value;
            }
        }

        public static string Pinned
        {
            get => _pinned;
            set
            {
                if (value == _pinned || string.IsNullOrEmpty(value)) return;

                _pinned = value;

                var json = value.Replace("'", "\"");

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new TupleConverter());

                var items = (List<PinItem>) JsonConvert.DeserializeObject(json, new List<PinItem>().GetType(),
                    settings);

                foreach (var item in items)
                {
                    item.Title = item.Title.Replace("&apos;", "'");
                    item.Path = item.Path.Replace("&apos;", "'");
                }

                Player.PinnedItems.ItemsSource = null;
                Player.PinnedItems.ItemsSource = items;
            }
        }

        public static bool HasProperty(this Type obj, string propertyName)
        {
            return obj.GetProperty(propertyName) != null;
        }
    }
}