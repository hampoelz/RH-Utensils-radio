using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using Radio.Wpf.Utilities;

namespace Radio.Wpf.Modules
{
    public partial class FileInfo
    {
        public FileInfo()
        {
            InitializeComponent();

            var soundEngine = AudioPlayer.Instance;
            soundEngine.PropertyChanged += NAudioEngine_PropertyChanged;
        }

        #region NAudio Engine Events

        private void NAudioEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var engine = AudioPlayer.Instance;
            switch (e.PropertyName)
            {
                case "FileTag":
                    if (engine.FileTag != null)
                    {
                        var tag = engine.FileTag.Tag;

                        Title.Text = tag.Title;
                        Artists.Text = string.Join(", ", tag.Performers);
                        Album.Text = tag.Album;
                        Genre.Text = string.Join(", ", tag.Genres);
                        Track.Text = tag.Track.ToString();
                        Year.Text = tag.Year.ToString();
                        Bitrate.Text = engine.FileTag.Properties.AudioBitrate + " kBit/s";
                        Copyright.Text = tag.Copyright;

                        if (tag.Pictures.Length > 0)
                            using (var albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data))
                            {
                                try
                                {
                                    var albumImage = new BitmapImage();
                                    albumImage.BeginInit();
                                    albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                    albumImage.StreamSource = albumArtworkMemStream;
                                    albumImage.EndInit();
                                    Cover.Source = albumImage;
                                }
                                catch (NotSupportedException)
                                {
                                    Cover.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png",
                                        UriKind.Relative));
                                }

                                albumArtworkMemStream.Close();
                            }
                        else
                            Cover.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png",
                                UriKind.Relative));
                    }
                    else
                    {
                        Cover.Source =
                            new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));
                    }

                    break;

                case "Stop":

                    Title.Text = "";
                    Artists.Text = "";
                    Album.Text = "";
                    Genre.Text = "";
                    Track.Text = "";
                    Year.Text = "";
                    Bitrate.Text = "";
                    Copyright.Text = "";

                    Cover.Source = new BitmapImage(new Uri("/Radio;component/Assets/music-note.png", UriKind.Relative));

                    break;
            }
        }

        #endregion NAudio Engine Events
    }
}