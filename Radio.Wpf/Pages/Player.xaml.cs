using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Radio.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Radio.Wpf.Pages
{
    public class PinItem
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public Visibility Visibility { get; set; }
    }

    public partial class Player : Page
    {
        public static bool playMusik;

        public static ListBox Pins;

        public static TextBlock FileName = new TextBlock();

        public static Button SkipPreviousButton = new Button();
        public static PackIcon SkipPreviousIcon = new PackIcon();

        public static Button PausePlayButton = new Button();
        public static PackIcon PausePlayIcon = new PackIcon();

        public static Button StopButton = new Button();
        public static PackIcon StopIcon = new PackIcon();

        public static Button SkipNextButton = new Button();
        public static PackIcon SkipNextIcon = new PackIcon();

        public Player()
        {
            InitializeComponent();

            LoadExpressionTheme();

            AudioPlayer soundEngine = AudioPlayer.Instance;
            soundEngine.PropertyChanged += NAudioEngine_PropertyChanged;

            spectrumAnalyzer1.RegisterSoundPlayer(soundEngine);
            spectrumAnalyzer2.RegisterSoundPlayer(soundEngine);

            Pins = PinsTemplate;
            Pins.Visibility = Visibility;
            Pins.ItemsSource = new List<PinItem>();

            ((INotifyCollectionChanged)Pins.Items).CollectionChanged += PinsCollectionChanged;

            FileName.Foreground = Brushes.White;
            FileName.TextAlignment = TextAlignment.Center;
            FileName.VerticalAlignment = VerticalAlignment.Center;
            FileName.HorizontalAlignment = HorizontalAlignment.Stretch;
            FileName.TextTrimming = TextTrimming.CharacterEllipsis;
            FileName.FontSize = 30;
            FileName.FontWeight = FontWeights.Bold;
            FileName.FontFamily = new FontFamily("Roboto Black");
            FileName.Text = "RH Radio";
            FileName.ToolTip = FileName.Text;

            NamePlaceholder.Children.Add(FileName);

            CreateToolbarButton(SkipPreviousButton, SkipPreviousIcon, PackIconKind.SkipPreviousCircleOutline);
            CreateToolbarButton(PausePlayButton, PausePlayIcon, PackIconKind.PlayCircleOutline);
            CreateToolbarButton(StopButton, StopIcon, PackIconKind.StopCircleOutline);
            CreateToolbarButton(SkipNextButton, SkipNextIcon, PackIconKind.SkipNextCircleOutline);

            PausePlayButton.Click += PausePlay_Click;
            StopButton.Click += Stop_Click;

            SkipPreviousButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            SkipNextButton.IsEnabled = false;

            playMusik = true;
        }

        private void CreateToolbarButton(Button btn, PackIcon pIcon, PackIconKind pIcoKind)
        {
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.Width = Double.NaN;
            btn.Height = Double.NaN;

            btn.Style = FindResource("MaterialDesignToolButton") as Style;

            btn.Foreground = Brushes.White;

            btn.Content = pIcon;

            pIcon.Kind = pIcoKind;
            pIcon.Height = Double.NaN;
            pIcon.Width = 30;
            pIcon.VerticalContentAlignment = VerticalAlignment.Center;
            pIcon.HorizontalContentAlignment = HorizontalAlignment.Center;

            Toolbar.Children.Add(btn);
        }

        private void LoadExpressionTheme()
        {
            Resources.MergedDictionaries.Clear();
            ResourceDictionary themeResources = Application.LoadComponent(new Uri("Expression.xaml", UriKind.Relative)) as ResourceDictionary;
            Resources.MergedDictionaries.Add(themeResources);
        }

        #region NAudio Engine Events

        private void NAudioEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AudioPlayer engine = AudioPlayer.Instance;
            switch (e.PropertyName)
            {
                case "FileTag":
                    if (engine.FileTag != null)
                    {
                        TagLib.Tag tag = engine.FileTag.Tag;

                        FileName.Text = Path.GetFileNameWithoutExtension(engine.FileTag.Name);

                        Title.Text = tag.Title;
                        Artists.Text = string.Join(", ", tag.Performers);
                        Album.Text = tag.Album;
                        Genre.Text = string.Join(", ", tag.Genres);
                        Track.Text = tag.Track.ToString();
                        Year.Text = tag.Year.ToString();
                        Bitrate.Text = engine.FileTag.Properties.AudioBitrate.ToString() + " kBit/s";
                        Copyright.Text = tag.Copyright;

                        if (!string.IsNullOrWhiteSpace(tag.Title))
                        {
                            FileName.Text = tag.Title;
                        }

                        FileName.ToolTip = FileName.Text;

                        if (tag.Pictures.Length > 0)
                        {
                            using (MemoryStream albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data))
                            {
                                try
                                {
                                    BitmapImage albumImage = new BitmapImage();
                                    albumImage.BeginInit();
                                    albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                    albumImage.StreamSource = albumArtworkMemStream;
                                    albumImage.EndInit();
                                    Cover.Source = albumImage;
                                }
                                catch (NotSupportedException)
                                {
                                    Cover.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));
                                }
                                albumArtworkMemStream.Close();
                            }
                        }
                        else
                        {
                            Cover.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));
                        }
                    }
                    else
                    {
                        Cover.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));
                    }
                    break;

                case "ChannelPosition":
                    if (double.IsNaN(engine.ChannelPosition)) return;

                    var currentPosition = TimeSpan.FromMilliseconds(engine.ChannelPosition);
                    currentTime.Text = TimeSpan.FromSeconds(Math.Floor(currentPosition.TotalMilliseconds)).ToString(@"mm\:ss");
                    Seekbar.Value = (int)TimeSpan.FromSeconds(Math.Floor(currentPosition.TotalMilliseconds)).TotalSeconds;

                    if (Seekbar.Value == Seekbar.Maximum)
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

                    if (App.Title != null) FileName.Text = App.Title;

                    FileName.ToolTip = FileName.Text;

                    _toPinItem = false;

                    Pin.IsChecked = false;

                    foreach (PinItem item in (List<PinItem>)Pins.ItemsSource)
                    {
                        if (item.Path == App.File)
                        {
                            Pin.IsChecked = true;
                        }
                    }
                    _toPinItem = true;

                    if (double.IsNaN(engine.ChannelLength)) return;

                    var maxPosition = TimeSpan.FromMilliseconds(engine.ChannelLength);
                    length.Text = TimeSpan.FromSeconds(Math.Floor(maxPosition.TotalMilliseconds)).ToString(@"mm\:ss");
                    Seekbar.Maximum = (int)TimeSpan.FromSeconds(Math.Floor(maxPosition.TotalMilliseconds)).TotalSeconds;

                    break;

                default:
                    // Do Nothing
                    break;
            }
        }

        #endregion NAudio Engine Events

        private void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Musik Dateien (*.mp3)|*.mp3|Online Musik [Testzweck] (*.radio)|*.radio"
            };
            if (openDialog.ShowDialog() == true)
            {
                StopAudioPlayer();
                App.File = openDialog.FileName;
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void PausePlay_Click(object sender, RoutedEventArgs e)
        {
            if (AudioPlayer.Instance.IsPlaying && AudioPlayer.Instance.CanPause)
            {
                PausePlayIcon.Kind = PackIconKind.PlayCircleOutline;

                AudioPlayer.Instance.Pause();
            }
            else if (!AudioPlayer.Instance.IsPlaying && AudioPlayer.Instance.CanPlay)
            {
                PausePlayIcon.Kind = PackIconKind.PauseCircleOutline;

                AudioPlayer.Instance.Play();
            }
            else if (!AudioPlayer.Instance.CanPlay)
            {
                OpenFile();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopAudioPlayer();
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            StopAudioPlayer();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                App.File = files[0];
            }
        }

        private void Seekbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (!AudioPlayer.Instance.CanPlay) return;

            if (!playMusik) return;

            AudioPlayer.Instance.ChannelPosition = Seekbar.Value;
        }

        private void StopAudioPlayer()
        {
            if (!AudioPlayer.Instance.CanStop) return;

            PausePlayIcon.Kind = PackIconKind.PlayCircleOutline;

            AudioPlayer.Instance.Stop();

            FileName.Text = "RH Radio";
            FileName.ToolTip = FileName.Text;
            Seekbar.Minimum = 0;
            Seekbar.Maximum = 0;
            Seekbar.Value = 0;

            length.Text = "00:00";
            currentTime.Text = "00:00";

            Title.Text = "";
            Artists.Text = "";
            Album.Text = "";
            Genre.Text = "";
            Track.Text = "";
            Year.Text = "";
            Bitrate.Text = "";
            Copyright.Text = "";

            Cover.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));

            _toPinItem = false;
            Pin.IsChecked = false;
            _toPinItem = true;

            StopButton.IsEnabled = false;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(App.File))
            {
                Process.Start(Directory.GetParent(App.File).ToString());
            }
            else if (!string.IsNullOrEmpty(App.File))
            {
                Process.Start(App.File);
            }
        }

        private void Chip_MouseEnter(object sender, MouseEventArgs e)
        {
            Chip chip = (Chip)sender;
            Image icon = (Image)chip.Icon;

            icon.Stretch = Stretch.Uniform;
            icon.Source = new BitmapImage(new Uri("/Radio;component/Assets/Play.png", UriKind.Relative));
        }

        private void Chip_MouseLeave(object sender, MouseEventArgs e)
        {
            Chip chip = (Chip)sender;
            TextBlock title = (TextBlock)chip.Content;
            Image icon = (Image)chip.Icon;

            SetCover(icon, title);
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            Chip chip = (Chip)sender;
            TextBlock title = (TextBlock)chip.Content;

            foreach (PinItem item in (List<PinItem>)Pins.ItemsSource)
            {
                if (item.Title == title.Text)
                {
                    StopAudioPlayer();
                    App.File = item.Path;
                }
            }
        }

        private void Chip_Loaded(object sender, RoutedEventArgs e)
        {
            Chip chip = (Chip)sender;
            TextBlock title = (TextBlock)chip.Content;
            Image icon = (Image)chip.Icon;

            SetCover(icon, title);
        }

        private void SetCover(Image icon, TextBlock title)
        {
            foreach (PinItem item in (List<PinItem>)Pins.ItemsSource)
            {
                if (item.Title == title.Text)
                {
                    var Path = item.Path;

                    if (!File.Exists(Path)) break;

                    var FileTag = TagLib.File.Create(Path);

                    if (FileTag == null) break;

                    TagLib.Tag tag = FileTag.Tag;

                    if (tag.Pictures.Length > 0)
                    {
                        using (MemoryStream albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data))
                        {
                            try
                            {
                                BitmapImage albumImage = new BitmapImage();
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
                                icon.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));
                            }
                            albumArtworkMemStream.Close();
                        }
                    }
                }
            }

            icon.Stretch = Stretch.None;
            icon.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));
        }

        private void Chip_DeleteClick(object sender, RoutedEventArgs e)
        {
            Chip chip = (Chip)sender;
            TextBlock title = (TextBlock)chip.Content;

            if (title.Text == FileName.Text)
            {
                Pin.IsChecked = false;
            }
            else
            {
                List<PinItem> items = (List<PinItem>)Pins.ItemsSource;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (items[i].Title == title.Text)
                    {
                        items.RemoveAt(i);
                    }
                }

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new TupleConverter());

                foreach (PinItem item in items)
                {
                    item.Title = item.Title.Replace("'", "&apos;");
                    item.Path = item.Path.Replace("'", "&apos;");
                }

                SettingsHelper.ChangeValue("pinned", JsonConvert.SerializeObject(items, settings).Replace("\"", "'"));
            }
        }

        public static bool _toPinItem = true;

        private void Pin_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(App.File))
            {
                _toPinItem = false;
                Pin.IsChecked = false;
                _toPinItem = true;
                return;
            }

            var palette = new PaletteHelper().QueryPalette();
            var hue = palette.PrimarySwatch.PrimaryHues.ToArray()[palette.PrimaryDarkHueIndex];

            PinIcon.Foreground = new SolidColorBrush(hue.Color);

            if (!_toPinItem) return;

            var items = (List<PinItem>)Pins.ItemsSource;

            items.Add(new PinItem() { Title = FileName.Text, Path = App.File });

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TupleConverter());

            foreach (PinItem item in items)
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

            if (!_toPinItem) return;

            var items = (List<PinItem>)Pins.ItemsSource;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].Title == FileName.Text)
                {
                    items.RemoveAt(i);
                }
            }

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TupleConverter());

            foreach (PinItem item in items)
            {
                item.Title = item.Title.Replace("'", "&apos;");
                item.Path = item.Path.Replace("'", "&apos;");
            }

            SettingsHelper.ChangeValue("pinned", JsonConvert.SerializeObject(items, settings).Replace("\"", "'"));
        }

        public void PinsCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            var items = (List<PinItem>)Pins.ItemsSource;

            if (items == null) return;

            if (items.Count <= 0)
            {
                NoPins.Visibility = Visibility.Visible;
            }
            else
            {
                NoPins.Visibility = Visibility.Hidden;
            }

            _toPinItem = false;

            Pin.IsChecked = false;

            foreach (PinItem item in (List<PinItem>)Pins.ItemsSource)
            {
                if (!File.Exists(item.Path))
                {
                    item.Visibility = Visibility.Collapsed;
                }

                if (item.Path == App.File)
                {
                    Pin.IsChecked = true;
                }
            }
            _toPinItem = true;
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                PausePlay_Click(sender, e);
            }
            else if (e.Key == Key.Right)
            {
                AudioPlayer.Instance.ChannelPosition += 10;
            }
            else if (e.Key == Key.Left)
            {
                AudioPlayer.Instance.ChannelPosition -= 10;
            }

            e.Handled = true;
        }
    }
}