using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Radio.Wpf.Modules;
using Radio.Wpf.Utilities;
using static Radio.Wpf.Utilities.AudioPlayer;
using FileInfo = Radio.Wpf.Modules.FileInfo;

namespace Radio.Wpf.Pages
{
    public class PinItem
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public FileResult? PathType { get; set; }
        public Visibility Visibility { get; set; }
    }

    public partial class Player
    {
        public static ListBox PinnedItems;
        public static TextBlock Volume;
        public Player()
        {
            InitializeComponent();

            Resources.MergedDictionaries.Clear();
            var themeResources =
                Application.LoadComponent(new Uri("Expression.xaml", UriKind.Relative)) as ResourceDictionary;
            Resources.MergedDictionaries.Add(themeResources);

            var soundEngine = Instance;
            soundEngine.PropertyChanged += NAudioEngine_PropertyChanged;

            SpectrumAnalyzer1.RegisterSoundPlayer(soundEngine);
            SpectrumAnalyzer2.RegisterSoundPlayer(soundEngine);

            ContentArea.Content = new UnderConstruction();

            PinnedItems = _PinnedItems;
            PinnedItems.ItemsSource = new List<PinItem>();

            Volume = _Volume;

            ((INotifyCollectionChanged) PinnedItems.Items).CollectionChanged += PinnedItemsCollectionChanged;

            Instance.CanOpen = true;
        }

        #region NAudio Engine Events

        private void NAudioEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var engine = Instance;
            switch (e.PropertyName)
            {
                case "FileTag":

                    if (engine.FileTag != null)
                    {
                        Title.Text = Path.GetFileNameWithoutExtension(engine.FileTag.Name);

                        if (!string.IsNullOrWhiteSpace(engine.FileTag.Tag.Title)) Title.Text = engine.FileTag.Tag.Title;
                    }

                    break;

                case "ChannelPosition":

                    if (double.IsNaN(engine.ChannelPosition)) return;

                    var currentPosition = TimeSpan.FromMilliseconds(engine.ChannelPosition);
                    CurrentTime.Text = TimeSpan.FromSeconds(Math.Floor(currentPosition.TotalMilliseconds))
                        .ToString(@"mm\:ss");
                    Seekbar.Value = (int) TimeSpan.FromSeconds(Math.Floor(currentPosition.TotalMilliseconds))
                        .TotalSeconds;

                    if (Math.Abs(Seekbar.Value - Seekbar.Maximum) < 1)
                    {
                        engine.ActiveStream.Position = 0;

                        if (!engine.CanPause) return;

                        if (Repeat.IsChecked == false)
                        {
                            PausePlayIcon.Kind = PackIconKind.PlayCircleOutline;

                            engine.Pause();
                        }
                    }

                    break;

                case "ChannelLength":

                    if (double.IsNaN(engine.ChannelLength)) return;

                    var maxPosition = TimeSpan.FromMilliseconds(engine.ChannelLength);
                    Length.Text = TimeSpan.FromSeconds(Math.Floor(maxPosition.TotalMilliseconds)).ToString(@"mm\:ss");
                    Seekbar.Maximum =
                        (int) TimeSpan.FromSeconds(Math.Floor(maxPosition.TotalMilliseconds)).TotalSeconds;

                    break;

                case "Play":

                    PausePlayIcon.Kind = PackIconKind.PauseCircleOutline;
                    StopButton.IsEnabled = true;

                    break;

                case "Pause":

                    PausePlayIcon.Kind = PackIconKind.PlayCircleOutline;

                    break;

                case "Stop":

                    ContentArea.Content = new UnderConstruction();

                    PausePlayIcon.Kind = PackIconKind.PlayCircleOutline;

                    Title.Text = "RH Radio";
                    Seekbar.Minimum = 0;
                    Seekbar.Maximum = 0;
                    Seekbar.Value = 0;

                    Length.Text = "00:00";
                    CurrentTime.Text = "00:00";

                    canPin = false;
                    Pin.IsChecked = false;
                    canPin = true;

                    StopButton.IsEnabled = false;

                    break;

                case "Content":

                    canPin = false;

                    Pin.IsChecked = false;

                    foreach (var item in (List<PinItem>) PinnedItems.ItemsSource)
                        if (item.Path == Instance.Path)
                            Pin.IsChecked = true;
                    canPin = true;

                    switch (Instance.PathType)
                    {
                        case FileResult.File:
                            ContentArea.Content = new FileInfo();
                            break;

                        case FileResult.Stream:
                            Title.Text = GetFilenameFromWebServer(Instance.Path);
                            ContentArea.Content = new UnderConstruction();
                            break;
                    }

                    break;
            }
        }

        public static string GetFilenameFromWebServer(string url)
        {
            var result = "";

            var req = WebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                if (string.IsNullOrEmpty(resp.Headers["Content-Disposition"])) return result;
                result = resp.Headers["Content-Disposition"]
                    .Substring(resp.Headers["Content-Disposition"].IndexOf("filename=", StringComparison.Ordinal) + 9)
                    .Replace("\"", "");

                result = Path.GetFileNameWithoutExtension(result);
            }

            return result;
        }

        #endregion NAudio Engine Events

        #region Player Engine

        private void OpenFile()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Musik Dateien (*.mp3)|*.mp3|Online Musik [Testzweck] (*.radio)|*.radio"
            };
            if (openDialog.ShowDialog() != true) return;
            Instance.Stop();
            Instance.OpenFile(openDialog.FileName);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void PausePlay_Click(object sender, RoutedEventArgs e)
        {
            if (Instance.IsPlaying && Instance.CanPause)
                Instance.Pause();
            else if (!Instance.IsPlaying && Instance.CanPlay)
                Instance.Play();
            else if (!Instance.CanPlay) OpenFile();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Instance.Stop();
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            Instance.Stop();

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

            Instance.OpenFile(files?[0]);
        }

        private void Seekbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Instance.ChannelPosition = Seekbar.Value;
        }

        private void Repeat_Checked(object sender, RoutedEventArgs e)
        {
            var palette = new PaletteHelper().QueryPalette();
            var hue = palette.PrimarySwatch.PrimaryHues.ToArray()[palette.PrimaryDarkHueIndex];

            RepeatIcon.Foreground = new SolidColorBrush(hue.Color);
        }

        private void Repeat_Unchecked(object sender, RoutedEventArgs e)
        {
            RepeatIcon.Foreground = Brushes.White;
        }

        private void OpenPath_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Instance.Path))
                Process.Start(Directory.GetParent(Instance.Path).ToString());
            else if (!string.IsNullOrEmpty(Instance.Path)) Process.Start(Instance.Path);
        }

        #endregion Player Engine

        #region Pins

        private bool canPin = true;

        private void Pin_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Instance.Path) || Instance.PathType == null)
            {
                canPin = false;
                Pin.IsChecked = false;
                canPin = true;
                return;
            }

            var palette = new PaletteHelper().QueryPalette();
            var hue = palette.PrimarySwatch.PrimaryHues.ToArray()[palette.PrimaryDarkHueIndex];

            PinIcon.Foreground = new SolidColorBrush(hue.Color);

            if (!canPin) return;

            var items = (List<PinItem>) PinnedItems.ItemsSource;

            items.Add(new PinItem {Title = Title.Text, Path = Instance.Path, PathType = Instance.PathType});

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TupleConverter());

            foreach (var item in items)
            {
                item.Title = item.Title.Replace("'", "&apos;");
                item.Path = item.Path.Replace("'", "&apos;");
            }

            var json = JsonConvert.SerializeObject(items, settings).Replace("\"", "'");

            SettingsHelper.ChangeValue("pinned", json);
        }

        private void Pin_Unchecked(object sender, RoutedEventArgs e)
        {
            PinIcon.Foreground = Brushes.White;

            if (!canPin) return;

            var items = (List<PinItem>) PinnedItems.ItemsSource;

            for (var i = items.Count - 1; i >= 0; i--)
                if (items[i].Title == Title.Text)
                    items.RemoveAt(i);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TupleConverter());

            foreach (var item in items)
            {
                item.Title = item.Title.Replace("'", "&apos;");
                item.Path = item.Path.Replace("'", "&apos;");
            }

            SettingsHelper.ChangeValue("pinned", JsonConvert.SerializeObject(items, settings).Replace("\"", "'"));
        }

        private static void SetCover(Image icon, TextBlock title)
        {
            foreach (var item in (List<PinItem>) PinnedItems.ItemsSource)
                if (item.Title == title.Text)
                {
                    var path = item.Path;

                    if (item.PathType == FileResult.File)
                    {
                        var fileTag = TagLib.File.Create(path);

                        if (fileTag == null) break;

                        var tag = fileTag.Tag;

                        if (tag.Pictures.Length > 0)
                        {
                            using (var albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data))
                            {
                                try
                                {
                                    var albumImage = new BitmapImage();
                                    albumImage.BeginInit();
                                    albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                    albumImage.StreamSource = albumArtworkMemStream;
                                    albumImage.EndInit();
                                    icon.Source = albumImage;

                                    icon.Stretch = Stretch.Uniform;

                                    return;
                                }
                                catch (NotSupportedException)
                                {
                                    icon.Stretch = Stretch.None;
                                    icon.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png",
                                        UriKind.Relative));
                                }

                                albumArtworkMemStream.Close();
                            }
                        }
                        else
                        {
                            icon.Stretch = Stretch.None;
                            icon.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png",
                                UriKind.Relative));
                        }
                    }
                    else if (item.PathType == FileResult.Stream)
                    {
                        icon.Stretch = Stretch.None;

                        var uri = new Uri("/Radio;component/Assets/cloud.png", UriKind.Relative);
                        var source = new BitmapImage();

                        source.BeginInit();
                        source.UriSource = uri;
                        source.DecodePixelHeight = 20;
                        source.DecodePixelWidth = 20;
                        source.EndInit();

                        icon.Source = source;
                    }
                    else
                    {
                        icon.Stretch = Stretch.None;
                        icon.Source =
                            new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));
                    }
                }
        }

        private void Chip_Loaded(object sender, RoutedEventArgs e)
        {
            var chip = (Chip) sender;
            var title = (TextBlock) chip.Content;
            var icon = (Image) chip.Icon;

            SetCover(icon, title);
        }

        private void Chip_MouseEnter(object sender, MouseEventArgs e)
        {
            var chip = (Chip) sender;
            var icon = (Image) chip.Icon;

            icon.Stretch = Stretch.Uniform;
            icon.Source = new BitmapImage(new Uri("/Radio;component/Assets/Play.png", UriKind.Relative));
        }

        private void Chip_MouseLeave(object sender, MouseEventArgs e)
        {
            var chip = (Chip) sender;
            var title = (TextBlock) chip.Content;
            var icon = (Image) chip.Icon;

            SetCover(icon, title);
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            var chip = (Chip) sender;
            var title = (TextBlock) chip.Content;

            foreach (var item in (List<PinItem>) PinnedItems.ItemsSource)
                if (item.Title == title.Text)
                {
                    Instance.Stop();
                    Instance.OpenFile(item.Path);
                }
        }

        private void Chip_DeleteClick(object sender, RoutedEventArgs e)
        {
            var chip = (Chip) sender;
            var title = (TextBlock) chip.Content;

            if (title.Text == Title.Text)
            {
                Pin.IsChecked = false;
            }
            else
            {
                var items = (List<PinItem>) PinnedItems.ItemsSource;

                for (var i = items.Count - 1; i >= 0; i--)
                    if (items[i].Title == title.Text)
                        items.RemoveAt(i);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new TupleConverter());

                foreach (var item in items)
                {
                    item.Title = item.Title.Replace("'", "&apos;");
                    item.Path = item.Path.Replace("'", "&apos;");
                }

                SettingsHelper.ChangeValue("pinned", JsonConvert.SerializeObject(items, settings).Replace("\"", "'"));
            }
        }

        public void PinnedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var items = (List<PinItem>) PinnedItems.ItemsSource;

            if (items == null) return;

            NoPins.Visibility = items.Count <= 0 ? Visibility.Visible : Visibility.Hidden;

            canPin = false;

            Pin.IsChecked = false;

            foreach (var item in (List<PinItem>) PinnedItems.ItemsSource)
            {
                if (item.PathType != FileResult.File && item.PathType != FileResult.Stream)
                    item.Visibility = Visibility.Collapsed;

                if (item.Path == Instance.Path) Pin.IsChecked = true;
            }

            canPin = true;
        }

        #endregion Pins

        #region Volume

        private void VolumePlus_Click(object sender, RoutedEventArgs e)
        {
            var vol = Convert.ToInt32(Volume.Text.Remove(Volume.Text.Length - 1)) + 1;

            if (vol > 200) return;

            Instance.Volume = (float)vol / 100;
        }

        private void VolumeMinus_Click(object sender, RoutedEventArgs e)
        {
            var vol = Convert.ToInt32(Volume.Text.Remove(Volume.Text.Length - 1)) - 1;

            if (vol < 0) return;

            Instance.Volume = (float)vol / 100;
        }

        private void Volume_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var vol = Convert.ToInt32(Volume.Text.Remove(Volume.Text.Length - 1));

            if (e.Delta < 0)
            {
                --vol;

                if (vol < 0) return;
            }
            else if (e.Delta > 0)
            {
                ++vol;

                if (vol > 200) return;
            }

            Instance.Volume = (float)vol / 100;
        }

        private void Volume_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var vol = (float) Convert.ToInt32(Volume.Text.Remove(Volume.Text.Length - 1)) / 100;

            SettingsHelper.ChangeValue("Volume", vol.ToString("0.00"));
        }

        #endregion
    }
}